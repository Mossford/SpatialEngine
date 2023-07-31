using System;
using Silk.NET.OpenGL;
using System.Numerics;

namespace GameTesting
{
    public class Mesh : IDisposable
    {
        public float[] vertexes;
        uint[] indices;
        GL gl;
        public uint vao;
        public uint vbo;
        public uint ebo;

        public Vector3 position = Vector3.Zero; 
        public float scale = 1f;
        public Vector3 rotation = Vector3.Zero;
        public Matrix4x4 modelMat;

        public Mesh(GL gl, float[] vertices, uint[] indices)
        {
            this.gl = gl;
            this.vertexes = vertices;
            this.indices = indices;
            BufferGens();
            CreateViewMat();
        }

        ~Mesh()
        {
            gl.DeleteVertexArray(vao);
            gl.DeleteBuffer(vbo);
            gl.DeleteBuffer(ebo);
        }

        public void CreateViewMat()
        {
            modelMat = Matrix4x4.Identity * (Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, rotation.X) * Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, rotation.Y) * Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, rotation.Z)) * Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position);
        }

        public unsafe void BufferGens()
        {
            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (float* buf = vertexes)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertexes.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
            fixed (uint* buf = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*) 0);
            gl.BindVertexArray(0);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
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