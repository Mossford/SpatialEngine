using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using JoltPhysicsSharp;

//engine stuff
using SpatialEngine.Rendering;
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

            ObjectLayerPairFilterTable objectLayerPairFilterTable = new(2);
            objectLayerPairFilterTable.EnableCollision(Layers.NON_MOVING, Layers.MOVING);
            objectLayerPairFilterTable.EnableCollision(Layers.MOVING, Layers.MOVING);

            // We use a 1-to-1 mapping between object layers and broadphase layers
            BroadPhaseLayerInterfaceTable broadPhaseLayerInterfaceTable = new(2, 2);
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
                if(obj.SO_rigidbody != null && bodyInterface.IsActive(obj.SO_rigidbody.rbID))
                {
                    obj.SO_mesh.position = bodyInterface.GetPosition(obj.SO_rigidbody.rbID);
                    obj.SO_mesh.rotation = bodyInterface.GetRotation(obj.SO_rigidbody.rbID);
                }
            }
            physicsSystem.Step(dt, 3);
        }
        
        public void DestroyPhysics(ref Scene scene)
        {
            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                if (scene.SpatialObjects[i].SO_rigidbody != null)
                {
                    bodyInterface.DestroyBody(scene.SpatialObjects[i].SO_rigidbody.rbID);
                }
            }

            Foundation.Shutdown();
        }

        public void CleanPhysics(ref Scene scene)
        {
            for (int i = 0; i < scene.SpatialObjects.Count; i++)
            {
                if(scene.SpatialObjects[i].SO_rigidbody != null)
                {
                    bodyInterface.DestroyBody(scene.SpatialObjects[i].SO_rigidbody.rbID);
                }
            }
        }
    }

    public class RigidBody
    {
        public BodyID rbID;
        public Body body;
        public BodyCreationSettings settings;

        Activation activation;

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
            SphereShape shape = new SphereShape(radius);
            shape.Density = 1;
            settings = new BodyCreationSettings(shape, position, rotation, motion, layer);
        }

        public RigidBody(in Vertex[] vertexes, Vector3 position, Quaternion rotation, MotionType motion, ObjectLayer layer)
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
            body = bodyInterface.CreateBody(settings);
            rbID = body.ID;
            bodyInterface.AddBody(rbID, 0);
            this.activation = activation;
        }

        public void SetMass(float mass)
        {
            MassProperties massProp = new MassProperties();
            massProp.ScaleToMass(mass);
            body.MotionProperties.SetMassProperties(settings.AllowedDOFs, new MassProperties());
        }

        public void AddForce(Vector3 dir, float power)
        {
            bodyInterface.AddForce(rbID, dir * power);
        }

        public void AddImpulseForce(Vector3 dir, float power)
        {
            bodyInterface.AddLinearVelocity(rbID, dir * power);
        }

        public void SetPosition(Double3 vec)
        {
            bodyInterface.SetPosition(rbID, vec, Activation.Activate);
        }

        public void SetRotation(Vector3 vec)
        {
            Quaternion quat = new Quaternion(Vector3.Normalize(vec * 180/MathF.PI), 1.0f);
            bodyInterface.SetRotation(rbID, quat, Activation.Activate);
        }

        public void SetRotation(Quaternion quat)
        {
            bodyInterface.SetRotation(rbID, quat, Activation.Activate);
        }

        public Vector3 GetPosition()
        {
            return bodyInterface.GetPosition(rbID);
        }

        public Quaternion GetRotation()
        {
            return bodyInterface.GetRotation(rbID);
        }

        public Vector3 GetVelocity()
        {
            return bodyInterface.GetLinearVelocity(rbID);
        }

        public Vector3 GetAngVelocity()
        {
            return bodyInterface.GetAngularVelocity(rbID);
        }

        public void SetVelocity(Vector3 vec)
        {
            bodyInterface.SetLinearVelocity(rbID, vec);
        }

        public void SetAngularVelocity(Vector3 vec)
        {
            bodyInterface.SetAngularVelocity(rbID, vec);
        }

        public void AddVelocity(Vector3 vec)
        {
            bodyInterface.AddLinearVelocity(rbID, vec);
        }

    }
}