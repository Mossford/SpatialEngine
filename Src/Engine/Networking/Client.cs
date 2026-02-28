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
using System.Numerics;
using SpatialEngine.Networking.Packets;
using SpatialEngine.SpatialMath;

namespace SpatialEngine.Networking
{
    public static class SpatialClient
    {
        public static Client client;

        public static ushort connectPort;
        public static string connectIp;

        static bool disconnected;
        static bool stopping;

        static bool waitPing = false;
        
        static Action<byte[]> handleServerPacket;
        public static event EventHandler ClientConnect;
        public static event EventHandler ClientDisconnect;
        public static event EventHandler OtherClientConnected;
        public static event EventHandler OtherClientDisconnected;

        public static float currentPing { get; set; }
        public static int pingCount { get; set; } = 0;
        static float lastPing = 0f;

        public static List<ClientPlayer> joinedPlayers;
        
        public static void SetServerHandle(Action<byte[]> action)
        {
            handleServerPacket = action;
        }
        
        public static void Init()
        {
            Message.InstancesPerPeer = 100;
            joinedPlayers = new List<ClientPlayer>();
            client = new Client();
            client.Connected += Connected;
            client.Disconnected += Disconnected;
            client.MessageReceived += handleMessage;
        }

        public static void Connect(string ip, ushort port)
        {
            if (!client.Connect($"{ip}:{port}", 5, 0, null, false))
            {
                return;
            }
            connectIp = ip;
            connectPort = port;

            ConnectPacket connectPacket = new ConnectPacket();
            SendRelib(connectPacket);
            PlayerJoinPacket playerJoinPacket = new PlayerJoinPacket(0, player.position, MathS.Vec3ToQuat(player.rotation));
            SendRelib(playerJoinPacket);
            disconnected = false;
        }

        public static void Disconnect() 
        {
            client.Disconnect();
            disconnected = true;
        }

