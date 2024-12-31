using System.IO;
using SpatialEngine;
using SpatialEngine.Networking;

namespace SpatialEngine.Networking.Packets
{
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
            writer.Write(Globals.EngVer);
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
}