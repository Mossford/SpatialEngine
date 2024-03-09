using JoltPhysicsSharp;
using System;
using System.Diagnostics;
using System.Numerics;

using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Resources;
using static SpatialEngine.Globals;
using SpatialGame;

namespace SpatialEngine.Networking
{
    public static class HeadlessServer
    {
        //might get replaced but a server that does not run the graphical components of the engine
        //Should only run the physics and networking with how the server should work

        //make an internal server that will run everything and the client will send information such controls

        static float totalTimeUpdate = 0f;
        static float currentTime = 0f;
        static float deltaTime = 0f;
        static float lastTime = 0f;

        public static void Init()
        {
            NetworkManager.Init();
            NetworkManager.InitServer();

            physics = new Physics();
            scene = new Scene();
            physics.InitPhysics();

            scene.AddSpatialObject(LoadModel(new Vector3(0, 0, 0), Quaternion.Identity, "Floor.obj"), new Vector3(50, 1, 50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(50, 30, 0), Quaternion.Identity, "FloorWall1.obj"), new Vector3(1, 30, 50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(0, 10, 50), Quaternion.Identity, "FloorWall2.obj"), new Vector3(50, 10, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(25, 5, 0), Quaternion.Identity, "FloorWall3.obj"), new Vector3(1, 5, 20), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(37, 4, 21), Quaternion.Identity, "FloorWall4.obj"), new Vector3(13, 4, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(37, 5, -21), Quaternion.Identity, "FloorWall5.obj"), new Vector3(13, 4, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(-50, 2, 0), Quaternion.Identity, "FloorWall6.obj"), new Vector3(1, 2, 50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(-30, 3, -50), Quaternion.Identity, "FloorWall7.obj"), new Vector3(20, 3, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);

            GameManager.InitGame();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            while (true)
            {
                TimeSpan ts = stopWatch.Elapsed;
                currentTime = (float)ts.TotalMilliseconds;

                deltaTime = currentTime - lastTime;

                totalTimeUpdate += deltaTime;
                while (totalTimeUpdate >= 16.667f)
                {
                    totalTimeUpdate -= 16.667f;
                    //needs to be in seconds so will be 0.016667f
                    Update(0.016667f);
                }

                lastTime = (float)ts.TotalMilliseconds;
            }

            stopWatch.Stop();
        }

        public static void Update(float dt)
        {
            physics.UpdatePhysics(ref scene, dt);

            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                SpatialObjectPacket packet = new SpatialObjectPacket(i, scene.SpatialObjects[i].SO_mesh.position, scene.SpatialObjects[i].SO_mesh.rotation);
                NetworkManager.server.SendUnrelibAll(packet);
            }

            NetworkManager.server.Update(dt);
        }
    }
}
