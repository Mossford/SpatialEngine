using System;
using System.Diagnostics;
using System.Numerics;
using JoltPhysicsSharp;

namespace SpatialEngine
{

    public class Layers
    {
        public static ObjectLayer NON_MOVING = 0;
        public static ObjectLayer MOVING = 1;
        public static ObjectLayer NUM_LAYERS = 2;
    }

    public class BroadPhaseLayers
    {
        public static BroadPhaseLayer NON_MOVING = 0;
	    public static BroadPhaseLayer MOVING = 1;
	    public static uint NUM_LAYERS = 2;
    }

    public class ObjectLayerPairFilterImpl : ObjectLayerPairFilter
    {
        protected override bool ShouldCollide(ObjectLayer inObject1, ObjectLayer inObject2)
        {
            switch (inObject1)
            {
            case 0: // non moving
                return inObject2 == Layers.MOVING; // Non moving only collides with moving
            case 1: // moving
                return true; // Moving collides with everything
            default:
                return false;
            }
        }
    }

    public class ObjectVsBroadPhaseLayerFilterImpl : ObjectVsBroadPhaseLayerFilter
    {
        protected override bool ShouldCollide(ObjectLayer inLayer1, BroadPhaseLayer inLayer2)
        {
            switch (inLayer1)
            {
            case 0: // non moving
                return inLayer2 == BroadPhaseLayers.MOVING;
            case 1: // moving
                return true;
            default:
                return false;
            }
        }
    }

    public class BPLayerInterfaceImpl : BroadPhaseLayerInterface
    {

        BroadPhaseLayer[] mObjectToBroadPhase = new BroadPhaseLayer[Layers.NUM_LAYERS];

        public BPLayerInterfaceImpl()
        {
            // Create a mapping table from object to broad phase layer
            mObjectToBroadPhase[Layers.NON_MOVING] = BroadPhaseLayers.NON_MOVING;
            mObjectToBroadPhase[Layers.MOVING] = BroadPhaseLayers.MOVING;
        }

        protected override int GetNumBroadPhaseLayers()
        {
            return (int)BroadPhaseLayers.NUM_LAYERS;
        }

        protected override BroadPhaseLayer GetBroadPhaseLayer(ObjectLayer inLayer)
        {
            return mObjectToBroadPhase[inLayer];
        }

        protected override string GetBroadPhaseLayerName(BroadPhaseLayer inLayer)
        {
            switch (inLayer)
            {
            case 0: // non moving	
                return "NON_MOVING";
            case 1: // moving		
                return "MOVING";
            default:													
                return "INVALID";
            }
        }
    }

    public unsafe class Physics
    {

        static TempAllocator tempAllocator;
        static readonly int maxJobs = 4;
        static readonly int maxJobBarriers = 4;
        static JobSystemThreadPool jobSystem;

        static readonly uint maxObjects = 65536;
        static readonly uint maxObjectMutex = 0;
        static readonly uint maxObjectPairs = 65536;
        static readonly uint maxContactConstraints = 10240;

        BPLayerInterfaceImpl layerInterface;
        ObjectVsBroadPhaseLayerFilterImpl objectVsBroadPhaseLayer;
        ObjectLayerPairFilterImpl objectLayerPair;
        PhysicsSystem physicsSystem;
        BodyInterface bodyInterface;

        BodyID sphereid;
        Body floor;

        public void InitPhysics()
        {
            Foundation.Init(false);
            tempAllocator = new TempAllocator(10 * 1024 * 1024);
            jobSystem = new JobSystemThreadPool(maxJobs, maxJobBarriers, Process.GetCurrentProcess().Threads.Count);
            physicsSystem = new PhysicsSystem();
            layerInterface = new BPLayerInterfaceImpl();
            objectVsBroadPhaseLayer = new ObjectVsBroadPhaseLayerFilterImpl();
            objectLayerPair = new ObjectLayerPairFilterImpl();
            physicsSystem.Init(maxObjects, maxObjectMutex, maxObjectPairs, maxContactConstraints, layerInterface, objectVsBroadPhaseLayer, objectLayerPair);
            bodyInterface = physicsSystem.BodyInterface;
            BoxShapeSettings floor_shape_settings = new BoxShapeSettings(new Vector3(50, 1, 50));
            BodyCreationSettings floorSettings = new BodyCreationSettings(floor_shape_settings, Vector3.Zero, Quaternion.Identity, MotionType.Static, Layers.NON_MOVING);
            floor = bodyInterface.CreateBody(floorSettings);
            bodyInterface.AddBody(floor.ID, Activation.DontActivate);
            BodyCreationSettings sphereSettings = new BodyCreationSettings(new SphereShape(0.5f), new Vector3(0,5,0), Quaternion.Identity, MotionType.Dynamic, Layers.MOVING);
            sphereid = bodyInterface.CreateAndAddBody(sphereSettings, Activation.Activate);
            bodyInterface.SetLinearVelocity(sphereid, new Vector3(0,-5,0));
            physicsSystem.Gravity = new Vector3(0, -9.81f, 0);
        }

        public void UpdatePhysics(ref Scene scene, float dt)
        {
            if(bodyInterface.IsActive(sphereid))
            {
                scene.SpatialObjects[1].SO_mesh.position = bodyInterface.GetCenterOfMassPosition(sphereid);
                Console.WriteLine(bodyInterface.GetCenterOfMassPosition(sphereid));
                physicsSystem.Update(dt, 1, tempAllocator, jobSystem);
            }
        }

        public void CleanPhysics()
        {
            bodyInterface.RemoveBody(sphereid);
            bodyInterface.DestroyBody(sphereid);

            bodyInterface.RemoveBody(floor.ID);
            bodyInterface.DestroyBody(floor.ID);

            tempAllocator.Dispose();
            Foundation.Shutdown();
        }
    }
}