using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Linq;
using JoltPhysicsSharp;
using Silk.NET.OpenGL;
using Silk.NET.SDL;

//engine stuff
using static SpatialEngine.Globals;
using SpatialEngine.Rendering;
using System.IO;
using System.Linq;

namespace SpatialEngine
{
    public class Scene
    {

        public List<SpatialObject> SpatialObjects;
        public List<int> idList;
        Stack<int> freeIds;

        public Scene()
        {
            SpatialObjects = new List<SpatialObject>();
            idList = new List<int>();
            freeIds = new Stack<int>();
        }

        public int AddSpatialObject(Mesh mesh)
        {
            int id = 0;
            if (freeIds.Count != 0)
            {
                id = freeIds.Pop();
                idList[id] = SpatialObjects.Count;
            }
            else
            {
                id = idList.Count;
                idList.Add(SpatialObjects.Count);
            }
            
            SpatialObjects.Add(new SpatialObject(mesh, id));
            
            vertCount += (uint)SpatialObjects[^1].mesh.vertexes.Length;
            indCount += (uint)SpatialObjects[^1].mesh.indices.Length;

            return id;
        }

        public int AddSpatialObject(Mesh mesh, MotionType motion, ObjectLayer layer, Activation activation, float mass = -1)
        {
            int id = 0;
            if (freeIds.Count != 0)
            {
                id = freeIds.Pop();
                idList[id] = SpatialObjects.Count;
            }
            else
            {
                id = idList.Count;
                idList.Add(SpatialObjects.Count);
            }
            
            SpatialObject obj = new SpatialObject(mesh, id);
            obj.rigidbody = new RigidBody(obj.mesh, id, obj.mesh.position, obj.mesh.rotation, motion, layer, mass);
            SpatialObjects.Add(obj);
            SpatialObjects[^1].rigidbody.AddToPhysics(ref bodyInterface, activation);
            
            vertCount += (uint)SpatialObjects[^1].mesh.vertexes.Length;
            indCount += (uint)SpatialObjects[^1].mesh.indices.Length;
            
            return id;
        }

        public int AddSpatialObject(Mesh mesh, Vector3 halfBoxSize, MotionType motion, ObjectLayer layer, Activation activation, float mass = -1)
        {
            int id = 0;
            if (freeIds.Count != 0)
            {
                id = freeIds.Pop();
                idList[id] = SpatialObjects.Count;
            }
            else
            {
                id = idList.Count;
                idList.Add(SpatialObjects.Count);
            }
            
            SpatialObject obj = new SpatialObject(mesh, id);
            obj.rigidbody = new RigidBody(halfBoxSize, obj.mesh.position, obj.mesh.rotation, motion, layer, mass);
            SpatialObjects.Add(obj);
            SpatialObjects[^1].rigidbody.AddToPhysics(ref bodyInterface, activation);
            
            vertCount += (uint)SpatialObjects[^1].mesh.vertexes.Length;
            indCount += (uint)SpatialObjects[^1].mesh.indices.Length;
            
            return id;
        }

        public int AddSpatialObject(Mesh mesh, float radius, MotionType motion, ObjectLayer layer, Activation activation, float mass = -1)
        {
            int id = 0;
            if (freeIds.Count != 0)
            {
                id = freeIds.Pop();
                idList[id] = SpatialObjects.Count;
            }
            else
            {
                id = idList.Count;
                idList.Add(SpatialObjects.Count);
            }
            
            SpatialObject obj = new SpatialObject(mesh, id);
            obj.rigidbody = new RigidBody(radius, obj.mesh.position, obj.mesh.rotation, motion, layer, mass);
            SpatialObjects.Add(obj);
            SpatialObjects[^1].rigidbody.AddToPhysics(ref bodyInterface, activation);
            
            vertCount += (uint)SpatialObjects[^1].mesh.vertexes.Length;
            indCount += (uint)SpatialObjects[^1].mesh.indices.Length;
            
            return id;
        }

