using JoltPhysicsSharp;
using System;
using System.Diagnostics;
using System.Numerics;
using SpatialEngine.Networking.Packets;
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
