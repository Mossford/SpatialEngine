using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection.Emit;
using JoltPhysicsSharp;
using Riptide;
using Silk.NET.Core;
using Silk.NET.Input;


//engine stuff
using static SpatialEngine.Globals;
using static SpatialEngine.Networking.IPackets;
using static SpatialEngine.Rendering.MeshUtils;

namespace SpatialEngine.Networking
{
    
    public enum PacketType : ushort
    {
        Ping,
        Pong,
        Connect,
        ConnectReturn,
        SpatialObject,
        SpawnSpatialObject
    }

    public interface IPackets
    {
        public ushort GetPacketType();
        public byte[] GetBytes();

        public record PingPacket() : IPackets
        {
            public ushort GetPacketType() => (ushort)PacketType.Ping;
            public byte[] GetBytes()
            {
                return BitConverter.GetBytes((ushort)PacketType.Ping);
            }
        }
        public record PongPacket()
        {
            public ushort GetPacketType() => (ushort)PacketType.Pong;
            public byte[] GetBytes()
            {
                return BitConverter.GetBytes((ushort)PacketType.Pong);
            }
        }
        public record ConnectPacket()
        {
            public ushort GetPacketType() => (ushort)PacketType.Connect;
            public byte[] GetBytes()
            {
                return BitConverter.GetBytes((ushort)PacketType.Connect);
            }
        }
        public record ConnectReturnPacket()
        {
            public ushort GetPacketType() => (ushort)PacketType.ConnectReturn;
            public byte[] GetBytes()
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes((ushort)PacketType.ConnectReturn));
                for (int i = 0; i < EngVer.Length; i++)
                {
                    bytes.Add(Convert.ToByte(EngVer[i]));
                }
                return bytes.ToArray();
            }
        }
        public record SpatialObjectPacket(int id, Vector3 position, Quaternion rotation)
        {
            public ushort GetPacketType() => (ushort)PacketType.SpatialObject;
            public byte[] GetBytes()
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes((ushort)PacketType.SpatialObject));
                bytes.AddRange(BitConverter.GetBytes(id));
                bytes.AddRange(BitConverter.GetBytes(position.X));
                bytes.AddRange(BitConverter.GetBytes(position.Y));
                bytes.AddRange(BitConverter.GetBytes(position.Z));
                bytes.AddRange(BitConverter.GetBytes(rotation.X));
                bytes.AddRange(BitConverter.GetBytes(rotation.Y));
                bytes.AddRange(BitConverter.GetBytes(rotation.Z));
                bytes.AddRange(BitConverter.GetBytes(rotation.W));
                return bytes.ToArray();
            }
        }
        public record SpawnSpatialObjectPacket(Vector3 position, Quaternion rotation, string modelLocation, ushort motionType, ushort ObjectLayer, ushort Activation)
        {
            public ushort GetPacketType() => (ushort)PacketType.SpawnSpatialObject;
            public byte[] GetBytes()
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes((ushort)PacketType.SpawnSpatialObject));
                bytes.AddRange(BitConverter.GetBytes(position.X));
                bytes.AddRange(BitConverter.GetBytes(position.Y));
                bytes.AddRange(BitConverter.GetBytes(position.Z));
                bytes.AddRange(BitConverter.GetBytes(rotation.X));
                bytes.AddRange(BitConverter.GetBytes(rotation.Y));
                bytes.AddRange(BitConverter.GetBytes(rotation.Z));
                bytes.AddRange(BitConverter.GetBytes(rotation.W));
                for (int i = 0; i < modelLocation.Length; i++)
                {
                    bytes.Add(Convert.ToByte(modelLocation[i]));
                }
                bytes.AddRange(BitConverter.GetBytes(motionType));
                bytes.AddRange(BitConverter.GetBytes(ObjectLayer));
                bytes.AddRange(BitConverter.GetBytes(Activation));
                return bytes.ToArray();
            }
        }
    }
    
    public class Packet
    {
        // may be better to have the handle packet here and it returns a record of the packet type i need
        // handles whatever data and returns the packet with the data

        public Packet()
        {
            
        }

        public byte[] ConvertToByte<T>(T packet) where T : IPackets
        {
            switch (packet.GetPacketType())
            {
                case (ushort)PacketType.Ping:
                    return packet.GetBytes();
                case (ushort)PacketType.Pong:
                    return packet.GetBytes();
                case (ushort)PacketType.Connect:
                    return packet.GetBytes();
                case (ushort)PacketType.ConnectReturn:
                    return packet.GetBytes();
                case (ushort)PacketType.SpatialObject:
                    return packet.GetBytes();
                case (ushort)PacketType.SpawnSpatialObject:
                    return packet.GetBytes();
            }

            // if reach here input packet is bad or not implement
            throw new Exception("Packet not found");
        }
        public void ByteToPacket(byte[] data)
        {

        }
    }
    
    
    //what should be done when a packet is recived of any types
    //should work for both client and host/server
    
    public static class PacketHandler
    {
        public static void HandlePacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            //packet type
            ushort type = reader.ReadUInt16();
    
            switch (type)
            {
                case (ushort)PacketType.Ping:
                    {
                    
                        break;
                    }
                case (ushort)PacketType.Pong:
                    {
                    
                        break;
                    }
                case (ushort)PacketType.SpatialObjectPacket:
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
    }
}