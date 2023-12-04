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
            target.X = -MathF.Sin(rotation.X*(3.14159265358979323846f/180.0f)) * MathF.Cos((rotation.Y)*(3.14159265358979323846f/180.0f));
            target.Y = -MathF.Sin((rotation.Y)*(3.14159265358979323846f/180.0f));
            target.Z = MathF.Cos((rotation.X)*(3.14159265358979323846f/180.0f)) * MathF.Cos((rotation.Y)*(3.14159265358979323846f/180.0f));
            return Vector3.Normalize(target);
        }

        public Vector3 GetCameraUp()
        {
            return Vector3.Cross(GetCamDir(), Vector3.Normalize(Vector3.Cross(Vector3.UnitY, GetCamDir())));
        }

        public Matrix4x4 GetViewMat()
        {
            return Matrix4x4.CreateLookAt(position, position + GetCamDir(), GetCameraUp());
        }

        public Matrix4x4 GetProjMat(float winX, float winY)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180.0f * zoom, winX / winY, 0.1f, 10000.0f);
        }
        
    }
}