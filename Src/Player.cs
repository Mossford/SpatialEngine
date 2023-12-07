using System;
using System.Numerics;
using System.Security.Cryptography;
using Silk.NET.Input;

namespace SpatialEngine
{
    public class Player
    {
        public Camera camera;
        Vector3 moveDir;
        public Vector3 position;
        public Vector3 rotation;
        public float speed;

        public Player(float speed, Vector3 position, Vector3 rotation)
        {
            this.speed = speed;
            this.position = position;
            this.rotation = rotation;
            camera = new Camera(position, rotation);
        }

        public void UpdatePlayer(float delta)
        {
            position += moveDir * delta * speed;
            camera.position = position;
            camera.rotation = rotation;
            moveDir = Vector3.Zero;
        }

        public void Movement(float delta, int[] keys)
        {
            Vector3 up = Vector3.UnitY;
            Vector3 down = -Vector3.UnitY;
            foreach (int key in keys)
            {
                switch (key)
                {
                case 87: // key W
                    moveDir = camera.GetCamDir();
                    moveDir = Vector3.Normalize(moveDir);
                    UpdatePlayer(delta);
                    break;
                case 83: // key S
                    moveDir = -camera.GetCamDir();
                    moveDir = Vector3.Normalize(moveDir);
                    UpdatePlayer(delta);
                    break;
                case 65: // key A
                    moveDir = -Vector3.Cross(camera.GetCamDir(), camera.GetCameraUp());
                    moveDir = Vector3.Normalize(moveDir);
                    UpdatePlayer(delta);
                    break;
                case 68: // key D
                    moveDir = Vector3.Cross(camera.GetCamDir(), camera.GetCameraUp());
                    moveDir = Vector3.Normalize(moveDir);
                    UpdatePlayer(delta);
                    break;
                case 32: // key Space
                    moveDir = up;
                    moveDir = Vector3.Normalize(moveDir);
                    UpdatePlayer(delta);
                    break;
                case 340: // key Left Shift
                    moveDir = down;
                    moveDir = Vector3.Normalize(moveDir);
                    UpdatePlayer(delta);
                    break;
                }
            }
        }

        public void Look(int x, int y, bool leftP, bool rightP)
        {
            rotation.X += x * camera.sensitivity;
            rotation.Y += y * camera.sensitivity;
            if(rotation.Y > 89.0f)
                rotation.Y =  89.0f;
            if(rotation.Y < -89.0f)
                rotation.Y = -89.0f;
        }
    }
}