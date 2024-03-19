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
using Shader = SpatialEngine.Rendering.Shader;

namespace SpatialEngine
{
    public struct SpatialObject
    {
        public Mesh SO_mesh;
        public RigidBody SO_rigidbody;
        public Shader SO_shader;
        public uint SO_id;

        public SpatialObject(Mesh mesh, uint id)
        {
            SO_mesh = mesh;
            SO_id = id;
        }

        public SpatialObject(RigidBody rigidBody, Mesh mesh, uint id)
        {
            SO_rigidbody = rigidBody;
            SO_mesh = mesh;
            SO_id = id;
        }

        public SpatialObject(RigidBody rigidBody, Mesh mesh, Shader shader, uint id)
        {
            SO_rigidbody = rigidBody;
            SO_shader = shader;
            SO_mesh = mesh;
            SO_id = id;
        }

        public SpatialObject(RigidBody rigidBody, Mesh mesh, string vertPath, string fragPath, uint id)
        {
            SO_rigidbody = rigidBody;
            SO_shader = new Shader(gl, vertPath, fragPath);
            SO_mesh = mesh;
            SO_id = id;
        }

        public SpatialObject(Mesh mesh, MotionType motion, ObjectLayer layer, Activation activation, uint id)
        {
            SO_mesh = mesh;
            SO_rigidbody = new RigidBody(SO_mesh, (int)SO_id, SO_mesh.position, SO_mesh.rotation, motion, layer);
            SO_rigidbody.AddToPhysics(ref bodyInterface, activation);
            SO_id = id;
        }

        public SpatialObject(Mesh mesh, MotionType motion, ObjectLayer layer, Activation activation, string vertPath, string fragPath, uint id)
        {
            SO_mesh = mesh;
            SO_rigidbody = new RigidBody(SO_mesh, (int)SO_id, SO_mesh.position, SO_mesh.rotation, motion, layer);
            SO_shader = new Shader(gl, vertPath, fragPath);
            SO_rigidbody.AddToPhysics(ref bodyInterface, activation);
            SO_id = id;
        }

        public uint GetSizeUsage()
        {
            uint total = 0;
            total += (uint)(8 * sizeof(float) * SO_mesh.vertexes.Length);
            total += (uint)(sizeof(uint) * SO_mesh.indices.Length);
            return total;
        }
    }

    public class Scene
    {

        public List<SpatialObject> SpatialObjects;
        public List<uint> idList;

        public Scene()
        {
            SpatialObjects = new List<SpatialObject>();
            idList = new List<uint>();
        }

        public void AddSpatialObject(Mesh mesh)
        {
            if(mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObjects.Add(new SpatialObject(mesh, (uint)id));
            idList.Add((uint)id);
        }

        public void AddSpatialObject(Mesh mesh, MotionType motion, ObjectLayer layer, Activation activation, float mass = -1)
        {
            if (mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObject obj = new SpatialObject(mesh, (uint)id);
            obj.SO_rigidbody = new RigidBody(obj.SO_mesh, id, obj.SO_mesh.position, obj.SO_mesh.rotation, motion, layer, mass);
            SpatialObjects.Add(obj);
            idList.Add((uint)id);
            SpatialObjects[id].SO_rigidbody.AddToPhysics(ref bodyInterface, activation);
        }

        public void AddSpatialObject(Mesh mesh, Vector3 halfBoxSize, MotionType motion, ObjectLayer layer, Activation activation, float mass = -1)
        {
            if (mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObject obj = new SpatialObject(mesh, (uint)id);
            obj.SO_rigidbody = new RigidBody(halfBoxSize, obj.SO_mesh.position, obj.SO_mesh.rotation, motion, layer, mass);
            SpatialObjects.Add(obj);
            idList.Add((uint)id);
            SpatialObjects[id].SO_rigidbody.AddToPhysics(ref bodyInterface, activation);
        }

        public void AddSpatialObject(Mesh mesh, float radius, MotionType motion, ObjectLayer layer, Activation activation, float mass = -1)
        {
            if (mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObject obj = new SpatialObject(mesh, (uint)id);
            obj.SO_rigidbody = new RigidBody(radius, obj.SO_mesh.position, obj.SO_mesh.rotation, motion, layer, mass);
            SpatialObjects.Add(obj);
            idList.Add((uint)id);
            SpatialObjects[id].SO_rigidbody.AddToPhysics(ref bodyInterface, activation);
        }

        public void Clear()
        {
            for (int i = 0; i < SpatialObjects.Count; i++)
            {
                bodyInterface.DestroyBody(SpatialObjects[i].SO_rigidbody.rbID);
            }

            SpatialObjects.Clear();
            idList.Clear();
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
                writer.WriteLine("SO " + SpatialObjects[i].SO_id);
                if (SpatialObjects[i].SO_mesh is not null)
                {
                    writer.WriteLine("ML " + SpatialObjects[i].SO_mesh.modelLocation);
                    writer.WriteLine("MP " + SpatialObjects[i].SO_mesh.position.X + "/" + SpatialObjects[i].SO_mesh.position.Y + "/" + SpatialObjects[i].SO_mesh.position.Z);
                    writer.WriteLine("MR " + SpatialObjects[i].SO_mesh.rotation.X + "/" + SpatialObjects[i].SO_mesh.rotation.Y + "/" + SpatialObjects[i].SO_mesh.rotation.Z + "/" + SpatialObjects[i].SO_mesh.rotation.W);
                    writer.WriteLine("MS " + SpatialObjects[i].SO_mesh.scale);
                }
                //writer.Write("TL " + SpatialObjects[i].SO_texture.textLocation);
                if (SpatialObjects[i].SO_rigidbody is not null)
                {
                    Vector3 vel = SpatialObjects[i].SO_rigidbody.GetVelocity();
                    writer.WriteLine("RV " + vel.X.ToString("G30") + "/" + vel.Y.ToString("G30") + "/" + vel.Z.ToString("G30"));
                }
                if (SpatialObjects[i].SO_shader is not null)
                {
                    writer.WriteLine("SLV " + SpatialObjects[i].SO_shader.vertPath);
                    writer.WriteLine("SLF " + SpatialObjects[i].SO_shader.fragPath);
                }
            }

            writer.Close();
        }

        public void LoadScene()
        {

        }
    }
}