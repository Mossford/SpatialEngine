using System.IO;
using System.Numerics;
using SpatialEngine.Networking;

namespace SpatialEngine.Networking.Packets
{
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
}