using System;
using System.Numerics;
using JoltPhysicsSharp;
using Silk.NET.Input;

//engine stuff
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Resources;
using SpatialEngine.Rendering;
using static SpatialEngine.Globals;
using SpatialGame;

namespace SpatialEngine
{
    public class Player
    {
        public Camera camera;
        Vector3 moveDir;
        public Vector3 position;
        public Vector3 rotation;
        public float speed;
        int id;

        public Player(float speed, Vector3 position, Vector3 rotation)
        {
            this.speed = speed;
            this.position = position;
            this.rotation = rotation;
            camera = new Camera(position, rotation);
            //id = scene.SpatialObjects.Count;
            //scene.AddSpatialObject(CreateSphereMesh(position, Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z), 2), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
        }

        public void UpdatePlayer(float delta)
        {
            position += moveDir * delta * speed;
            camera.position = position;
            camera.rotation = rotation;
            //scene.SpatialObjects[id].SO_mesh.position = position;
            //scene.SpatialObjects[id].SO_mesh.rotation = Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
            moveDir = Vector3.Zero;
        }

        public void Movement(float delta)
        {
            Vector3 up = Vector3.UnitY;
            Vector3 down = -Vector3.UnitY;
            if (Input.IsKeyDown(Key.W))
            {
                moveDir = camera.GetCamDir();
                moveDir = Vector3.Normalize(moveDir);
                UpdatePlayer(delta);
            }
            if (Input.IsKeyDown(Key.S))
            {
                moveDir = -camera.GetCamDir();
                moveDir = Vector3.Normalize(moveDir);
                UpdatePlayer(delta);
            }
            if (Input.IsKeyDown(Key.A))
            {
                moveDir = -Vector3.Cross(camera.GetCamDir(), camera.GetCameraUp());
                moveDir = Vector3.Normalize(moveDir);
                UpdatePlayer(delta);
            }
            if (Input.IsKeyDown(Key.D))
            {
                moveDir = Vector3.Cross(camera.GetCamDir(), camera.GetCameraUp());
                moveDir = Vector3.Normalize(moveDir);
                UpdatePlayer(delta);
            }
            if (Input.IsKeyDown(Key.Space))
            {
                moveDir = up;
                moveDir = Vector3.Normalize(moveDir);
                UpdatePlayer(delta);
            }
            if (Input.IsKeyDown(Key.ShiftLeft))
            {
                moveDir = down;
                moveDir = Vector3.Normalize(moveDir);
                UpdatePlayer(delta);
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

        public void LaunchObject(ref Scene scene, string name)
        {
            scene.AddSpatialObject(LoadModel(camera.position + (camera.GetCamDir() * 13.0f), Quaternion.Identity, name), MotionType.Dynamic, Layers.MOVING, Activation.Activate);
            scene.SpatialObjects[scene.SpatialObjects.Count - 1].SO_rigidbody.AddImpulseForce(Vector3.Normalize(camera.GetCamDir()), 100.0f);
        }

        public void LaunchCube(ref Scene scene)
        {
            scene.AddSpatialObject(LoadModel(camera.position + (camera.GetCamDir() * 13.0f), Quaternion.Identity, "Cube.obj"), new Vector3(1.0f), MotionType.Dynamic, Layers.MOVING, Activation.Activate);
            scene.SpatialObjects[scene.SpatialObjects.Count - 1].SO_rigidbody.AddImpulseForce(Vector3.Normalize(camera.GetCamDir()), 100.0f);
        }

        public void LaunchSphere(ref Scene scene)
        {
            scene.AddSpatialObject(CreateSphereMesh(camera.position + (camera.GetCamDir() * 13.0f), Quaternion.Identity, 2), 1.0f, MotionType.Dynamic, Layers.MOVING, Activation.Activate);
            scene.SpatialObjects[scene.SpatialObjects.Count - 1].SO_rigidbody.AddImpulseForce(Vector3.Normalize(camera.GetCamDir()), 100.0f);
        }
    }
}