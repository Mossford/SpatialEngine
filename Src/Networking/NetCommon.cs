using System;
using System.IO;
using System.Numerics;

namespace SpatialEngine
{
    namespace Networking
    {
        public interface IPacket
        {
            public abstract byte[] ConvertToByte();
            public abstract void ByteToPacket(byte[] data);
        }

        //will need more infomration then this like info about the rigidbody state and whatever
        public class SpatialObjectPacket : IPacket
        {
            public int id;
            public Vector3 Position;
            public Quaternion Rotation;

            public byte[] ConvertToByte()
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
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

            public void ByteToPacket(byte[] data)
            {
                MemoryStream stream = new MemoryStream(data);
                BinaryReader reader = new BinaryReader(stream);
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

    }
}