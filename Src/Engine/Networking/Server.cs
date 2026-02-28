using System;
using System.IO;
using Riptide;
using JoltPhysicsSharp;
using System.Collections.Generic;

//engine stuff
using static SpatialEngine.Globals;
using static SpatialEngine.Rendering.MeshUtils;
using Riptide.Transports;
using System.Collections;
using SpatialEngine.Networking.Packets;
using SpatialEngine.Rendering;

namespace SpatialEngine.Networking
{
    //client and server
    //updates get sent here and sent out here
    //Client Server arch:
    /*
        client sends input updates to server and server will apply that update
        server can also be host
        host computer will auto update its self and send that update to all clients
        
        every frame:
        check for stuff from clients (input)
        if there is input
            run the update based on that input (enum with switch?)
        run host update
        send all update to client (position of things, rotation of things)
    
    */
    
    public static class SpatialServer
    {
        public static Server server;

        public static ushort port;
        public static string ip;
        public static int maxConnections { get; set; }
        //first is the current id and the key is the client id from the server which riptide does not auto correct
        public static Dictionary<uint, uint> connectionIds;
        static uint connectionCount = 0;

        static bool stopping;
        static Action<byte[], Connection> handleClientPacket;


        public static void Init(string ip, ushort port = 58301, int maxConnections = 10)
        {
            SpatialServer.ip = ip;
            SpatialServer.port = port;
            SpatialServer.maxConnections = maxConnections;
            Message.InstancesPerPeer = 100;
            connectionIds = new Dictionary<uint, uint>();
        }

        public static void SetClientHandle(Action<byte[], Connection> action)
        {
            handleClientPacket = action;
        }

        public static void Start()
        {
            server = new Server();
            server.MessageReceived += handleMessage;
            server.ClientConnected += ClientConnected;
            server.ClientDisconnected += ClientDisconnected;
            server.Start(port, (ushort)maxConnections, 0, false);
        }

        public static void Stop()
        {
            stopping = true;
            server.Stop();
        }

        public static void Update(float deltaTime)
        {
            if(!stopping)
            {
                server.Update();
            }
        }

        public static void ClientConnected(object sender, ServerConnectedEventArgs e)
        {
            connectionIds.Add(e.Client.Id, (uint)connectionIds.Count);
        }

        public static void ClientDisconnected(object sender, ServerDisconnectedEventArgs e)
        {
            //reset all values in connectionId after the left client as we need to bring down it by one
            for (uint i = connectionIds[e.Client.Id]; i < server.Clients.Length; i++)
            {
                //and skip the first client as will be -1
                //if (connectionIds[server.Clients[i].Id] != 0)
                //{
                    //set the values at the keys of each client after the left client to be one less
                    connectionIds[server.Clients[i].Id] = connectionIds[server.Clients[i].Id] - 1;
                //}
            }

            connectionCount--;

            PlayerLeavePacket packet = new PlayerLeavePacket();

            //using the algorithm for sending the player packets
            int currentId = (int)connectionIds[e.Client.Id];

            //start from one as we cannot access the client list as the client has been removed
            for (int i = 0; i < connectionCount; i++)
            {
                //if we are the same client we skip
                if (i == e.Client.Id)
                    continue;

                if (currentId > connectionIds[server.Clients[i].Id])
                {
                    packet.clientId = currentId - 1;
                    SendRelib(packet, server.Clients[i].Id);
                }
                else
                {
                    packet.clientId = currentId;
                    SendRelib(packet, server.Clients[i].Id);
                }
            }
            connectionIds.Remove(e.Client.Id);

        }

        public static Connection[] GetServerConnections()
        {
            if(stopping)
            {
                return [];
            }
            return server.Clients;
        }

        public static void SendUnrelib(Packet packet, ushort clientId)
        {
            if(!stopping)
            {
                Message msgUnrelib = Message.Create(MessageSendMode.Unreliable, packet.GetPacketType());
                msgUnrelib.AddBytes(packet.ConvertToByte());
                server.Send(msgUnrelib, clientId);
            }
        }

        public static void SendRelib(Packet packet, ushort clientId)
        {
            if (!stopping)
            {
                Message msgRelib = Message.Create(MessageSendMode.Reliable, packet.GetPacketType());
                msgRelib.AddBytes(packet.ConvertToByte());
                server.Send(msgRelib, clientId);
            }
        }

        public static void SendUnrelibAll(Packet packet)
        {
            if (!stopping)
            {
                Message msgUnrelib = Message.Create(MessageSendMode.Unreliable, packet.GetPacketType());
                msgUnrelib.AddBytes(packet.ConvertToByte());
                server.SendToAll(msgUnrelib);
            }
        }

        public static void SendRelibAll(Packet packet)
        {
            if (!stopping)
            {
                Message msgRelib = Message.Create(MessageSendMode.Reliable, packet.GetPacketType());
                msgRelib.AddBytes(packet.ConvertToByte());
                server.SendToAll(msgRelib);
            }
        }

        public static void SendUnrelibAllExclude(Packet packet, ushort clientId)
        {
            if (!stopping)
            {
                Message msgUnrelib = Message.Create(MessageSendMode.Unreliable, packet.GetPacketType());
                msgUnrelib.AddBytes(packet.ConvertToByte());
                for (int i = 0; i < server.Clients.Length; i++)
                {
                    if(clientId != server.Clients[i].Id)
                        server.Send(msgUnrelib, server.Clients[i]);
                }
            }
        }

