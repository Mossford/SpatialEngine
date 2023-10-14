using System;
using System.Numerics;

namespace SpatialEngine
{
    public class Camera
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 target;
        public Vector3 direction;
        public float zoom;

        public Camera(Vector3 position, Quaternion rotation, Vector3 target, float zoom)
        {
            this.position = position;
            this.rotation = rotation;
            this.target = target;
            direction = GetCamDir();
            this.zoom = zoom;
        }

        public Vector3 GetCamDir()
        {
            return Vector3.Normalize(position - target);
        }

        public Vector3 GetCameraRight()
        {
            return Vector3.Normalize(Vector3.Cross(Vector3.UnitY, direction));
        }

        public Vector3 GetCameraFront()
        {
            return Vector3.Cross(Vector3.UnitY, GetCameraRight());
        }

        Matrix4x4 MatrixLookAt(Vector3 eye, Vector3 target, Vector3 up)
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
            result.M41 = -Vector3.Dot(vx, eye);
            result.M42 = -Vector3.Dot(vy, eye);
            result.M43 = -Vector3.Dot(vz, eye);
            result.M44 = 1.0f;

            return result;
        }

        Matrix4x4 MatrixPerspective(float fovY, float aspect, float nearPlane, float farPlane)
        {
            Matrix4x4 result = Matrix4x4.Identity;

            float top = nearPlane*MathF.Tan(fovY*0.5f);
            float bottom = -top;
            float right = top*aspect;
            float left = -right;

            float rl = right - left;
            float tb = top - bottom;
            float fn = farPlane - nearPlane;

            result.M11 = nearPlane*2.0f/rl;
            result.M22 = nearPlane*2.0f/tb;
            result.M31 = (right + left)/rl;
            result.M32 = (top + bottom)/tb;
            result.M33 = -(farPlane + nearPlane)/fn;
            result.M34 = -1.0f;
            result.M43 = -(farPlane*nearPlane*2.0f)/fn;

            return result;
        }

        public Matrix4x4 GetViewMat()
        {
            return MatrixLookAt(position, position + new Vector3(0,0,1), Vector3.UnitY); 
        }

        public Matrix4x4 GetProjMat()
        {
            return MatrixPerspective(MathF.PI / 180 * zoom, 1.77777f, 0.0001f, 100f);
        }
        
    }
}