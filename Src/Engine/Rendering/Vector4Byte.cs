using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialEngine.Rendering
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vector4Byte
    {
        public byte x { get; set; }
        public byte y { get; set; }
        public byte z { get; set; }
        public byte w { get; set; }

        public Vector4Byte(byte x, byte y, byte z, byte w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector4Byte(Vector3 vector, byte w)
        {
            this.x = (byte)vector.X;
            this.y = (byte)vector.Y;
            this.z = (byte)vector.Z;
            this.w = w;
        }

        public static int GetSize()
        {
            return 4;
        }

        public static implicit operator Vector4(Vector4Byte v)
        {
            return new Vector4(v.x, v.y, v.z, v.w);
        }

        public static implicit operator Vector3(Vector4Byte v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator Vector4Byte(Vector4 v)
        {
            return new Vector4Byte(
                (byte)v.X,
                (byte)v.Y,
                (byte)v.Z,
                (byte)v.W
            );
        }

        public static implicit operator Vector4Byte(Vector3 v)
        {
            return new Vector4Byte(
                (byte)v.X,
                (byte)v.Y,
                (byte)v.Z,
                255
            );
        }

        public static Vector4Byte operator *(Vector4Byte v, float scalar)
        {
            return new Vector4Byte(
                (byte)(v.x * scalar),
                (byte)(v.y * scalar),
                (byte)(v.z * scalar),
                (byte)(v.w * scalar)
            );
        }

        public static Vector4Byte operator /(Vector4Byte v, float scalar)
        {
            return new Vector4Byte(
                (byte)(v.x / scalar),
                (byte)(v.y / scalar),
                (byte)(v.z / scalar),
                (byte)(v.w / scalar)
            );
        }

        public static Vector4Byte operator *(Vector4Byte v, Vector4Byte b)
        {
            return new Vector4Byte(
                (byte)(v.x * b.x),
                (byte)(v.y * b.y),
                (byte)(v.z * b.z),
                (byte)(v.w * b.w)
            );
        }

        public static Vector4Byte operator /(Vector4Byte v, Vector4Byte b)
        {
            return new Vector4Byte(
                (byte)(v.x / b.x),
                (byte)(v.y / b.y),
                (byte)(v.z / b.z),
                (byte)(v.w / b.w)
            );
        }

        public override string ToString()
        {
            return "<" + x + ", " + y + ", " + z + ", " + w + ">";
        }
    }
}
