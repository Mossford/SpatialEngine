using System.IO;
using System.Numerics;
using JoltPhysicsSharp;
using SpatialEngine.Networking;

namespace SpatialEngine.Networking.Packets
{
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
}