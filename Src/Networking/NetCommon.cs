using System;
using System.IO;
using System.Numerics;
using System.Reflection.Emit;
using Silk.NET.Input;
using static SpatialEngine.Globals;

namespace SpatialEngine
{
    namespace Networking
    {
	
        public enum PacketType
        {
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
                writer.Write((int)PacketType.SpatialObjectPacket);
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
                int type = reader.ReadInt32();
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

        public class SpawnSpatialObjectPacket : Packet
        {
            public int id;
            public Vector3 Position;
            public Quaternion Rotation;
            public string ModelLocation;

            public SpawnSpatialObjectPacket()
            {

            }

            public SpawnSpatialObjectPacket(int id, Vector3 position, Quaternion rotation, string modelLocation)
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
                writer.Write((int)PacketType.SpatialObjectPacket);
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
                int type = reader.ReadInt32();
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
        }


        public static class PacketHandler
        {
            public static void HandlePacket(byte[] data)
            {
                MemoryStream stream = new MemoryStream(data);
                BinaryReader reader = new BinaryReader(stream);
                //packet type
                int type = reader.ReadInt32();

                switch (type)
                {
                    case (int)PacketType.SpatialObjectPacket:
                    {
                        SpatialObjectPacket packet = new SpatialObjectPacket();
                        packet.ByteToPacket(data);
                        scene.SpatialObjects[packet.id].SO_mesh.position = packet.Position;
                        //scene.SpatialObjects[packet.id].SO_mesh.rotation = packet.Rotation;
                        stream.Close();
                        reader.Close();
                        break;
                    }
                }
            }
        }
    }
}