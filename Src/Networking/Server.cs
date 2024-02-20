using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Riptide;

//engine stuff
using static SpatialEngine.Globals;
using static SpatialEngine.Rendering.MeshUtils;
using System.Numerics;
using JoltPhysicsSharp;
using Riptide.Transports;

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
    
    public class SpatialServer
    {
        public static Server server;

        public ushort port;
        public string ip;
        public int maxConnections { get; protected set; }

        bool stopping;


        public SpatialServer(string ip, ushort port = 58301, int maxConnections = 10)
        {
            this.ip = ip;
            this.port = port;
            this.maxConnections = maxConnections;
            Message.InstancesPerPeer = 100;
        }

        public void Start()
        {
            server = new Server();
            server.ClientConnected += ClientConnected;
            server.MessageReceived += handleMessage;
            server.Start(port, (ushort)maxConnections, 0, false);
        }

        public void Stop()
        {
            stopping = true;
            server.Stop();
        }

        public void Update(float deltaTime)
        {
            if(!stopping)
            {
                server.Update();
            }
        }

        public void ClientConnected(object sender, EventArgs e)
        {
            
        }

        public Connection[] GetServerConnections()
        {
            if(stopping)
            {
                return [];
            }
            return server.Clients;
        }

        public void SendUnrelib(Packet packet, ushort clientId)
        {
            if(!stopping)
            {
                Message msgUnrelib = Message.Create(MessageSendMode.Unreliable, packet.GetPacketType());
                msgUnrelib.AddBytes(packet.ConvertToByte());
                server.Send(msgUnrelib, clientId);
            }
        }

        public void SendRelib(Packet packet, ushort clientId)
        {
            if (!stopping)
            {
                Message msgRelib = Message.Create(MessageSendMode.Reliable, packet.GetPacketType());
                msgRelib.AddBytes(packet.ConvertToByte());
                server.Send(msgRelib, clientId);
            }
        }

        public void SendUnrelibAll(Packet packet)
        {
            if (!stopping)
            {
                Message msgUnrelib = Message.Create(MessageSendMode.Unreliable, packet.GetPacketType());
                msgUnrelib.AddBytes(packet.ConvertToByte());
                server.SendToAll(msgUnrelib);
            }
        }
        public void SendUnrelibAllExclude(Packet packet, ushort clientId)
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

        public void SendRelibAllExclude(Packet packet, ushort clientId)
        {
            if (!stopping)
            {
                Message msgUnrelib = Message.Create(MessageSendMode.Reliable, packet.GetPacketType());
                msgUnrelib.AddBytes(packet.ConvertToByte());
                for (int i = 0; i < server.Clients.Length; i++)
                {
                    if (clientId != server.Clients[i].Id)
                        server.Send(msgUnrelib, server.Clients[i]);
                }
            }
        }


        public void SendRelibAll(Packet packet)
        {
            if (!stopping)
            {
                Message msgRelib = Message.Create(MessageSendMode.Reliable, packet.GetPacketType());
                msgRelib.AddBytes(packet.ConvertToByte());
                server.SendToAll(msgRelib);
            }
        }

        public void handleMessage(object sender, MessageReceivedEventArgs e)
        {
            if (!stopping)
                HandlePacketServer(e.Message.GetBytes(), e.FromConnection);
        }

        public void Close()
        {
            stopping = true;
            server.Stop();
            server = null;
        }

        //Handles packets that come from the client

        void HandlePacketServer(byte[] data, Connection client)
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

                        //scene sync when connect
                        SceneSyncStart packet2 = new SceneSyncStart();
                        SendRelib(packet2, client.Id);
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
                //spawn object on server side but then send to all clients except from the client it was sent by
                case (ushort)PacketType.SpawnSpatialObject:
                    {
                        SpawnSpatialObjectPacket packet = new SpawnSpatialObjectPacket();
                        packet.ByteToPacket(data);
                        if (packet.id < scene.SpatialObjects.Count)
                        {
                            bodyInterface.DestroyBody(scene.SpatialObjects[packet.id].SO_rigidbody.rbID);
                            scene.SpatialObjects[packet.id] = new SpatialObject(LoadModel(packet.Position, packet.Rotation, SpatialEngine.Resources.ModelPath, packet.ModelLocation), (MotionType)packet.MotionType, (ObjectLayer)packet.ObjectLayer, (Activation)packet.Activation, (uint)packet.id);
                        }
                        else
                        {
                            scene.AddSpatialObject(LoadModel(packet.Position, packet.Rotation, SpatialEngine.Resources.ModelPath, packet.ModelLocation), (MotionType)packet.MotionType, (ObjectLayer)packet.ObjectLayer, (Activation)packet.Activation);
                        }
                        stream.Close();
                        reader.Close();

                        SendUnrelibAllExclude(packet, client.Id);

                        break;
                    }
                case (ushort)PacketType.SceneSyncClear:
                    {
                        for (int i = 0; i < scene.SpatialObjects.Count; i++)
                        {
                            SpawnSpatialObjectPacket packet = new SpawnSpatialObjectPacket(i, scene.SpatialObjects[i].SO_mesh.position, scene.SpatialObjects[i].SO_mesh.rotation, 
                                scene.SpatialObjects[i].SO_mesh.modelLocation, scene.SpatialObjects[i].SO_rigidbody.settings.MotionType, bodyInterface.GetObjectLayer(scene.SpatialObjects[i].SO_rigidbody.rbID), 
                                (Activation)Convert.ToInt32(bodyInterface.IsActive(scene.SpatialObjects[i].SO_rigidbody.rbID)));
                            SendRelib(packet, client.Id);
                        }
                        break;
                    }
                case (ushort)PacketType.Player:
                    {
                        //send this packet to all clients except from sender
                        PlayerPacket packet = new PlayerPacket();
                        packet.ByteToPacket(data);
                        //set id to the client id so the clients can have the correct spot in the list and subtract 1 as it does not include itself
                        packet.id = client.Id - 1;
                        SendUnrelibAllExclude(packet, client.Id);
                        break;
                    }
                case (ushort)PacketType.PlayerJoin:
                    {
                        //send join signal to all other clients except to the one that joined
                        PlayerJoinPacket packet = new PlayerJoinPacket();
                        packet.ByteToPacket(data);
                        SendRelibAllExclude(packet, client.Id);

                        //send signals to create a player for the current client if it joined after another client
                        for (int i = 0; i < server.ClientCount; i++)
                        {
                            SendRelib(packet, client.Id);
                        }
                        break;
                    }
            }
        }

        public bool IsRunning() => server.IsRunning;
    }
}