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

        public Matrix4x4 GetViewMat()
        {
            return Matrix4x4.CreateLookAt(position, position + new Vector3(0,0,1), Vector3.UnitY);
        }

        public Matrix4x4 GetProjMat(float winX, float winY)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180.0f * zoom, winX / winY, 0.1f, 10000.0f);
        }
        
    }
}