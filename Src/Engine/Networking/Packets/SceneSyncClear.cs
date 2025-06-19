using System.IO;
using SpatialEngine.Networking;

namespace SpatialEngine.Networking.Packets
{
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
}