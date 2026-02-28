using System;
using System.Numerics;
using JoltPhysicsSharp;
using SpatialEngine.Rendering;

namespace SpatialEngine
{
    public struct RigidBody : IDisposable
    {
        public BodyID rbID;
        public Body body;
        public BodyCreationSettings settings;

        public Activation activation;

        public ObjectLayer layer;
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
            this.layer = layer;
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
            this.layer = layer;
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
                vertices[i] = mesh.vertexes[i].position * mesh.scale;
            }

            //checked volume calculation is correct here
            // icosphere subdividon 5 gives a pi value of 3.139
            // cube gives 7.9999 which is close to the actual 8 it should be with a length of 2

            //we need to calculate volume as the bindings or jolt dont have a volume I can grab
            float tempVolume = 0f;
            for (int i = 0; i < mesh.indices.Length; i += 3)
            {
                tempVolume += Vector3.Dot(vertices[mesh.indices[i]] * mesh.scale, Vector3.Cross(vertices[mesh.indices[i + 1]] * mesh.scale, vertices[mesh.indices[i + 2]] * mesh.scale)) / 6.0f;
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
            this.layer = layer;
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
                    Vector3[] vertices = new Vector3[Globals.currentScene.GetSpatialObject(meshId).mesh.vertexes.Length];
                    for (int i = 0; i < Globals.currentScene.GetSpatialObject(meshId).mesh.vertexes.Length; i++)
                    {
                        vertices[i] = Globals.currentScene.GetSpatialObject(meshId).mesh.vertexes[i].position;
                    }
                    ConvexHullShapeSettings shape = new ConvexHullShapeSettings(vertices);
                    shape.Density = mass / volume;
                    Double3 pos = (Double3)GetPosition();
                    Quaternion rot = GetRotation();
                    MotionType motion = body.MotionType;
                    ObjectLayer layer = Globals.bodyInterface.GetObjectLayer(rbID);
                    Globals.bodyInterface.RemoveBody(rbID);
                    Globals.bodyInterface.DestroyBody(rbID);
                    body = Globals.bodyInterface.CreateBody(new BodyCreationSettings(shape, pos, rot, motion, layer));
                    rbID = body.ID;
                    Globals.bodyInterface.AddBody(rbID, activation);
                    break;
                }
                case 0:
                {
                    BoxShapeSettings shape = new BoxShapeSettings(halfBoxSize);
                    shape.Density = mass / volume;
                    Double3 pos = (Double3)GetPosition();
                    Quaternion rot = GetRotation();
                    MotionType motion = body.MotionType;
                    ObjectLayer layer = Globals.bodyInterface.GetObjectLayer(rbID);
                    Globals.bodyInterface.RemoveBody(rbID);
                    Globals.bodyInterface.DestroyBody(rbID);
                    body = Globals.bodyInterface.CreateBody(new BodyCreationSettings(shape, pos, rot, motion, layer));
                    rbID = body.ID;
                    Globals.bodyInterface.AddBody(rbID, activation);
                    break;
                }
                case 1:
                {
                    SphereShapeSettings shape = new SphereShapeSettings(radius);
                    shape.Density = mass / volume;
                    Double3 pos = (Double3)GetPosition();
                    Quaternion rot = GetRotation();
                    MotionType motion = body.MotionType;
                    ObjectLayer layer = Globals.bodyInterface.GetObjectLayer(rbID);
                    Globals.bodyInterface.RemoveBody(rbID);
                    Globals.bodyInterface.DestroyBody(rbID);
                    body = Globals.bodyInterface.CreateBody(new BodyCreationSettings(shape, pos, rot, motion, layer));
                    rbID = body.ID;
                    Globals.bodyInterface.AddBody(rbID, activation);
                    break;
                }
                case 2:
                {
                    Vector3[] vertices = new Vector3[Globals.currentScene.GetSpatialObject(meshId).mesh.vertexes.Length];
                    for (int i = 0; i < Globals.currentScene.GetSpatialObject(meshId).mesh.vertexes.Length; i++)
                    {
                        vertices[i] = Globals.currentScene.SpatialObjects[meshId].mesh.vertexes[i].position;
                    }
                    ConvexHullShapeSettings shape = new ConvexHullShapeSettings(vertices);
                    shape.Density = mass / volume;
                    Double3 pos = (Double3)GetPosition();
                    Quaternion rot = GetRotation();
                    MotionType motion = body.MotionType;
                    ObjectLayer layer = Globals.bodyInterface.GetObjectLayer(rbID);
                    Globals.bodyInterface.RemoveBody(rbID);
                    Globals.bodyInterface.DestroyBody(rbID);
                    body = Globals.bodyInterface.CreateBody(new BodyCreationSettings(shape, pos, rot, motion, layer));
                    rbID = body.ID;
                    Globals.bodyInterface.AddBody(rbID, activation);
                    break;
                }
            }
        }

        public void AddForce(Vector3 dir, float power)
        {
            Globals.bodyInterface.AddForce(rbID, dir * power);
        }

        public void AddForceAtPos(Vector3 dir, Vector3 pos, float power)
        {
            Globals.bodyInterface.AddForceAndTorque(rbID, dir * power, Vector3.Cross(pos, dir));
        }
        public void AddImpulseForce(Vector3 dir, float power)
        {
            Globals.bodyInterface.AddLinearVelocity(rbID, dir * power);
        }

        public void SetPosition(Vector3 vec)
        {
            Globals.bodyInterface.SetPosition(rbID, (Double3)vec, activation);
        }

        public void SetRotation(Vector3 vec)
        {
            Quaternion quat = new Quaternion(Vector3.Normalize(vec * 180 / MathF.PI), 1.0f);
            Globals.bodyInterface.SetRotation(rbID, quat, activation);
        }

        public void SetRotation(Quaternion quat)
        {
            Globals.bodyInterface.SetRotation(rbID, quat, activation);
        }

        public Vector3 GetPosition()
        {
            return Globals.bodyInterface.GetPosition(rbID);
        }

        public Quaternion GetRotation()
        {
            return Globals.bodyInterface.GetRotation(rbID);
        }

        public Vector3 GetVelocity()
        {
            return Globals.bodyInterface.GetLinearVelocity(rbID);
        }

        public Vector3 GetAngVelocity()
        {
            return Globals.bodyInterface.GetAngularVelocity(rbID);
        }

        public Vector3 GetPointVelocity(Vector3 pos)
        {
            return Globals.bodyInterface.GetPointVelocity(rbID, (Double3)pos);
        }

        public void SetVelocity(Vector3 vec)
        {
            Globals.bodyInterface.SetLinearVelocity(rbID, vec);
        }

        public void SetAngularVelocity(Vector3 vec)
        {
            Globals.bodyInterface.SetAngularVelocity(rbID, vec);
        }

        public void AddVelocity(Vector3 vec)
        {
            Globals.bodyInterface.AddLinearVelocity(rbID, vec);
        }

        public void RemoveFromPhysics()
        {
            Globals.bodyInterface.RemoveBody(rbID);
        }

        public void Dispose()
        {
            if (Globals.bodyInterface.IsActive(rbID))
            {
                Console.WriteLine("test");
                Globals.bodyInterface.RemoveBody(rbID);
                Globals.bodyInterface.DestroyBody(rbID);
            }
        }
    }
}