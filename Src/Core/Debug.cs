using JoltPhysicsSharp;
using Silk.NET.Maths;
using System.Numerics;
using Silk.NET.OpenGL;
using static SpatialEngine.Globals;
using static SpatialEngine.Resources;
using Shader = SpatialEngine.Rendering.Shader;
using System.Collections.Generic;

namespace SpatialEngine
{
    public static class Debugging
    {
        static Matrix4x4 m_proj;
        static Matrix4x4 m_view;

        static List<uint> vaos = new List<uint>();
        static List<uint> vbos = new List<uint>();
        static List<uint> drawSizes = new List<uint>();

        static Shader lineShader = new Shader(gl, ShaderPath + "DebugDrawing.vert", ShaderPath + "DebugDrawing.frag");


        enum DebugTypes
        {
            Line = 2,
            Triangle = 3,
            Cube = 36,
            Sphere = 60,
        };

        public static void SetNeededDebug(Matrix4x4 proj, Matrix4x4 view)
        {
            m_proj = proj;
            m_view = view;
        }

        public unsafe static void DrawDebugLine(Vector3 start, Vector3 end, Vector3 color)
        {
            uint vbo, vao;
            float[] points = new float[12];

            points[0] = start.X;
            points[1] = start.Y;
            points[2] = start.Z;
            points[3] = color.X;
            points[4] = color.Y;
            points[5] = color.Z;

            points[6] = end.X;
            points[7] = end.Y;
            points[8] = end.Z;
            points[9] = color.X;
            points[10] = color.Y;
            points[11] = color.Z;

            vbo = gl.GenBuffer();
            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* v = &points[0])
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(points.Length * sizeof(uint)), v, BufferUsageARB.StaticDraw);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));
            gl.BindVertexArray(0);

            vaos.Add(vao);
            vbos.Add(vbo);
            drawSizes.Add(2);
        }

        public unsafe static void DrawDebugTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 color)
        {
            uint vbo, vao;
            float[] points = new float[18];

            points[0] = a.X;
            points[1] = a.Y;
            points[2] = a.Z;
            points[3] = color.X;
            points[4] = color.Y;
            points[5] = color.Z;

            points[6] = b.X;
            points[7] = b.Y;
            points[8] = b.Z;
            points[9] = color.X;
            points[10] = color.Y;
            points[11] = color.Z;

            points[12] = c.X;
            points[13] = c.Y;
            points[14] = c.Z;
            points[15] = color.X;
            points[16] = color.Y;
            points[17] = color.Z;

            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* v = &points[0])
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(points.Length * sizeof(uint)), v, BufferUsageARB.StaticDraw);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));
            gl.BindVertexArray(0);

            vaos.Add(vao);
            vbos.Add(vbo);
            drawSizes.Add(3);
        }

        public unsafe static void DrawDebugCube(Vector3 pos, float scale, Vector3 color)
        {
            uint vbo, vao;
            float[] vertices = 
                {
            -scale + pos.X, -scale + pos.Y,  scale + pos.Z, color.X, color.Y, color.Z,
            -scale + pos.X,  scale + pos.Y,  scale + pos.Z, color.X, color.Y, color.Z,
            -scale + pos.X, -scale + pos.Y, -scale + pos.Z, color.X, color.Y, color.Z,
            -scale + pos.X,  scale + pos.Y, -scale + pos.Z, color.X, color.Y, color.Z,
             scale + pos.X, -scale + pos.Y,  scale + pos.Z, color.X, color.Y, color.Z,
             scale + pos.X,  scale + pos.Y,  scale + pos.Z, color.X, color.Y, color.Z,
             scale + pos.X, -scale + pos.Y, -scale + pos.Z, color.X, color.Y, color.Z,
             scale + pos.X,  scale + pos.Y, -scale + pos.Z, color.X, color.Y, color.Z
                };
            uint[] indices = 
                {
                1, 2, 0,
                3, 6, 2,
                7, 4, 6,
                5, 0, 4,
                6, 0, 2,
                3, 5, 7,
                1, 3, 2,
                3, 7, 6,
                7, 5, 4,
                5, 1, 0,
                6, 4, 0,
                3, 1, 5
                };

            float[] points = new float[6 * 36];
            for (int i = 0; i < 36; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    points[j + 6 * i] = vertices[indices[i] * 6 + j];
                }
            }

            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* v = &points[0])
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(points.Length * sizeof(uint)), v, BufferUsageARB.StaticDraw);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * (uint)sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * (uint)sizeof(float), (void*)(3 * sizeof(float)));
            gl.BindVertexArray(0);

            vaos.Add(vao);
            vbos.Add(vbo);
            drawSizes.Add(36);
        }

        public static void DrawDebugItems()
        {
            for (int i = 0; i < vaos.Count; i++)
            {
                gl.UseProgram(lineShader.shader);
                lineShader.setMat4("proj", m_proj);
                lineShader.setMat4("view", m_view);
                gl.Disable(GLEnum.DepthTest);
                gl.BindVertexArray(vaos[i]);
                if (drawSizes[i] == (int)DebugTypes.Line)
                    gl.DrawArrays(GLEnum.Lines, 0, drawSizes[i]);
                if (drawSizes[i] == (int)DebugTypes.Cube || drawSizes[i] == (int)DebugTypes.Triangle || drawSizes[i] == (int)DebugTypes.Sphere)
                    gl.DrawArrays(GLEnum.Triangles, 0, drawSizes[i]);
                gl.BindVertexArray(0);
                gl.Enable(GLEnum.DepthTest);
                gl.DeleteBuffer(vbos[i]);
                gl.DeleteVertexArray(vaos[i]);
            }
            vaos.Clear();
            vbos.Clear();
            drawSizes.Clear();
        }
    }
}