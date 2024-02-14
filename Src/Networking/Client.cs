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
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace SpatialEngine.Networking
{
    public class SpatialClient
    {
        public static Client client;

        public ushort connectPort;
        public string connectIp;

        bool disconnected;
        bool stopping;

        bool waitPing = false;

        public float currentPing { get; protected set; }
        public int pingCount { get; protected set; } = 0;
        int pingCountSecret = 0;
        float pingTotal = 0f;

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
        }

        public void Connect(string ip, ushort port)
        {
            client.Connect($"{ip}:{port}", 5, 0, null, false);
            connectIp = ip;
            connectPort = port;

            ConnectPacket connectPacket = new ConnectPacket();
            SendRelib(connectPacket);
            disconnected = false;
        }

        public void Disconnect() 
        {
            client.Disconnect();
            disconnected = true;
        }

        static float accu = 0f;
        public void Update(float deltaTime)
        {
            if (!stopping || disconnected)
            {
                for (int i = 0; i < scene.SpatialObjects.Count; i++)
                {
                    SpatialObjectPacket packet = new SpatialObjectPacket(i, scene.SpatialObjects[i].SO_mesh.position, scene.SpatialObjects[i].SO_mesh.rotation);
                    SendUnrelib(packet);
                }
                client.Update();


                //get ping every 1 seconds and if nothing can be done disconnect from server as time out
                accu += deltaTime;
                while (accu >= 0.7f)
                {
                    accu -= 0.7f;
                    GetPingAsync();
                    pingTotal += currentPing;
                }
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
            if(client.IsConnected || !stopping)
            {
                Message msgUnrelib = Message.Create(MessageSendMode.Unreliable, packet.GetPacketType());
                msgUnrelib.AddBytes(packet.ConvertToByte());
                client.Send(msgUnrelib);
            }
        }

        //calling this a lot causes null error on the message create
        public void SendRelib(Packet packet)
        {
            if (client.IsConnected || !stopping)
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
            client = null;
        }

        public void handleMessage(object sender, MessageReceivedEventArgs e)
        {
            if(!stopping)
                HandlePacketClient(e.Message.GetBytes());
        }

        public float GetPing()
        {
            //start of getting the ping so we return the current ping as an average will mess the values
            if(pingCountSecret <= 5)
            {
                return currentPing;
            }
            //can now average as there should be sufficent data for averaging the ping
            else
            {
                //check if the pingCount is greater than 15 as we dont want to average too much
                if(pingCount > 15)
                {
                    pingCount = 1;
                    pingTotal = currentPing;
                    return currentPing;
                }
                //otherwise we are good to average the ping to get a somewhat good ping value
                else
                {
                    return pingTotal / pingCount;
                }
            }
        }

        async Task GetPingAsync()
        {
            await Task.Run(() => 
            {
                float timeStart = Globals.GetTime();
                PingPacket packet = new PingPacket();
                SendRelib(packet);
                waitPing = true;
                float accum = 0f;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while(waitPing)
                {
                    //stop checking for ping after 15 seconds
                    if(stopwatch.ElapsedMilliseconds / 1000 >= 15)
                    {
                        Console.WriteLine("Timed out: Could not ping server");
                        Disconnect();
                        break;
                    }
                }
                stopwatch.Stop();
                float timeEnd = Globals.GetTime();
                currentPing = timeEnd - timeStart;
                pingCount++;
                pingCountSecret++;
            });
        }

        //Handles packets that come from the server

        void HandlePacketClient(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            //data sent is not a proper packet
            if (data.Length < 2)
                return;

            //packet type
            ushort type = reader.ReadUInt16();

            switch (type)
            {
                case (ushort)PacketType.Pong:
                    {
                        waitPing = false;
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
                        scene.AddSpatialObject(LoadModel(packet.Position, packet.Rotation, SpatialEngine.Resources.ModelPath, packet.ModelLocation), (MotionType)packet.MotionType, (ObjectLayer)packet.ObjectLayer, (Activation)packet.Activation);
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