        public static void SendRelibAllExclude(Packet packet, ushort clientId)
        {
            if (!stopping)
            {
                Message msgRelib = Message.Create(MessageSendMode.Reliable, packet.GetPacketType());
                msgRelib.AddBytes(packet.ConvertToByte());
                for (int i = 0; i < server.Clients.Length; i++)
                {
                    if (clientId != server.Clients[i].Id)
                    {
                        server.Send(msgRelib, server.Clients[i]);
                    }
                }
            }
        }

        public static void handleMessage(object sender, MessageReceivedEventArgs e)
        {
            if (!stopping)
                HandlePacketServer(e.Message.GetBytes(), e.FromConnection);
        }

        public static void Close()
        {
            stopping = true;
            server.Stop();
            server = null;
        }

        //Handles packets that come from the client

        static void HandlePacketServer(byte[] data, Connection client)
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
                case (ushort)PacketType.Ping:
                    {
                        PongPacket packet = new PongPacket();
                        SendRelib(packet, client.Id);
                        break;
                    }
                case (ushort)PacketType.Connect:
                    {
                        ConnectReturnPacket packet = new ConnectReturnPacket();
                        SendRelib(packet, client.Id);
                        break;
                    }
                case (ushort)PacketType.SpatialObject:
                    {
                        SpatialObjectPacket packet = new SpatialObjectPacket();
                        packet.ByteToPacket(data);
                        if (packet.id >= currentScene.SpatialObjects.Count)
                            break;
                        
                        currentScene.SpatialObjects[packet.id].rigidbody.SetPosition(packet.Position);
                        currentScene.SpatialObjects[packet.id].rigidbody.SetRotation(packet.Rotation);
                        currentScene.SpatialObjects[packet.id].rigidbody.SetVelocity(packet.Velocity);
                        currentScene.SpatialObjects[packet.id].rigidbody.SetAngularVelocity(packet.AngVelocity);
                        currentScene.SpatialObjects[packet.id].enabled = packet.Enabled;
                        stream.Close();
                        reader.Close();
                        break;
                    }
                //spawn object on server side but then send to all clients except from the client it was sent by
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
                                packet.ModelLocation, packet.Scale), 
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

                        SendUnrelibAllExclude(packet, client.Id);

                        break;
                    }
                case (ushort)PacketType.SceneSyncClear:
                    {
                        for (int i = 0; i < currentScene.SpatialObjects.Count; i++)
                        {
                            SpawnSpatialObjectPacket packet = new SpawnSpatialObjectPacket(i, currentScene.SpatialObjects[i].mesh.position, currentScene.SpatialObjects[i].mesh.rotation, 
                                currentScene.SpatialObjects[i].mesh.scale, currentScene.SpatialObjects[i].texture.textLocation,
                                currentScene.SpatialObjects[i].mesh.modelLocation, currentScene.SpatialObjects[i].rigidbody.settings.MotionType, bodyInterface.GetObjectLayer(currentScene.SpatialObjects[i].rigidbody.rbID), 
                                (Activation)Convert.ToInt32(bodyInterface.IsActive(currentScene.SpatialObjects[i].rigidbody.rbID)), currentScene.SpatialObjects[i].enabled);
                            packet.Velocity = currentScene.SpatialObjects[i].rigidbody.GetVelocity();
                            packet.AngVelocity = currentScene.SpatialObjects[i].rigidbody.GetAngVelocity();
                            SendRelib(packet, client.Id);
                        }
                        break;
                    }
                case (ushort)PacketType.Player:
                    {
                        //send this packet to all clients except from sender
                        PlayerPacket packet = new PlayerPacket();
                        packet.ByteToPacket(data);
                        if(connectionIds.Count == server.ClientCount)
                        {

                            //if our client id is greater than the one before we subtract 1 from the sent id
                            //if our client id is less than the one after we use the same value
                            uint currentId = connectionIds[client.Id];

                            for (int i = 0; i < connectionIds.Count; i++)
                            {
                                //if we are the same client we skip
                                if (i == currentId)
                                    continue;

                                if (currentId > connectionIds[server.Clients[i].Id])
                                {
                                    packet.id = (int)currentId - 1;
                                    SendUnrelib(packet, server.Clients[i].Id);
                                }
                                else
                                {
                                    packet.id = (int)currentId;
                                    SendUnrelib(packet, server.Clients[i].Id);
                                }
                            }
                        }
                        break;
                    }
                case (ushort)PacketType.PlayerJoin:
                    {
                        //send join signal to all other clients except to the one that joined
                        PlayerJoinPacket packet = new PlayerJoinPacket();
                        packet.ByteToPacket(data);
                        SendRelibAllExclude(packet, client.Id);

                        connectionCount++;

                        //send signals to create a player for the current client if it joined after another client
                        //if (connectionCount == server.ClientCount)
                        //{
                            for (int i = 0; i < connectionCount - 1; i++)
                            {
                                SendRelib(packet, client.Id);
                            }
                        //}
                        break;
                    }
            }
            
            if(handleClientPacket is not null)
                handleClientPacket.Invoke(data, client);
        }

        public static bool IsRunning() => server is not null && server.IsRunning;

    }
}