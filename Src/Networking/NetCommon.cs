using System;
using System.IO;
using System.Numerics;
using System.Reflection.Emit;
using JoltPhysicsSharp;
using Silk.NET.Core;
using Silk.NET.Input;

//engine stuff
using static SpatialEngine.Globals;
using static SpatialEngine.Rendering.MeshUtils;

namespace SpatialEngine.Networking
{
    
    public enum PacketType : ushort
    {
        Ping,
        Pong,
        Connect,
        ConnectReturn,
        SpatialObjectPacket,
        SpawnSpatialObject,
    }
    
    public abstract class Packet
    {
        // may be better to have the handle packet here and it returns a record of the packet type i need
        // handles whatever data and returns the packet with the data
    
        public abstract byte[] ConvertToByte();
        public abstract void ByteToPacket(byte[] data);
    }
    
    public abstract class ServerPacket
    {
    
        public abstract byte[] ConvertToByte();
        public abstract void ByteToPacket(byte[] data);
    }
    
    public abstract class ClientPacket
    {
    
        public abstract byte[] ConvertToByte();
        public abstract void ByteToPacket(byte[] data);
    }
    
    
    //Connection packets
    
    public class ConnectPacket : ClientPacket
    {
        public ConnectPacket() 
        {
            
        }
    
        public override byte[] ConvertToByte() 
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.Connect);
            return stream.ToArray();
        }
    
        public override void ByteToPacket(byte[] data)
        {
            //does nothing
        }
    }
    
    public class ConnectReturnPacket : ServerPacket
    {
        public ConnectReturnPacket()
        {
    
        }
    
        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(EngVer);
            throw new NotImplementedException();
    
        }
    
        public override void ByteToPacket(byte[] data)
        {
            //does nothing
        }
    }

    //Packet for getting time between sent packets
    //checks just based on the enum of packets

    public class PingPacket : ClientPacket
    {
        public PingPacket()
        {

        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.Ping);
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            //does nothing
        }
    }

    public class PongPacket : ServerPacket
    {
        public PongPacket()
        {

        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.Pong);
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            //does nothing
        }
    }
    

    //sync packets
    
    //will need more infomration then this like info about the rigidbody state and whatever
    public class SpatialObjectPacket : ServerPacket
    {
        public int id;
        public Vector3 Position;
        public Quaternion Rotation;
    
        public SpatialObjectPacket()
        {
    
        }
    
        public SpatialObjectPacket(int id, Vector3 position, Quaternion rotation)
        {
            this.id = id;
            this.Position = position;
            this.Rotation = rotation;
        }
    
        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            //type of packet
            writer.Write((ushort)PacketType.SpatialObjectPacket);
            //id
            writer.Write(id);
            //position
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);
            //rotation
            writer.Write(Rotation.X);
            writer.Write(Rotation.Y);
            writer.Write(Rotation.Z);
            writer.Write(Rotation.W);
            stream.Close();
            writer.Close();
    
            return stream.ToArray();
        }
    
        public override void ByteToPacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            ushort type = reader.ReadUInt16();
            id = reader.ReadInt32();
            //position
            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            float posZ = reader.ReadSingle();
            Position = new Vector3(posX, posY, posZ);
            //rotation
            float rotX = reader.ReadSingle();
            float rotY = reader.ReadSingle();
            float rotZ = reader.ReadSingle();
            float rotW = reader.ReadSingle();
            Rotation = new Quaternion(rotX, rotY, rotZ, rotW);
            stream.Close();
            reader.Close();
        }
    }
    
    public class SpawnSpatialObjectPacket : ClientPacket
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public string ModelLocation;
        public int MotionType;
        public int ObjectLayer;
        public int Activation;
    
        public SpawnSpatialObjectPacket()
        {
    
        }
    
        public SpawnSpatialObjectPacket(int id, Vector3 position, Quaternion rotation, string modelLocation, MotionType motionType, ObjectLayer objectLayer, Activation activation)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.ModelLocation = modelLocation;
            this.MotionType = (int)motionType;
            this.ObjectLayer = objectLayer;
            this.Activation = (int)activation;
        }
    
        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            //type of packet
            writer.Write((ushort)PacketType.SpawnSpatialObject);
            //id
            //writer.Write(id);
            //position
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);
            //rotation
            writer.Write(Rotation.X);
            writer.Write(Rotation.Y);
            writer.Write(Rotation.Z);
            writer.Write(Rotation.W);
            //modellocation
            writer.Write(ModelLocation);
            //motion type
            writer.Write(MotionType);
            //object layer
            writer.Write(ObjectLayer);
            //activation
            writer.Write(Activation);
            stream.Close();
            writer.Close();
    
            return stream.ToArray();
        }
    
        public override void ByteToPacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            int type = reader.ReadUInt16();
            //id = reader.ReadInt32();
            //position
            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            float posZ = reader.ReadSingle();
            Position = new Vector3(posX, posY, posZ);
            //rotation
            float rotX = reader.ReadSingle();
            float rotY = reader.ReadSingle();
            float rotZ = reader.ReadSingle();
            float rotW = reader.ReadSingle();
            Rotation = new Quaternion(rotX, rotY, rotZ, rotW);
            ModelLocation = reader.ReadString();
            MotionType = reader.ReadInt32();
            ObjectLayer = reader.ReadInt32();
            Activation = reader.ReadInt32();
            stream.Close();
            reader.Close();
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