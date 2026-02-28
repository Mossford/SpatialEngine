using System.IO;
using System.Numerics;
using SpatialEngine.Networking;

namespace SpatialEngine.Networking.Packets
{
    public class SpatialObjectPacket : Packet
    {
        public int id;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngVelocity;
        public bool Enabled;

        public SpatialObjectPacket()
        {

        }

        public SpatialObjectPacket(int id, Vector3 position, Quaternion rotation, bool enabled = true)
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
            //velocity
            writer.Write(Velocity.X);
            writer.Write(Velocity.Y);
            writer.Write(Velocity.Z);
            //ang velocity
            writer.Write(AngVelocity.X);
            writer.Write(AngVelocity.Y);
            writer.Write(AngVelocity.Z);
            writer.Write(Enabled);
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
            Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Rotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Velocity = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            AngVelocity = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Enabled = reader.ReadBoolean();
            stream.Close();
            reader.Close();
        }

        public override ushort GetPacketType() => (ushort)PacketType.SpatialObject;
    }
}