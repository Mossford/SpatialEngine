using JoltPhysicsSharp;
using System;
using System.Numerics;

namespace SpatialEngine.Rendering
{
    public class Camera
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 direction;
        public float zoom;
        public float sensitivity = 0.2f;
        public float aspect;
        public float nearPlane;
        public float farPlane;

        public Matrix4x4 viewMat;
        public Matrix4x4 projMat;
        public Matrix4x4 projCloseMat;

        public Frustum frustum;

        public Camera(Vector3 position, Vector3 rotation, float zoom = 60.0f, float nearPlane = 0.1f, float farPlane = 1000f)
        {
            this.position = position;
            this.rotation = rotation;
            direction = GetCamDir();
            this.zoom = zoom;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;
            this.aspect = Window.size.X / Window.size.Y;

            frustum = new Frustum();
            frustum.CreateFromCamera(this);
        }

        public void Update()
        {
            this.aspect = Window.size.X / Window.size.Y;
            frustum.CreateFromCamera(this);
        }

        public Vector3 GetCamDir()
        {
            Vector3 target;
            target.X = -MathF.Sin(rotation.X*(MathF.PI/180.0f)) * MathF.Cos(rotation.Y*(MathF.PI/180.0f));
            target.Y = -MathF.Sin(rotation.Y*(MathF.PI/180.0f));
            target.Z = MathF.Cos(rotation.X*(MathF.PI/180.0f)) * MathF.Cos(rotation.Y*(MathF.PI/180.0f));
            return Vector3.Normalize(target);
        }

        public Vector3 GetCameraUp()
        {
            return Vector3.Cross(GetCamDir(), Vector3.Normalize(Vector3.Cross(Vector3.UnitY, GetCamDir())));
        }

        public void SetViewMat()
        {
            viewMat = Matrix4x4.CreateLookAt(position, GetCamDir() + position, GetCameraUp());
        }

        public void SetProjMat(float winX, float winY)
        {
            projMat = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180.0f * zoom, winX / winY, nearPlane, farPlane);
        }

        public void SetProjMatClose(float winX, float winY)
        {
            //used for depth buffer and for a more close range to get more data
            //not meant for full rendering
            projCloseMat = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180.0f * zoom, winX / winY, 1f, 2000f);
        }
    }
}