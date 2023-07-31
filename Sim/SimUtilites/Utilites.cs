using System;
using System.Numerics;
using GameTesting;

namespace GameTestingUtilites
{

    public struct Ray
    {
        public Vector3 origin;
        public Vector3 direction;
    }

    public static class MathExt
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

        public static Vector3 ApplyMatrixVec3Proj(Vector3 self, Matrix4x4 proj, Matrix4x4 view)
        {
            Matrix4x4 mat = (MultiplyMat(proj, view));
            float a00 = mat.M11, a01 = mat.M12, a02 = mat.M13, a03 = mat.M14;
            float a10 = mat.M21, a11 = mat.M22, a12 = mat.M23, a13 = mat.M24;
            float a20 = mat.M31, a21 = mat.M32, a22 = mat.M33, a23 = mat.M34;
            float a30 = mat.M41, a31 = mat.M42, a32 = mat.M43, a33 = mat.M44;
            float b00 = a00*a11 - a01*a10;
            float b01 = a00*a12 - a02*a10;
            float b02 = a00*a13 - a03*a10;
            float b03 = a01*a12 - a02*a11;
            float b04 = a01*a13 - a03*a11;
            float b05 = a02*a13 - a03*a12;
            float b06 = a20*a31 - a21*a30;
            float b07 = a20*a32 - a22*a30;
            float b08 = a20*a33 - a23*a30;
            float b09 = a21*a32 - a22*a31;
            float b10 = a21*a33 - a23*a31;
            float b11 = a22*a33 - a23*a32;
            float invDet = 1.0f/(b00*b11 - b01*b10 + b02*b09 + b03*b08 - b04*b07 + b05*b06);
            mat = new Matrix4x4(
                (a11*b11 - a12*b10 + a13*b09)*invDet,
                (-a01*b11 + a02*b10 - a03*b09)*invDet,
                (a31*b05 - a32*b04 + a33*b03)*invDet,
                (-a21*b05 + a22*b04 - a23*b03)*invDet,
                (-a10*b11 + a12*b08 - a13*b07)*invDet,
                (a00*b11 - a02*b08 + a03*b07)*invDet,
                (-a30*b05 + a32*b02 - a33*b01)*invDet,
                (a20*b05 - a22*b02 + a23*b01)*invDet,
                (a10*b10 - a11*b08 + a13*b06)*invDet,
                (-a00*b10 + a01*b08 - a03*b06)*invDet,
                (a30*b04 - a31*b02 + a33*b00)*invDet,
                (-a20*b04 + a21*b02 - a23*b00)*invDet,
                (-a10*b09 + a11*b07 - a12*b06)*invDet,
                (a00*b09 - a01*b07 + a02*b06)*invDet,
                (-a30*b03 + a31*b01 - a32*b00)*invDet,
                (a20*b03 - a21*b01 + a22*b00)*invDet 
            );
            Vector4 vec = ApplyMatrix(new Vector4(self, 1f), mat);
            return new Vector3(vec.X / vec.W, vec.Y / vec.W, vec.Z / vec.W);
        }

        public static Matrix4x4 MultiplyMat(Matrix4x4 a, Matrix4x4 b)
        {
            return new Matrix4x4(
                a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
                a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
                a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
                a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,
                a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
                a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
                a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
                a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,
                a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
                a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
                a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
                a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,
                a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
                a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
                a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
                a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44
            );
        }

        public static Matrix4x4 MatrixLookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            Matrix4x4 result = Matrix4x4.Identity;

            Vector3 vz = eye - target;
            float length = vz.Length();
            if (length == 0.0f) length = 1.0f;
                vz /= length;

            Vector3 vx = Vector3.Cross(up, vz);
            length = vx.Length();
            if (length == 0.0f) length = 1.0f;
                vx /= length;

            Vector3 vy = Vector3.Cross(vz, vx);

            result.M11 = vx.X;
            result.M12 = vx.Y;
            result.M13 = vx.Z;
            result.M14 = 0.0f;
            result.M21 = vy.X;
            result.M22 = vy.Y;
            result.M23 = vy.Z;
            result.M24 = 0.0f;
            result.M31 = vz.X;
            result.M32 = vz.Y;
            result.M33 = vz.Z;
            result.M34 = 0.0f;
            result.M41 = -(Vector3.Dot(vx, eye));
            result.M42 = -(Vector3.Dot(vy, eye));
            result.M43 = -(Vector3.Dot(vz, eye));
            result.M44 = 1.0f;

