using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Linq;
using JoltPhysicsSharp;
using Silk.NET.OpenGL;

using Silk.NET.SDL;
using static Silk.NET.Core.Native.WinString;



//engine stuff
using static SpatialEngine.Globals;
using SpatialEngine.Rendering;

namespace SpatialEngine
{
    public struct SpatialObject
    {
        public Mesh SO_mesh;
        public RigidBody SO_rigidbody;
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

        public void AddSpatialObject(Mesh mesh, MotionType motion, ObjectLayer layer, Activation activation)
        {
            if(mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObject obj = new SpatialObject(mesh, (uint)id);
            obj.SO_rigidbody = new RigidBody(obj.SO_mesh.vertexes, obj.SO_mesh.position, obj.SO_mesh.rotation, motion, layer);
            SpatialObjects.Add(obj);
            idList.Add((uint)id);
            SpatialObjects[id].SO_rigidbody.AddToPhysics(ref bodyInterface, activation);
        }

        public void AddSpatialObject(Mesh mesh, Vector3 halfBoxSize, MotionType motion, ObjectLayer layer, Activation activation)
        {
            if(mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObject obj = new SpatialObject(mesh, (uint)id);
            obj.SO_rigidbody = new RigidBody(halfBoxSize, obj.SO_mesh.position, obj.SO_mesh.rotation, motion, layer);
            SpatialObjects.Add(obj);
            idList.Add((uint)id);
            SpatialObjects[id].SO_rigidbody.AddToPhysics(ref bodyInterface, activation);
        }

        public void AddSpatialObject(Mesh mesh, float radius, MotionType motion, ObjectLayer layer, Activation activation)
        {
            if(mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObject obj = new SpatialObject(mesh, (uint)id);
            obj.SO_rigidbody = new RigidBody(radius, obj.SO_mesh.position, obj.SO_mesh.rotation, motion, layer);
            SpatialObjects.Add(obj);
            idList.Add((uint)id);
            SpatialObjects[id].SO_rigidbody.AddToPhysics(ref bodyInterface, activation);
        }

        public void DeleteObjects()
        {

        }

        public void SaveScene()
        {

        }

        public void LoadScene()
        {

        }
    }
}