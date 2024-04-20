using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using JoltPhysicsSharp;
using Riptide;
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
        SpatialObject,
        SpawnSpatialObject,
        SceneSyncStart,
        SceneSyncClear,
        Player,
        PlayerJoin,
        PlayerLeave,
    }

    public abstract class Packet
    {
        // may be better to have the handle packet here and it returns a record of the packet type i need
        // handles whatever data and returns the packet with the data

        public abstract byte[] ConvertToByte();
        public abstract void ByteToPacket(byte[] data);
        public abstract ushort GetPacketType();
    }


    //Connection packets

    public class ConnectPacket : Packet
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

        public override ushort GetPacketType() => (ushort)PacketType.Connect;
    }

    public class ConnectReturnPacket : Packet
    {
        public string engVersion;

        public ConnectReturnPacket()
        {

        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.ConnectReturn);
            writer.Write(EngVer);
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            int type = reader.ReadUInt16();
            engVersion = reader.ReadString();
        }

        public override ushort GetPacketType() => (ushort)PacketType.ConnectReturn;
    }

    //Packet for getting time between sent packets
    //checks just based on the enum of packets

    public class PingPacket : Packet
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

        public override ushort GetPacketType() => (ushort)PacketType.Ping;
    }

    public class PongPacket : Packet
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

        public override ushort GetPacketType() => (ushort)PacketType.Pong;
    }


    //sync packets

    //will need more infomration then this like info about the rigidbody state and whatever
    public class SpatialObjectPacket : Packet
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
            writer.Write((ushort)PacketType.SpatialObject);
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

        public override ushort GetPacketType() => (ushort)PacketType.SpatialObject;
    }

    public class SpawnSpatialObjectPacket : Packet
    {
        public int id;
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
            this.id = id;
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
            ModelLocation = reader.ReadString();
            MotionType = reader.ReadInt32();
            ObjectLayer = reader.ReadInt32();
            Activation = reader.ReadInt32();
            stream.Close();
            reader.Close();
        }

        public override ushort GetPacketType() => (ushort)PacketType.SpawnSpatialObject;
    }

    public class SceneSyncStart : Packet
    {
        public SceneSyncStart()
        {

        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.SceneSyncStart);
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            //does nothing
        }

        public override ushort GetPacketType() => (ushort)PacketType.SceneSyncStart;
    }

    public class SceneSyncClear : Packet
    {
        public SceneSyncClear()
        {

        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.SceneSyncClear);
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            //does nothing
        }

        public override ushort GetPacketType() => (ushort)PacketType.SceneSyncClear;
    }

    public class PlayerPacket : Packet
    {
        public int id;
        public Vector3 Position;
        public Quaternion Rotation;

        public PlayerPacket()
        {

        }

        public PlayerPacket(int id, Vector3 position, Quaternion rotation)
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
            writer.Write((ushort)PacketType.Player);
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

        public override ushort GetPacketType() => (ushort)PacketType.Player;
    }

    public class PlayerJoinPacket : Packet
    {
        public int id;
        public Vector3 Position;
        public Quaternion Rotation;
        public string ModelLocation;

        public PlayerJoinPacket()
        {

        }

        public PlayerJoinPacket(int id, Vector3 position, Quaternion rotation, string modelLocation)
        {
            this.id = id;
            this.Position = position;
            this.Rotation = rotation;
            this.ModelLocation = modelLocation;
        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            //type of packet
            writer.Write((ushort)PacketType.PlayerJoin);
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
            //modellocation
            writer.Write(ModelLocation);
            stream.Close();
            writer.Close();

            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            int type = reader.ReadUInt16();
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
            ModelLocation = reader.ReadString();
            stream.Close();
            reader.Close();
        }

        public override ushort GetPacketType() => (ushort)PacketType.PlayerJoin;
    }

    public class PlayerLeavePacket : Packet
    {
        public int clientId;

        public PlayerLeavePacket()
        {

        }

        public PlayerLeavePacket(int clientId)
        {
            this.clientId = clientId;
        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            //type of packet
            writer.Write((ushort)PacketType.PlayerJoin);
            //id
            writer.Write(clientId);
            stream.Close();
            writer.Close();

            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            int type = reader.ReadUInt16();
            clientId = reader.ReadInt32();
            stream.Close();
            reader.Close();
        }

        public override ushort GetPacketType() => (ushort)PacketType.PlayerLeave;
    }
}