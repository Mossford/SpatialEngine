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
        public Vector3 Scale;
        public string ModelLocation;
        public string TextureLocation;
        public int MotionType;
        public int ObjectLayer;
        public int Activation;
        public bool Enabled;
        public Vector3 Velocity;
        public Vector3 AngVelocity;

        public SpawnSpatialObjectPacket()
        {

        }

        public SpawnSpatialObjectPacket(int id, Vector3 position, Quaternion rotation, Vector3 scale, string modelLocation, string textureLocation, MotionType motionType, ObjectLayer objectLayer, Activation activation, bool enabled)
        {
            this.id = id;
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
            this.ModelLocation = modelLocation;
            this.TextureLocation = textureLocation;
            this.MotionType = (int)motionType;
            this.ObjectLayer = objectLayer;
            this.Activation = (int)activation;
            this.Enabled = enabled;
        }
        
        public SpawnSpatialObjectPacket(SpatialObject so)
        {
            this.id = id;
            this.Position = so.mesh.position;
            this.Rotation = so.mesh.rotation;
            this.Scale =so.mesh.scale;
            this.ModelLocation = so.mesh.modelLocation;
            this.TextureLocation = so.texture.textLocation;
            this.MotionType = (int)so.rigidbody.settings.MotionType;
            this.ObjectLayer = so.rigidbody.layer;
            this.Activation = (int)so.rigidbody.activation;
            this.Enabled = so.enabled;
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
            //scale
            writer.Write(Scale.X);
            writer.Write(Scale.Y);
            writer.Write(Scale.Z);
            //modellocation
            if(ModelLocation is not null)
                writer.Write(ModelLocation);
            else
                writer.Write("0");
            if(TextureLocation is not null)
                writer.Write(TextureLocation);
            else
                writer.Write("");
            //motion type
            writer.Write(MotionType);
            //object layer
            writer.Write(ObjectLayer);
            //activation
            writer.Write(Activation);
            writer.Write(Enabled);
            
            writer.Write(Velocity.X);
            writer.Write(Velocity.Y);
            writer.Write(Velocity.Z);
            
            writer.Write(AngVelocity.X);
            writer.Write(AngVelocity.Y);
            writer.Write(AngVelocity.Z);
            
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
            Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            //rotation
            Rotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            TextureLocation = reader.ReadString();
            ModelLocation = reader.ReadString();
            MotionType = reader.ReadInt32();
            ObjectLayer = reader.ReadInt32();
            Activation = reader.ReadInt32();
            Enabled = reader.ReadBoolean();
            Velocity = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            AngVelocity = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            stream.Close();
            reader.Close();
        }

        public override ushort GetPacketType() => (ushort)PacketType.SpawnSpatialObject;
    }
}