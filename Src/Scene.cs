using System;
using System.Collections.Generic;
using System.Numerics;
using JoltPhysicsSharp;
using Silk.NET.OpenGL;

using Silk.NET.SDL;


//engine stuff
using static SpatialEngine.Globals;

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

        public uint vao { get; protected set; }
        public uint vbo { get; protected set; }
        public uint ebo { get; protected set; }
        Vertex[] vertexes;
        uint[] indices;

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
            obj.SO_rigidbody = new RigidBody(obj.SO_mesh.vertexes, obj.SO_mesh.position, obj.SO_mesh.rotation, motion, layer);
            SpatialObjects.Add(obj);
            SpatialObjects[id].SO_mesh.BufferGens();
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
            SpatialObjects[id].SO_mesh.BufferGens();
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
            SpatialObjects[id].SO_mesh.BufferGens();
            idList.Add((uint)id);
            SpatialObjects[id].SO_rigidbody.AddToPhysics(ref bodyInterface, activation);
        }

        public void DrawSingle(ref Shader shader, Matrix4x4 view, Matrix4x4 proj, Vector3 camPos)
        {
            shader.setMat4("view", view);
            shader.setMat4("projection", proj);
            shader.setVec3("viewPos", camPos);
            for (int i = 0; i < SpatialObjects.Count; i++)
            {
                shader.setMat4("model", SpatialObjects[i].SO_mesh.modelMat);
                SpatialObjects[i].SO_mesh.DrawMesh();
            }
        }

        public unsafe void DrawScene(ref Shader shader, Matrix4x4 view, Matrix4x4 proj, Vector3 camPos)
        {
            gl.BindVertexArray(vao);
            shader.setMat4("view", view);
            shader.setMat4("projection", proj);
            shader.setVec3("viewPos", camPos);
            int offset = 0;
            for (int i = 0; i < SpatialObjects.Count; i++)
            {
                for (int g = i; g < i; g++)
                {
                    offset += SpatialObjects[g].SO_mesh.indices.Length;
                }
                shader.setMat4("model", SpatialObjects[i].SO_mesh.modelMat);
                gl.DrawElementsBaseVertex(GLEnum.Triangles, (uint)indices.Length, GLEnum.UnsignedInt, (void*)0, offset);
            }
        }

        public unsafe void CreateBufferGenScene()
        {

            int vertexSize = 0;
            int indiceSize = 0;
            for (int i = 0; i < SpatialObjects.Count; i++)
            {
                vertexSize += SpatialObjects[i].SO_mesh.vertexes.Length;
                indiceSize += SpatialObjects[i].SO_mesh.indices.Length;
            }

            if (vertexes == null || indices == null)
            {
                vertexes = new Vertex[0];
                indices = new uint[0];
            }

            if (vertexes.Length != vertexSize)
            {
                vertexes = new Vertex[vertexSize];
                indices = new uint[indiceSize];
                for (int i = 0; i < SpatialObjects.Count; i++)
                {
                    for (int j = 0; j < SpatialObjects[i].SO_mesh.vertexes.Length; j++)
                    {
                        vertexes[j + SpatialObjects[i].SO_mesh.vertexes.Length * i] = SpatialObjects[i].SO_mesh.vertexes[j];
                    }
                    for (int j = 0; j < SpatialObjects[i].SO_mesh.indices.Length; j++)
                    {
                        indices[j + SpatialObjects[i].SO_mesh.indices.Length * i] = SpatialObjects[i].SO_mesh.indices[j];
                    }
                }
            }

            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = vertexes)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexes.Length * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
            fixed (uint* buf = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
            gl.BindVertexArray(0);

        }

        public unsafe void UpdateBufferGenScene()
        {

            int vertexSize = 0;
            int indiceSize = 0;
            for (int i = 0; i < SpatialObjects.Count; i++)
            {
                vertexSize += SpatialObjects[i].SO_mesh.vertexes.Length;
                indiceSize += SpatialObjects[i].SO_mesh.indices.Length;
            }

            if (vertexes == null || indices == null)
            {
                vertexes = new Vertex[0];
                indices = new uint[0];
            }

            if (vertexes.Length != vertexSize)
            {
                vertexes = new Vertex[vertexSize];
                indices = new uint[indiceSize];
                for (int i = 0; i < SpatialObjects.Count; i++)
                {
                    for (int j = 0; j < SpatialObjects[i].SO_mesh.vertexes.Length; j++)
                    {
                        vertexes[j + SpatialObjects[i].SO_mesh.vertexes.Length * i] = SpatialObjects[i].SO_mesh.vertexes[j];
                    }
                    for (int j = 0; j < SpatialObjects[i].SO_mesh.indices.Length; j++)
                    {
                        indices[j + SpatialObjects[i].SO_mesh.indices.Length * i] = SpatialObjects[i].SO_mesh.indices[j];
                    }
                }
            }

            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = vertexes)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexes.Length * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
            fixed (uint* buf = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            gl.BindVertexArray(0);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

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