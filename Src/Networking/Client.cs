using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Riptide;
using Riptide.Transports;
using Riptide.Utils;

using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Globals;
using JoltPhysicsSharp;
using System.IO;

namespace SpatialEngine.Networking
{
    public class SpatialClient
    {
        public static Client client;
        public static Thread clientThread;

        public ushort connectPort;
        public string connectIp;

        bool stopping;

        public SpatialClient()
        {
            Message.InstancesPerPeer = 100;
        }

        public void Start(string ip, ushort port)
        {
            connectIp = ip;
            connectPort = port;
            client = new Client();
            client.Connected += Connected;
            client.Disconnected += Disconnected;
            client.MessageReceived += handleMessage;
            Connect(connectIp, connectPort);
            clientThread = new Thread(Update);
            clientThread.Start();
        }

        public void Connect(string ip, ushort port)
        {
            client.Connect($"{ip}:{port}", 5, 0, null, false);
            connectIp = ip;
            connectPort = port;
        }

        public void Disconnect() 
        {
            client.Disconnect();
        }

        public void Update()
        {
            ConnectPacket connectPacket = new ConnectPacket();
            SendRelib(connectPacket);

            while (true)
            {
                if (stopping)
                    return;

                for (int i = 0; i < scene.SpatialObjects.Count; i++)
                {
                    SpatialObjectPacket packet = new SpatialObjectPacket(i, scene.SpatialObjects[i].SO_mesh.position, scene.SpatialObjects[i].SO_mesh.rotation);
                    SendUnrelib(packet);
                }

                client.Update();
            }
        }

        void Connected(object sender, EventArgs e)
        {
               
        }

        void Disconnected(object sender, EventArgs e)
        {
            connectIp = "";
            connectPort = 0;
        }

        public void SendUnrelib(Packet packet)
        {
            if(client.IsConnected)
            {
                Message msgUnrelib = Message.Create(MessageSendMode.Unreliable, packet.GetPacketType());
                msgUnrelib.AddBytes(packet.ConvertToByte());
                client.Send(msgUnrelib);
            }
        }

        //calling this a lot causes null error on the message create
        public void SendRelib(Packet packet)
        {
            if (client.IsConnected)
            {
                Message msgRelib = Message.Create(MessageSendMode.Reliable, packet.GetPacketType());
                msgRelib.AddBytes(packet.ConvertToByte());
                client.Send(msgRelib);
            }
        }

        public void Close()
        {
            client.Disconnect();
            stopping = true;
            clientThread.Interrupt();
            client = null;
            clientThread = null;
        }

        public void handleMessage(object sender, MessageReceivedEventArgs e)
        {
            HandlePacketClient(e.Message.GetBytes());
        }

        //Handles packets that come from the server

        void HandlePacketClient(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            //packet type
            ushort type = reader.ReadUInt16();

            switch (type)
            {
                case (ushort)PacketType.Pong:
                    {

                        break;
                    }
                case (ushort)PacketType.ConnectReturn:
                    {
                        ConnectReturnPacket packet = new ConnectReturnPacket();
                        packet.ByteToPacket(data);
                        Console.WriteLine("Server version is: " + packet.engVersion + " Client version is: " + EngVer);
                        break;
                    }
                case (ushort)PacketType.SpatialObject:
                    {
                        SpatialObjectPacket packet = new SpatialObjectPacket();
                        packet.ByteToPacket(data);
                        if (packet.id >= scene.SpatialObjects.Count)
                            break;
                        scene.SpatialObjects[packet.id].SO_rigidbody.SetPosition((Double3)packet.Position);
                        scene.SpatialObjects[packet.id].SO_rigidbody.SetRotation(packet.Rotation);
                        stream.Close();
                        reader.Close();
                        break;
                    }
                case (ushort)PacketType.SpawnSpatialObject:
                    {
                        SpawnSpatialObjectPacket packet = new SpawnSpatialObjectPacket();
                        packet.ByteToPacket(data);
                        scene.AddSpatialObject(LoadModel(packet.Position, packet.Rotation, packet.ModelLocation), (MotionType)packet.MotionType, (ObjectLayer)packet.ObjectLayer, (Activation)packet.Activation);
                        stream.Close();
                        reader.Close();
                        break;
                    }
            }
        }

        public Connection GetConnection() => client.Connection;
        public bool IsConnected() => client.IsConnected;
    }
}