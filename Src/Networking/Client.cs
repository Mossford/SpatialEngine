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
using SpatialEngine.Rendering;
using System.Collections.Generic;
using SpatialEngine.SpatialMath;

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
        float lastPing = 0f;

        // the other clients meshes, they only need position and rotation so a mesh is all that is needed
        public List<Mesh> playerMeshes;

        public SpatialClient()
        {
            Message.InstancesPerPeer = 100;
            playerMeshes = new List<Mesh>();
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
            PlayerJoinPacket playerJoinPacket = new PlayerJoinPacket(0, player.position, MathS.Vec3ToQuat(player.rotation), "Cube.obj");
            SendRelib(playerJoinPacket);
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
            if (!stopping && !disconnected)
            {
                /*for (int i = 0; i < scene.SpatialObjects.Count; i++)
                {
                    SpatialObjectPacket packet = new SpatialObjectPacket(i, scene.SpatialObjects[i].SO_mesh.position, scene.SpatialObjects[i].SO_mesh.rotation);
                    SendUnrelib(packet);
                }*/
                //send own player pos and rot to server
                PlayerPacket packet = new PlayerPacket(0, player.position, MathS.Vec3ToQuat(player.rotation));
                SendUnrelib(packet);
                client.Update();


                //get ping every 1 seconds and if nothing can be done disconnect from server as time out
                accu += deltaTime;
                while (accu >= 0.7f)
                {
                    accu -= 0.7f;
                    GetPingAsync();
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
            if (!stopping)
                HandlePacketClient(e.Message.GetBytes());
        }

        //gets the ping of the client and removes the delay caused by waiting 16ms for an update so a true ping can
        //be returned. With checking for if the ping is less than 0 which returns the ping before it
        public float GetPing()
        {
            if(currentPing - 0.0166f > 0f)
            {
                return currentPing - 0.0166f;
            }
            else
            {
                return lastPing - 0.0166f;
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
                    if(stopwatch.ElapsedMilliseconds / 1000 >= 1)
                    {
                        Console.WriteLine("Timed out: Could not ping server");
                        Disconnect();
                        break;
                    }
                }
                stopwatch.Stop();
                float timeEnd = Globals.GetTime();
                if (currentPing - 0.0166f > 0f)
                    lastPing = currentPing;
                currentPing = timeEnd - timeStart;
                pingCount++;
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
                        //will set the mesh now that physics will run on the server
                        scene.SpatialObjects[packet.id].SO_mesh.position = packet.Position;
                        scene.SpatialObjects[packet.id].SO_mesh.rotation = packet.Rotation;
                        stream.Close();
                        reader.Close();
                        break;
                    }
                case (ushort)PacketType.SpawnSpatialObject:
                    {
                        SpawnSpatialObjectPacket packet = new SpawnSpatialObjectPacket();
                        packet.ByteToPacket(data);
                        if (packet.id < scene.SpatialObjects.Count)
                        {
                            bodyInterface.DestroyBody(scene.SpatialObjects[packet.id].SO_rigidbody.rbID);
                            scene.SpatialObjects[packet.id] = new SpatialObject(LoadModel(packet.Position, packet.Rotation, packet.ModelLocation), (MotionType)packet.MotionType, (ObjectLayer)packet.ObjectLayer, (Activation)packet.Activation, (uint)packet.id);
                        }
                        else
                        {
                            scene.AddSpatialObject(LoadModel(packet.Position, packet.Rotation, packet.ModelLocation), (MotionType)packet.MotionType, (ObjectLayer)packet.ObjectLayer, (Activation)packet.Activation);
                        }
                        stream.Close();
                        reader.Close();
                        break;
                    }
                case (ushort)PacketType.SceneSyncStart:
                    {
                        SceneSyncClear packet = new SceneSyncClear();
                        SendRelib(packet);
                        break;
                    }
                case (ushort)PacketType.Player:
                    {
                        PlayerPacket packet = new PlayerPacket();
                        packet.ByteToPacket(data);
                        if(packet.id < playerMeshes.Count)
                        {
                            playerMeshes[packet.id].position = packet.Position;
                            playerMeshes[packet.id].rotation = packet.Rotation;
                        }
                        break;
                    }
                case (ushort)PacketType.PlayerJoin:
                    {
                        PlayerJoinPacket packet = new PlayerJoinPacket();
                        //hardcoded mesh location for now as using the packet causes it not to find the mesh
                        playerMeshes.Add(LoadModel(packet.Position, packet.Rotation, "Cube.obj"));
                        break;
                    }
            }
        }

        public Connection GetConnection() => client.Connection;
        public bool IsConnected() => client.IsConnected;
    }
}