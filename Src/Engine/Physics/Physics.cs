using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Threading;
using JoltPhysicsSharp;
using Silk.NET.Input;


//engine stuff
using static SpatialEngine.Globals;

namespace SpatialEngine
{
    public class Layers
    {
        public static ObjectLayer NON_MOVING = 0;
        public static ObjectLayer MOVING = 1;
        public static ObjectLayer NONCOLLIDE = 2;
        public static ObjectLayer NUM_LAYERS = 3;
    }

    public class BroadPhaseLayers
    {
        public static BroadPhaseLayer NON_MOVING = 0;
        public static BroadPhaseLayer MOVING = 1;
        public static BroadPhaseLayer NONCOLLIDE = 2;
        public static uint NUM_LAYERS = 3;
    }

    public unsafe class Physics
    {
        static readonly int maxObjects = 65536;
        static readonly int maxObjectMutex = 0;
        static readonly int maxObjectPairs = 65536;
        static readonly int maxContactConstraints = 10240;

        ObjectLayerPairFilter objectLayerPairFilter;
        BroadPhaseLayerInterface broadPhaseLayerInterface;
        ObjectVsBroadPhaseLayerFilter objectVsBroadPhaseLayerFilter;


        public void InitPhysics()
        {
            Foundation.Init(0u, false);

            ObjectLayerPairFilterTable objectLayerPairFilterTable = new(3);
            objectLayerPairFilterTable.DisableCollision(Layers.NONCOLLIDE, Layers.NON_MOVING);
            objectLayerPairFilterTable.EnableCollision(Layers.NON_MOVING, Layers.MOVING);
            objectLayerPairFilterTable.EnableCollision(Layers.MOVING, Layers.MOVING);

            // We use a 1-to-1 mapping between object layers and broadphase layers
            BroadPhaseLayerInterfaceTable broadPhaseLayerInterfaceTable = new(3, 3);
            broadPhaseLayerInterfaceTable.MapObjectToBroadPhaseLayer(Layers.NONCOLLIDE, BroadPhaseLayers.NONCOLLIDE);
            broadPhaseLayerInterfaceTable.MapObjectToBroadPhaseLayer(Layers.NON_MOVING, BroadPhaseLayers.NON_MOVING);
            broadPhaseLayerInterfaceTable.MapObjectToBroadPhaseLayer(Layers.MOVING, BroadPhaseLayers.MOVING);

            objectLayerPairFilter = objectLayerPairFilterTable;
            broadPhaseLayerInterface = broadPhaseLayerInterfaceTable;
            objectVsBroadPhaseLayerFilter = new ObjectVsBroadPhaseLayerFilterTable(broadPhaseLayerInterfaceTable, 2, objectLayerPairFilterTable, 2);

            PhysicsSystemSettings settings = new()
            {
                ObjectLayerPairFilter = objectLayerPairFilter,
                BroadPhaseLayerInterface = broadPhaseLayerInterface,
                ObjectVsBroadPhaseLayerFilter = objectVsBroadPhaseLayerFilter,
                MaxBodies = maxObjects,
                MaxBodyPairs = maxObjectPairs,
                MaxContactConstraints = maxContactConstraints
            };

            physicsSystem = new PhysicsSystem(settings);

            physicsSystem.OptimizeBroadPhase();
            bodyInterface = physicsSystem.BodyInterface;
        }

        public void UpdatePhysics(ref Scene scene, float dt)
        {
            foreach (SpatialObject obj in scene.SpatialObjects)
            {
                if(!obj.enabled)
                    bodyInterface.SetObjectLayer(obj.rigidbody.rbID, Layers.NONCOLLIDE);
                else
                    bodyInterface.SetObjectLayer(obj.rigidbody.rbID, obj.rigidbody.layer);
                    
                
                obj.mesh.position = bodyInterface.GetPosition(obj.rigidbody.rbID);
                obj.mesh.rotation = bodyInterface.GetRotation(obj.rigidbody.rbID);
            }
            physicsSystem.Step(dt, 3);
        }

        public void CleanPhysics(ref Scene scene)
        {
            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                bodyInterface.RemoveBody(scene.SpatialObjects[i].rigidbody.rbID);
                bodyInterface.DestroyBody(scene.SpatialObjects[i].rigidbody.rbID);
            }
            
            Foundation.Shutdown();
        }
    }
}