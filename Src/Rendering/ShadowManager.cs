using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using static SpatialEngine.Globals;
using static SpatialEngine.Math;

namespace SpatialEngine.Rendering
{
    public static class ShadowManager
    {
        public static float zMult = 10f;

        public static void DrawShadows(Matrix4x4 proj, Matrix4x4 view, Vector3 lightDir)
        {
            List<Vector4> frustumCorners = new();
            Matrix4x4.Invert(proj * view, out Matrix4x4 inv);

            for (int x = 0; x < 2; ++x)
            {
                for (int y = 0; y < 2; ++y)
                {
                    for (int z = 0; z < 2; ++z)
                    {
                        Vector4 pt = ApplyMatrixVec4(new Vector4(2.0f * x - 1.0f, 2.0f * y - 1.0f, 2.0f * z - 1.0f, 1.0f), inv);
                        frustumCorners.Add(pt / pt.W);
                    }
                }
            }

            Vector3 center = Vector3.Zero;
            foreach(Vector4 pt in frustumCorners)
            {
                center += new Vector3(pt.X, pt.Y, pt.Z);
            }
            center /= frustumCorners.Count;
            Matrix4x4 lightView = Matrix4x4.CreateLookAt(center + lightDir, center, Vector3.UnitY);

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;
            foreach (Vector4 pt in frustumCorners)
            {
                Vector4 trf = ApplyMatrixVec4(pt, lightView);
                minX = float.Min(minX, trf.X);
                maxX = float.Max(maxX, trf.X);
                minY = float.Min(minY, trf.Y);
                maxY = float.Max(maxY, trf.Y);
                minZ = float.Min(minZ, trf.Z);
                maxZ = float.Max(maxZ, trf.Z);
            }

            if (minZ < 0)
            {
                minZ *= zMult;
            }
            else
            {
                minZ /= zMult;
            }
            if (maxZ < 0)
            {
                maxZ /= zMult;
            }
            else
            {
                maxZ *= zMult;
            }

            Matrix4x4 lightProjection = Matrix4x4.CreateOrthographicOffCenter(minX, maxX, minY, maxY, minZ, maxZ);
        }
    }
}
