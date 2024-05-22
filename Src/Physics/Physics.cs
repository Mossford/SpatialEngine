using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.Emit;
using System.Threading;
using JoltPhysicsSharp;
using Silk.NET.Input;


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
                if (obj.SO_rigidbody is not null && bodyInterface.IsActive(obj.SO_rigidbody.rbID))
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
                if (scene.SpatialObjects[i].SO_rigidbody is not null)
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
                if (scene.SpatialObjects[i].SO_rigidbody is not null)
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
        //needed to be able to set the mass
        float volume;
        //0 is a cube
        //1 is a sphere
        //2 is a mesh
        int shapeType = -1;

        Vector3 halfBoxSize = Vector3.Zero;
        float radius = 0;
        int meshId;

        public RigidBody(BodyCreationSettings settings)
        {
            this.settings = settings;
        }

        public RigidBody(Vector3 halfBoxSize, Vector3 position, Quaternion rotation, MotionType motion, ObjectLayer layer, float mass = -1f)
        {
            this.halfBoxSize = halfBoxSize;
            BoxShapeSettings shape = new BoxShapeSettings(halfBoxSize);
            volume = halfBoxSize.X * halfBoxSize.Y * halfBoxSize.Z;
            shapeType = 0;
            if (mass != -1f)
            {
                shape.Density = mass / volume;
            }
            settings = new BodyCreationSettings(new BoxShapeSettings(halfBoxSize), position, rotation, motion, layer);
        }

        public RigidBody(float radius, Vector3 position, Quaternion rotation, MotionType motion, ObjectLayer layer, float mass = -1f)
        {
            this.radius = radius;
            SphereShape shape = new SphereShape(radius);
            volume = 4f / 3f * MathF.PI * (radius * radius * radius);
            shapeType = 1;
            if (mass != -1f)
            {
                shape.Density = mass / volume;
            }
            settings = new BodyCreationSettings(shape, position, rotation, motion, layer);
        }

        public RigidBody(in Mesh mesh, int id, Vector3 position, Quaternion rotation, MotionType motion, ObjectLayer layer, float mass = -1f)
        {
            Vector3[] vertices = new Vector3[mesh.vertexes.Length];
            for (int i = 0; i < mesh.vertexes.Length; i++)
            {
                vertices[i] = mesh.vertexes[i].position;
            }

            //checked volume calculation is correct here
            // icosphere subdividon 5 gives a pi value of 3.139
            // cube gives 7.9999 which is close to the actual 8 it should be with a length of 2

            //we need to calculate volume as the bindings or jolt dont have a volume I can grab
            float tempVolume = 0f;
            for (int i = 0; i < mesh.indices.Length; i += 3)
            {
                tempVolume += Vector3.Dot(vertices[mesh.indices[i]], Vector3.Cross(vertices[mesh.indices[i + 1]], vertices[mesh.indices[i + 2]])) / 6.0f;
            }
            volume = tempVolume;
            shapeType = 2;
            meshId = id;
            ConvexHullShapeSettings shape = new ConvexHullShapeSettings(vertices);
            if (mass != -1f)
            {
                shape.Density = mass / volume;
            }

            settings = new BodyCreationSettings(shape, position, rotation, motion, layer);
        }

        public void AddToPhysics(ref BodyInterface bodyInterface, Activation activation)
        {
            body = bodyInterface.CreateBody(settings);
            rbID = body.ID;
            bodyInterface.AddBody(rbID, activation);
            this.activation = activation;
        }

        public void SetMass(float mass)
        {
            //since I cannot find a way to set the mass and the only thing that works is setting the density to change the mass
            // we will work backwards we have the target mass we put in and our starting volume. If we divide those both we get 
            // the needed density to set our mass

            // mass = density * volume
            // density = mass / volume

            //I dont want to do it this way but it is so hard and setting the mass wont do shit
            //so we are going to do it and i am not going to explain anythign
            switch (shapeType)
            {
                default:
                    {
                        Vector3[] vertices = new Vector3[scene.SpatialObjects[meshId].SO_mesh.vertexes.Length];
                        for (int i = 0; i < scene.SpatialObjects[meshId].SO_mesh.vertexes.Length; i++)
                        {
                            vertices[i] = scene.SpatialObjects[meshId].SO_mesh.vertexes[i].position;
                        }
                        ConvexHullShapeSettings shape = new ConvexHullShapeSettings(vertices);
                        shape.Density = mass / volume;
                        Double3 pos = (Double3)GetPosition();
                        Quaternion rot = GetRotation();
                        MotionType motion = body.MotionType;
                        ObjectLayer layer = bodyInterface.GetObjectLayer(rbID);
                        bodyInterface.DestroyBody(rbID);
                        body = bodyInterface.CreateBody(new BodyCreationSettings(shape, pos, rot, motion, layer));
                        rbID = body.ID;
                        bodyInterface.AddBody(rbID, activation);
                        break;
                    }
                case 0:
                    {
                        BoxShapeSettings shape = new BoxShapeSettings(halfBoxSize);
                        shape.Density = mass / volume;
                        Double3 pos = (Double3)GetPosition();
                        Quaternion rot = GetRotation();
                        MotionType motion = body.MotionType;
                        ObjectLayer layer = bodyInterface.GetObjectLayer(rbID);
                        bodyInterface.DestroyBody(rbID);
                        body = bodyInterface.CreateBody(new BodyCreationSettings(shape, pos, rot, motion, layer));
                        rbID = body.ID;
                        bodyInterface.AddBody(rbID, activation);
                        break;
                    }
                case 1:
                    {
                        SphereShapeSettings shape = new SphereShapeSettings(radius);
                        shape.Density = mass / volume;
                        Double3 pos = (Double3)GetPosition();
                        Quaternion rot = GetRotation();
                        MotionType motion = body.MotionType;
                        ObjectLayer layer = bodyInterface.GetObjectLayer(rbID);
                        bodyInterface.DestroyBody(rbID);
                        body = bodyInterface.CreateBody(new BodyCreationSettings(shape, pos, rot, motion, layer));
                        rbID = body.ID;
                        bodyInterface.AddBody(rbID, activation);
                        break;
                    }
                case 2:
                    {
                        Vector3[] vertices = new Vector3[scene.SpatialObjects[meshId].SO_mesh.vertexes.Length];
                        for (int i = 0; i < scene.SpatialObjects[meshId].SO_mesh.vertexes.Length; i++)
                        {
                            vertices[i] = scene.SpatialObjects[meshId].SO_mesh.vertexes[i].position;
                        }
                        ConvexHullShapeSettings shape = new ConvexHullShapeSettings(vertices);
                        shape.Density = mass / volume;
                        Double3 pos = (Double3)GetPosition();
                        Quaternion rot = GetRotation();
                        MotionType motion = body.MotionType;
                        ObjectLayer layer = bodyInterface.GetObjectLayer(rbID);
                        bodyInterface.DestroyBody(rbID);
                        body = bodyInterface.CreateBody(new BodyCreationSettings(shape, pos, rot, motion, layer));
                        rbID = body.ID;
                        bodyInterface.AddBody(rbID, activation);
                        break;
                    }
            }
        }

        public void AddForce(Vector3 dir, float power)
        {
            bodyInterface.AddForce(rbID, dir * power);
        }

        public void AddForceAtPos(Vector3 dir, Vector3 pos, float power)
        {
            bodyInterface.AddForceAndTorque(rbID, dir * power, Vector3.Cross(pos, dir));
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
            Quaternion quat = new Quaternion(Vector3.Normalize(vec * 180 / MathF.PI), 1.0f);
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

        public Vector3 GetPointVelocity(Vector3 pos)
        {
            return bodyInterface.GetPointVelocity(rbID, (Double3)pos);
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