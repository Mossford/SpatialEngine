using System.Collections.Generic;
using System.Numerics;
using JoltPhysicsSharp;

//engine stuff
using static SpatialEngine.Globals;

namespace SpatialEngine
{
    public struct SpatialObject
    {
        public Mesh SO_mesh;
        public RigidBody SO_RigidBody;
        public uint SO_id;

        public SpatialObject(Mesh mesh, uint id)
        {
            SO_mesh = mesh;
            SO_id = id;
        }

        public SpatialObject(RigidBody rigidBody, Mesh mesh, uint id)
        {
            SO_RigidBody = rigidBody;
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
            SpatialObjects[id].SO_mesh.BufferGens();
            idList.Add((uint)id);
        }

        public void AddSpatialObject(Mesh mesh, MotionType motion, ObjectLayer layer, Activation activation)
        {
            if(mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObject obj = new SpatialObject(mesh, (uint)id);
            obj.SO_RigidBody = new RigidBody(obj.SO_mesh.vertexes, obj.SO_mesh.position, obj.SO_mesh.rotation, motion, layer);
            SpatialObjects.Add(obj);
            SpatialObjects[id].SO_mesh.BufferGens();
            idList.Add((uint)id);
            SpatialObjects[id].SO_RigidBody.AddToPhysics(ref bodyInterface, activation);
        }

        public void AddSpatialObject(Mesh mesh, Vector3 halfBoxSize, MotionType motion, ObjectLayer layer, Activation activation)
        {
            if(mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObject obj = new SpatialObject(mesh, (uint)id);
            obj.SO_RigidBody = new RigidBody(halfBoxSize, obj.SO_mesh.position, obj.SO_mesh.rotation, motion, layer);
            SpatialObjects.Add(obj);
            SpatialObjects[id].SO_mesh.BufferGens();
            idList.Add((uint)id);
            SpatialObjects[id].SO_RigidBody.AddToPhysics(ref bodyInterface, activation);
        }

        public void AddSpatialObject(Mesh mesh, float radius, MotionType motion, ObjectLayer layer, Activation activation)
        {
            if(mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObject obj = new SpatialObject(mesh, (uint)id);
            obj.SO_RigidBody = new RigidBody(radius, obj.SO_mesh.position, obj.SO_mesh.rotation, motion, layer);
            SpatialObjects.Add(obj);
            SpatialObjects[id].SO_mesh.BufferGens();
            idList.Add((uint)id);
            SpatialObjects[id].SO_RigidBody.AddToPhysics(ref bodyInterface, activation);
        }

        public void DrawSingle(ref Shader shader, Matrix4x4 view, Matrix4x4 proj, Vector3 camPos)
        {
            shader.SetUniform("view", view);
            shader.SetUniform("projection", proj);
            shader.SetUniform("viewPos", camPos);
            for (int i = 0; i < SpatialObjects.Count; i++)
            {
                shader.SetUniform("model", SpatialObjects[i].SO_mesh.modelMat);
                SpatialObjects[i].SO_mesh.DrawMesh();
            }
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