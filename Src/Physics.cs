using System.Diagnostics;
using System.Numerics;
using JoltPhysicsSharp;

//engine stuff
using static SpatialEngine.Globals;

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
        static JobSystemThreadPool jobSystem;

        static readonly uint maxObjects = 65536;
        static readonly uint maxObjectMutex = 0;
        static readonly uint maxObjectPairs = 65536;
        static readonly uint maxContactConstraints = 10240;

        BPLayerInterfaceImpl layerInterface;
        ObjectVsBroadPhaseLayerFilterImpl objectVsBroadPhaseLayer;
        ObjectLayerPairFilterImpl objectLayerPair;

        public void InitPhysics()
        {
            Foundation.Init(false);
            tempAllocator = new TempAllocator(10 * 1024 * 1024);
            jobSystem = new JobSystemThreadPool(Foundation.MaxPhysicsJobs, Foundation.MaxPhysicsBarriers, Process.GetCurrentProcess().Threads.Count);
            physicsSystem = new PhysicsSystem();
            layerInterface = new BPLayerInterfaceImpl();
            objectVsBroadPhaseLayer = new ObjectVsBroadPhaseLayerFilterImpl();
            objectLayerPair = new ObjectLayerPairFilterImpl();
            physicsSystem.Init(maxObjects, maxObjectMutex, maxObjectPairs, maxContactConstraints, layerInterface, objectVsBroadPhaseLayer, objectLayerPair);
            physicsSystem.OptimizeBroadPhase();
            bodyInterface = physicsSystem.BodyInterface;
        }

        public void UpdatePhysics(ref Scene scene, float dt)
        {
            foreach (SpatialObject obj in scene.SpatialObjects)
            {
                if(obj.SO_RigidBody != null && bodyInterface.IsActive(obj.SO_RigidBody.rbID))
                {
                    obj.SO_mesh.position = bodyInterface.GetCenterOfMassPosition(obj.SO_RigidBody.rbID);
                    obj.SO_mesh.rotation = new Vector3(bodyInterface.GetRotation(obj.SO_RigidBody.rbID).X, bodyInterface.GetRotation(obj.SO_RigidBody.rbID).Y, bodyInterface.GetRotation(obj.SO_RigidBody.rbID).Z);
                }
            }
            physicsSystem.Update(dt, 1, tempAllocator, jobSystem);
        }

        public void CleanPhysics(ref Scene scene)
        {
            foreach (SpatialObject obj in scene.SpatialObjects)
            {
                if(obj.SO_RigidBody != null)
                {
                    bodyInterface.RemoveBody(obj.SO_RigidBody.rbID);
                    bodyInterface.DestroyBody(obj.SO_RigidBody.rbID);
                }
            }

            tempAllocator.Dispose();
            Foundation.Shutdown();
        }
    }

    public class RigidBody
    {
        public BodyID rbID;
        public BodyCreationSettings settings;

        public RigidBody(BodyCreationSettings settings)
        {
            this.settings = settings;
        }

        public RigidBody(Vector3 halfBoxSize, Vector3 position, Quaternion rotation, MotionType motion, ObjectLayer layer)
        {
            settings = new BodyCreationSettings(new BoxShapeSettings(halfBoxSize), position, rotation, motion, layer);
        }

        public RigidBody(float radius, Vector3 position, Quaternion rotation, MotionType motion, ObjectLayer layer)
        {
            settings = new BodyCreationSettings(new SphereShape(radius), position, rotation, motion, layer);
        }

        public RigidBody(Vertex[] vertexes, Vector3 position, Quaternion rotation, MotionType motion, ObjectLayer layer)
        {
            Vector3[] vertices = new Vector3[vertexes.Length];
            for (int i = 0; i < vertexes.Length; i++)
            {
                vertices[i] = vertexes[i].position;
            }
            settings = new BodyCreationSettings(new ConvexHullShapeSettings(vertices), position, rotation, motion, layer);
        }

        public void AddToPhysics(ref BodyInterface bodyInterface, Activation activation)
        {
            rbID = bodyInterface.CreateAndAddBody(settings, activation);
        }
    }
}