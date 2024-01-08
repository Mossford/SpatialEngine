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
        public static Vector4 ApplyMatrix(Vector4 self, Matrix4x4 matrix)
        {
            return new Vector4(
                matrix.M11 * self.X + matrix.M12 * self.Y + matrix.M13 * self.Z + matrix.M14 * self.W,
                matrix.M21 * self.X + matrix.M22 * self.Y + matrix.M23 * self.Z + matrix.M24 * self.W,
                matrix.M31 * self.X + matrix.M32 * self.Y + matrix.M33 * self.Z + matrix.M34 * self.W,
                matrix.M41 * self.X + matrix.M42 * self.Y + matrix.M43 * self.Z + matrix.M44 * self.W
            );
        }

        public static float Vector3Angle(Vector3 a, Vector3 b)
        {
            float dotProduct = Vector3.Dot(a, b);
            float magnitudeA = a.Length();
            float magnitudeB = b.Length();

            // Avoid division by zero
            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return 0f;
            }

            float cosTheta = dotProduct / (magnitudeA * magnitudeB);
            float thetaRad = (float)MathF.Acos(cosTheta);

            // Convert angle from radians to degrees
            return thetaRad * (180f / MathF.PI);
        }
    }
}
