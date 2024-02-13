using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using JoltPhysicsSharp;

using SpatialEngine;
using static SpatialEngine.Globals;
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Math;
using static SpatialEngine.Resources;

namespace PlaneGame
{
    public static class GameManager
    {
        public static Plane plane;

        public static void InitGame()
        {
            plane = new Plane(scene.SpatialObjects.Count);
            scene.AddSpatialObject(LoadModel(new Vector3(0, 10, 0), Vec3ToQuat(new Vector3(0,0,100)), ModelPath + "Plane.obj"), MotionType.Dynamic, Layers.MOVING, Activation.Activate);
        }

        public static void UpdateGame(float dt)
        {
            plane.Update(dt);
            //scene.SpatialObjects[plane.id].SO_rigidbody.SetVelocity(plane.velocity);
        }
    }
}
