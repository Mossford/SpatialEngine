using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using JoltPhysicsSharp;

using SpatialEngine;
using SpatialEngine.Rendering;
using SpatialEngine.Terrain;
using static SpatialEngine.Globals;
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Resources;
using static SpatialEngine.Terrain.Terrain;

namespace SpatialGame
{
    public static class GameManager
    {

        public static void InitGame()
        {
            scene.AddSpatialObject(LoadModel(new Vector3(0, 0, 0), Quaternion.Identity, "Floor.obj"), new Vector3(50, 1, 50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(50, 30, 0), Quaternion.Identity, "FloorWall1.obj"), new Vector3(1, 30, 50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(0, 10, 50), Quaternion.Identity, "FloorWall2.obj"), new Vector3(50, 10, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(25, 5, 0), Quaternion.Identity, "FloorWall3.obj"), new Vector3(1, 5, 20), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(37, 4, 21), Quaternion.Identity, "FloorWall4.obj"), new Vector3(13, 4, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(37, 5, -21), Quaternion.Identity, "FloorWall5.obj"), new Vector3(13, 4, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(-50, 2, 0), Quaternion.Identity, "FloorWall6.obj"), new Vector3(1, 2, 50), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.AddSpatialObject(LoadModel(new Vector3(-30, 3, -50), Quaternion.Identity, "FloorWall7.obj"), new Vector3(20, 3, 1), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);

            //Terrain test = new Terrain(64, 64, 1, 1);

        }

        public static void UpdateGame(float dt)
        {
            GameInput.Update(dt);
        }

        public static void FixedUpdateGame(float dt)
        {

        }
    }
}