        public void RemoveSpatialObject(int id)
        {
            if (id < 0 || id >= idList.Count)
                return;
            
            //grab id for object to delete
            int objectId = idList[id];
            
            if (objectId != SpatialObjects.Count)
            {
                //swap object with end
                SpatialObjects[objectId] = SpatialObjects[^1];
                //set swapped end object id ref to object id
                idList[SpatialObjects[objectId].id] = objectId;
            }
            
            //add id to be reused
            freeIds.Push(id);
            idList[id] = -1;
            
            //delete end object
            vertCount -= (uint)SpatialObjects[^1].mesh.vertexes.Length;
            indCount -= (uint)SpatialObjects[^1].mesh.indices.Length;
            
            SpatialObjects[^1].rigidbody.Dispose();
            SpatialObjects[^1].mesh.Dispose();
            SpatialObjects[^1].shader?.Dispose();
            SpatialObjects.RemoveAt(SpatialObjects.Count - 1);
        }

        public void Update()
        {
            for (int i = 0; i < SpatialObjects.Count; i++)
            {
                SpatialObjects[i].mesh.SetModelMatrix();
            }
        }

        public void Clear()
        {
            for (int i = 0; i < SpatialObjects.Count; i++)
            {
                bodyInterface.DestroyBody(SpatialObjects[i].rigidbody.rbID);
            }

            SpatialObjects.Clear();
            idList.Clear();
            freeIds.Clear();
        }

        public SpatialObject GetSpatialObject(int id)
        {
            if (id < 0 || id >= idList.Count || idList[id] == -1)
                return null;
            
            return SpatialObjects[idList[id]];
        }

        public void SaveScene(string location, string name)
        {
            if(!File.Exists(location + name))
            {
                File.Create(location + name);
            }
            else
            {
                File.WriteAllText(location + name, string.Empty);
            }

            StreamWriter writer = new StreamWriter(location + name);

            string info =
                "#Scene File\n" +
                "#Scene and Object Layout\n" +
                "\n" +
                "#S (Scene name)\n" +
                "#SN (number of SpatialObjects)\n" +
                "#SO (object number)\n" +
                "#ML (Mesh location)\n" +
                "#MP (Mesh position.x)/(mesh position.y)/(mesh position.z)\n" +
                "#MR (Mesh rotation.x)/(mesh rotation.y)/(mesh rotation.z)\n" +
                "#MS (mesh scale)\n" +
                "#TL (Texture location)\n" +
                "#RV (Velocity.x)/(Velocity.y)/(Velocity.z)\n" +
                "#SLV (Shader Vertex Location)\n" +
                "#SLF (Shader Fragment Location)\n";

            writer.WriteLine(info);
            writer.WriteLine("S " + name.Remove(name.LastIndexOf('.'), name.Length - name.LastIndexOf('.')));
            writer.WriteLine("SN " + SpatialObjects.Count);

            for (int i = 0; i < SpatialObjects.Count; i++)
            {
                writer.WriteLine("SO " + SpatialObjects[i].id);
                writer.WriteLine("ML " + SpatialObjects[i].mesh.modelLocation);
                writer.WriteLine("MP " + SpatialObjects[i].mesh.position.X + "/" + SpatialObjects[i].mesh.position.Y + "/" + SpatialObjects[i].mesh.position.Z);
                writer.WriteLine("MR " + SpatialObjects[i].mesh.rotation.X + "/" + SpatialObjects[i].mesh.rotation.Y + "/" + SpatialObjects[i].mesh.rotation.Z + "/" + SpatialObjects[i].mesh.rotation.W);
                writer.WriteLine("MS " + SpatialObjects[i].mesh.scale);
                //writer.Write("TL " + SpatialObjects[i].SO_texture.textLocation);
                Vector3 vel = SpatialObjects[i].rigidbody.GetVelocity();
                writer.WriteLine("RV " + vel.X.ToString("G30") + "/" + vel.Y.ToString("G30") + "/" + vel.Z.ToString("G30"));
                if (SpatialObjects[i].shader is not null)
                {
                    writer.WriteLine("SLV " + SpatialObjects[i].shader.vertPath);
                    writer.WriteLine("SLF " + SpatialObjects[i].shader.fragPath);
                }
            }

            writer.Close();
        }

        public void LoadScene()
        {

        }
    }
}