using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using static SpatialEngine.Globals;
using SpatialEngine.SpatialMath;

namespace SpatialEngine.Rendering
{
    public static class ShadowManager
    {
        public static float zMult = 10f;
        public static float[] cascadeLevels;
        public static int cascadeNum;
        public static int shadowResolution;

        public static void Init(float farPlane, int cascadeNum = 4)
        {
            ShadowManager.cascadeNum = cascadeNum;
            cascadeLevels = new float[cascadeNum];
            shadowResolution = 2048;

            cascadeLevels[0] = farPlane / 50f;
            cascadeLevels[1] = farPlane / 25f;
            cascadeLevels[2] = farPlane / 10f;
            cascadeLevels[3] = farPlane / 2f;
        }

        public static void Update()
        {
            
        }
        
        static Vector4[] GetFrustumCornersWorldSpace(Matrix4x4 proj, Matrix4x4 view)
        {
            Matrix4x4.Invert((proj * view), out Matrix4x4 invertComb);

            Vector4[] corners = new Vector4[8];
            int count = 0;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector4 point = MathS.ApplyMatrixVec4(new Vector4(2.0f * x - 1.0f, 2.0f * y - 1.0f, 2.0f * z - 1.0f, 1.0f), invertComb);
                        corners[count] = point / point.W;
                        count++;
                    }
                }
            }
    
            return corners;
        }

        static Matrix4x4 CreateLightMat(float nearPlane, float farPlane, int width, int height, in Matrix4x4 view, Vector3 lightDir)
        {
            Matrix4x4 proj = Matrix4x4.CreatePerspective(width, height, nearPlane, farPlane);
            Vector4[] corners = GetFrustumCornersWorldSpace(proj, view);

            Vector3 center = Vector3.Zero;
            for (int i = 0; i < corners.Length; i++)
            {
                center += new Vector3(corners[i].X, corners[i].Y, corners[i].Z);
            }
            center /= corners.Length;
            Matrix4x4 lightView = Matrix4x4.CreateLookAt(center + lightDir, center, Vector3.UnitY);

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector4 trf = MathS.ApplyMatrixVec4(corners[i], lightView);
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
            
            return lightProjection * lightView;
        }
        
        static Matrix4x4[] getLightSpaceMatrices(float nearPlane, float farPlane, int width, int height, in Matrix4x4 view, Vector3 lightDir)
        {
            Matrix4x4[] mats = new Matrix4x4[cascadeNum + 1];
            for (int i = 0; i < cascadeNum; ++i)
            {
                if (i == 0)
                {
                    mats[0] = CreateLightMat(nearPlane, farPlane, width, height, view, lightDir);
                }
                else if (i < cascadeNum)
                {
                    mats[i] = CreateLightMat(cascadeLevels[i - 1], cascadeLevels[i], width, height, view, lightDir);
                }
                else
                {
                    mats[i] = CreateLightMat(cascadeLevels[i - 1], farPlane, width, height, view, lightDir);
                }
            }
            return mats;
        }

    }
}