            return result;
        }

        public static Matrix4x4 MatrixPerspective(float fovY, float aspect, float nearPlane, float farPlane)
        {
            Matrix4x4 result = Matrix4x4.Identity;

            float top = nearPlane*MathF.Tan(fovY*0.5f);
            float bottom = -top;
            float right = top*aspect;
            float left = -right;

            // MatrixFrustum(-right, right, -top, top, near, far);
            float rl = right - left;
            float tb = top - bottom;
            float fn = farPlane - nearPlane;

            result.M11 = (nearPlane*2.0f)/rl;
            result.M22 = (nearPlane*2.0f)/tb;
            result.M31 = (right + left)/rl;
            result.M32 = (top + bottom)/tb;
            result.M33 = -(farPlane + nearPlane)/fn;
            result.M34 = -1.0f;
            result.M43 = -(farPlane*nearPlane*2.0f)/fn;

            return result;
        }

    }

    public class BoundingBox
    {
        public Vector3 max;
        public Vector3 min;

        public BoundingBox(float[] vertexes, Vector3 pos)
        {
            ConstructBoundingBox(vertexes, pos);
        }

        public void ConstructBoundingBox(float[] vertexes, Vector3 pos)
        {
            int vertTot = vertexes.Length / 3;
            float xMax = float.MinValue;
            float yMax = float.MinValue;
            float zMax = float.MinValue;
            float xMin = float.MaxValue;
            float yMin = float.MaxValue;
            float zMin = float.MaxValue;

            float vertX = 0;
            float vertY = 0;
            float vertZ = 0;

            for (int i = 0; i < vertTot; i++)
            {
                vertX = vertexes[i * 3] + pos.X;
                vertY = vertexes[i * 3 + 1] + pos.Y;
                vertZ = vertexes[i * 3 + 2] + pos.Z;
                if(vertX < xMin)
                    xMin = vertX;
                if(vertY < yMin)
                    yMin = vertY;
                if(vertZ < zMin)
                    zMin = vertZ;
                if(vertX > xMax)
                    xMax = vertX;
                if(vertY > yMax)
                    yMax = vertY;
                if(vertZ > zMax)
                    zMax = vertZ;
            }

            max = new Vector3(xMax, yMax, zMax);
            min = new Vector3(xMin, yMin, zMin);
        }

        public bool PointInisdeBounds(Vector3 pos)
        {
            return pos.X < max.X && pos.Y < max.Y && pos.Z < max.Z && pos.X > min.X && pos.Y > min.Y && pos.Z > min.Z;
        }

        public bool RayInsideBounds(Ray ray)
        {
            float t1 = (min.X - ray.origin.X) * ( 1 / ray.direction.X);
            float t2 = (max.X - ray.origin.X) * ( 1 / ray.direction.X);

            float tmin = MathF.Min(t1, t2);
            float tmax = MathF.Max(t1, t2);

            t1 = (min.X - ray.origin.X) * (1 / ray.direction.X);
            t2 = (max.X - ray.origin.X) * (1 / ray.direction.X);

            tmin = MathF.Max(tmin, MathF.Min(t1, t2));
            tmax = MathF.Min(tmax, MathF.Max(t1, t2));

            t1 = (min.Y - ray.origin.Y) * (1 / ray.direction.Y);
            t2 = (max.Y - ray.origin.Y) * (1 / ray.direction.Y);

            tmin = MathF.Max(tmin, MathF.Min(t1, t2));
            tmax = MathF.Min(tmax, MathF.Max(t1, t2));

            t1 = (min.Z - ray.origin.Z) * (1 / ray.direction.Z);
            t2 = (max.Z - ray.origin.Z) * (1 / ray.direction.Z);

            tmin = MathF.Max(tmin, MathF.Min(t1, t2));
            tmax = MathF.Min(tmax, MathF.Max(t1, t2));

            return tmax > MathF.Max(tmin, 0.0f);
        }
    }
}
