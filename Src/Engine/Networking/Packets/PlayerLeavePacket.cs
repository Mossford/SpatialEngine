using System.IO;
using SpatialEngine.Networking;

namespace SpatialEngine.Networking.Packets
{
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
            writer.Write((ushort)PacketType.PlayerLeave);
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