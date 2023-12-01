using System;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SpatialEngine
{

    public struct Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        public Vertex(Vector3 pos, Vector3 nor, Vector2 tex)
        {
            position = pos;
            normal = nor;
            uv = tex;
        }
    }

    public class Mesh : IDisposable
    {
        public Vertex[] vertexes;
        public uint[] indices;
        GL gl;
        public uint vao;
        public uint vbo;
        public uint ebo;

        public Vector3 position = Vector3.Zero; 
        public float scale = 1f;
        public Vector3 rotation = Vector3.Zero;
        public Matrix4x4 modelMat;

        public Mesh(GL gl, Vertex[] vertexes, uint[] indices)
        {
            this.gl = gl;
            this.vertexes = vertexes;
            this.indices = indices;
        }

        ~Mesh()
        {
            //gl.DeleteVertexArray(vao);
            //gl.DeleteBuffer(vbo);
            //gl.DeleteBuffer(ebo);
        }

        public void SetModelMatrix()
        {
            modelMat = Matrix4x4.Identity;
            modelMat = Matrix4x4.CreateTranslation(position) * (Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, rotation.X) * Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, rotation.Y) * Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, rotation.Z)) * Matrix4x4.CreateScale(scale);
        }

        public unsafe void BufferGens()
        {
            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = vertexes)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertexes.Length * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
            fixed (uint* buf = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) 0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) (3 * sizeof(float)));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*) (6 * sizeof(float)));
            gl.BindVertexArray(0);
        }

        public unsafe void DrawMesh()
        {
            gl.BindVertexArray(vao);
            gl.DrawElements(GLEnum.Triangles, (uint)indices.Length, GLEnum.UnsignedInt, (void*) 0);
        }

        public void Dispose()
        {
            gl.DeleteVertexArray(vao);
            gl.DeleteBuffer(vbo);
            gl.DeleteBuffer(ebo);
        }
    }
}