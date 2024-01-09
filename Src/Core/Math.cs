using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpatialEngine
{
    public static class Math
    {
        public static Vector4 ApplyMatrixVec4(Vector4 self, Matrix4x4 matrix)
        {
            return new Vector4(
                matrix.M11 * self.X + matrix.M12 * self.Y + matrix.M13 * self.Z + matrix.M14 * self.W,
                matrix.M21 * self.X + matrix.M22 * self.Y + matrix.M23 * self.Z + matrix.M24 * self.W,
                matrix.M31 * self.X + matrix.M32 * self.Y + matrix.M33 * self.Z + matrix.M34 * self.W,
                matrix.M41 * self.X + matrix.M42 * self.Y + matrix.M43 * self.Z + matrix.M44 * self.W
            );
        }

        public static Vector3 ApplyMatrixVec3(Vector3 self, Matrix4x4 matrix)
        {
            Vector3 vec;
            vec.X = matrix.M11 * self.X + matrix.M12 * self.Y + matrix.M13 * self.Z + matrix.M14;
            vec.Y = matrix.M21 * self.X + matrix.M22 * self.Y + matrix.M23 * self.Z + matrix.M24;
            vec.Z = matrix.M31 * self.X + matrix.M32 * self.Y + matrix.M33 * self.Z + matrix.M34;
            return vec;
        }

        public static float ClampValue(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static float Vector3Angle(Vector3 a, Vector3 b)
        {
            float denominator = MathF.Sqrt(Vector3.Dot(a, a) * Vector3.Dot(b, b));
            if (denominator < 1e-15f)
                return 0f;
            float dot = ClampValue(Vector3.Dot(a, b) / denominator, -1f, 1f);
            return (MathF.Acos(dot)) * 180f/MathF.PI;
        }
    }
}
