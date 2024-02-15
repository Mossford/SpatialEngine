using JoltPhysicsSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Resources;
using PlaneGame;

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

            Globals.physics = new Physics();
            Globals.scene = new Scene();
            Globals.physics.InitPhysics();

            Globals.scene.AddSpatialObject(LoadModel(new Vector3(0, 0, 0), Quaternion.Identity, ModelPath, "Floor.obj"), new Vector3(50, 1, 50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            Globals.scene.AddSpatialObject(LoadModel(new Vector3(50, 30, 0), Quaternion.Identity, ModelPath, "FloorWall1.obj"), new Vector3(1, 30, 50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            Globals.scene.AddSpatialObject(LoadModel(new Vector3(0, 10, 50), Quaternion.Identity, ModelPath, "FloorWall2.obj"), new Vector3(50, 10, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            Globals.scene.AddSpatialObject(LoadModel(new Vector3(25, 5, 0), Quaternion.Identity, ModelPath, "FloorWall3.obj"), new Vector3(1, 5, 20), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            Globals.scene.AddSpatialObject(LoadModel(new Vector3(37, 4, 21), Quaternion.Identity, ModelPath, "FloorWall4.obj"), new Vector3(13, 4, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            Globals.scene.AddSpatialObject(LoadModel(new Vector3(37, 5, -21), Quaternion.Identity, ModelPath, "FloorWall5.obj"), new Vector3(13, 4, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            Globals.scene.AddSpatialObject(LoadModel(new Vector3(-50, 2, 0), Quaternion.Identity, ModelPath, "FloorWall6.obj"), new Vector3(1, 2, 50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            Globals.scene.AddSpatialObject(LoadModel(new Vector3(-30, 3, -50), Quaternion.Identity, ModelPath, "FloorWall7.obj"), new Vector3(20, 3, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);

            GameManager.InitGame();

            while (true)
            {
                currentTime = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).Microseconds / 1000000f;

                totalTimeUpdate += deltaTime;
                //Console.WriteLine(totalTimeUpdate);
                //while (totalTimeUpdate >= 0.0166f)
                //{
                    //Console.WriteLine(deltaTime);
                    //totalTimeUpdate -= 0.0166f;
                    Update(0.0166f);
                //}
                deltaTime = currentTime - lastTime;
                lastTime = currentTime;
            }
        }

        public static void Update(float dt)
        {
            Globals.physics.UpdatePhysics(ref Globals.scene, dt);
            NetworkManager.server.Update(dt);
        }
    }
}