        static float accu = 0f;
        public static void Update(float deltaTime)
        {
            if (!stopping && !disconnected)
            {
                /*for (int i = 0; i < scene.SpatialObjects.Count; i++)
                {
                    SpatialObjectPacket packet = new SpatialObjectPacket(i, scene.SpatialObjects[i].SO_mesh.position, scene.SpatialObjects[i].SO_mesh.rotation);
                    SendUnrelib(packet);
                }*/
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

        static void Connected(object sender, EventArgs e)
        {
            ClientConnect?.Invoke(null, EventArgs.Empty);
        }

        static void Disconnected(object sender, EventArgs e)
        {
            connectIp = "";
            connectPort = 0;
            ClientDisconnect?.Invoke(null, EventArgs.Empty);
        }

        public static void SendUnrelib(Packet packet)
        {
            if(client.IsConnected || !stopping)
            {
                Message msgUnrelib = Message.Create(MessageSendMode.Unreliable, packet.GetPacketType());
                msgUnrelib.AddBytes(packet.ConvertToByte());
                client.Send(msgUnrelib);
            }
        }

        //calling this a lot causes null error on the message create
        public static void SendRelib(Packet packet)
        {
            if (client is not null && (client.IsConnected || !stopping))
            {
                Message msgRelib = Message.Create(MessageSendMode.Reliable, packet.GetPacketType());
                msgRelib.AddBytes(packet.ConvertToByte());
                client.Send(msgRelib);
            }
        }

        public static void Close()
        {
            client.Disconnect();
            stopping = true;
            client = null;
        }

        public static void handleMessage(object sender, MessageReceivedEventArgs e)
        {
            if (!stopping)
                HandlePacketClient(e.Message.GetBytes());
        }

        //gets the ping of the client and removes the delay caused by waiting 16ms for an update so a true ping can
        //be returned. With checking for if the ping is less than 0 which returns the ping before it
        public static float GetPing()
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

        static async Task GetPingAsync()
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

        static void HandlePacketClient(byte[] data)
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
                        if (packet.id >= currentScene.SpatialObjects.Count)
                            break;
                        //will set the mesh now that physics will run on the server
                        currentScene.SpatialObjects[packet.id].rigidbody.SetPosition(packet.Position);
                        currentScene.SpatialObjects[packet.id].rigidbody.SetRotation(packet.Rotation);
                        currentScene.SpatialObjects[packet.id].rigidbody.SetVelocity(packet.Velocity);
                        currentScene.SpatialObjects[packet.id].rigidbody.SetAngularVelocity(packet.AngVelocity);
                        currentScene.SpatialObjects[packet.id].enabled = packet.Enabled;
                        stream.Close();
                        reader.Close();
                        break;
                    }
                case (ushort)PacketType.SpawnSpatialObject:
                    {
                        SpawnSpatialObjectPacket packet = new SpawnSpatialObjectPacket();
                        packet.ByteToPacket(data);
                        if (packet.id < currentScene.SpatialObjects.Count)
                        {
                            currentScene.SpatialObjects[packet.id].rigidbody.RemoveFromPhysics();
                            currentScene.SpatialObjects[packet.id] = new SpatialObject(
                                LoadModel(packet.Position, 
                                    packet.Rotation, 
                                    packet.ModelLocation, 
                                    packet.Scale), 
                                (MotionType)packet.MotionType, 
                                (ObjectLayer)packet.ObjectLayer, 
                                (Activation)packet.Activation, 
                                currentScene.SpatialObjects[packet.id].id);
                            currentScene.SpatialObjects[packet.id].enabled = packet.Enabled;
                            currentScene.SpatialObjects[packet.id].texture = TextureManager.RetrieveTexture(packet.TextureLocation);
                            currentScene.SpatialObjects[packet.id].rigidbody.SetVelocity(packet.Velocity);
                            currentScene.SpatialObjects[packet.id].rigidbody.SetAngularVelocity(packet.AngVelocity);
                        }
                        else
                        {
                            currentScene.AddSpatialObject(
                                LoadModel(packet.Position, 
                                    packet.Rotation, 
                                    packet.ModelLocation, 
                                    packet.Scale), 
                                (MotionType)packet.MotionType, 
                                (ObjectLayer)packet.ObjectLayer, 
                                (Activation)packet.Activation);
                            currentScene.SpatialObjects[^1].enabled = packet.Enabled;
                            currentScene.SpatialObjects[^1].texture = TextureManager.RetrieveTexture(packet.TextureLocation);
                            currentScene.SpatialObjects[^1].rigidbody.SetVelocity(packet.Velocity);
                            currentScene.SpatialObjects[^1].rigidbody.SetAngularVelocity(packet.AngVelocity);
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
                        if (packet.id < joinedPlayers.Count)
                        {
                            joinedPlayers[packet.id].position = packet.Position;
                            joinedPlayers[packet.id].rotation = packet.Rotation;
                        }
                        break;
                    }
                case (ushort)PacketType.PlayerJoin:
                    {
                        PlayerJoinPacket packet = new PlayerJoinPacket();
                        joinedPlayers.Add(new ClientPlayer(packet.id, packet.Position, packet.Rotation));
                        OtherClientConnected?.Invoke(null, new OtherClientConnectEvent()
                        {
                            client = joinedPlayers.Count - 1
                        });
                        break;
                    }
                case (ushort)PacketType.PlayerLeave:
                    {
                        PlayerLeavePacket packet = new PlayerLeavePacket();
                        packet.ByteToPacket(data);
                        joinedPlayers.RemoveAt(packet.clientId);
                        OtherClientDisconnected?.Invoke(null, new OtherClientConnectEvent()
                        {
                            client = packet.clientId
                        });
                        break;
                    }
            }
            
            if(handleServerPacket is not null)
                handleServerPacket.Invoke(data);
        }

        public static Connection GetConnection() => client.Connection;
        public static bool IsConnected() => client is not null && client.IsConnected;
    }
}