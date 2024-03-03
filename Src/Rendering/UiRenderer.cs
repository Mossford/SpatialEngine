﻿using Silk.NET.OpenGL;
using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpatialEngine.Rendering
{

    public class UiElement : IDisposable
    {
        record UIVertex(Vector3 pos, Vector2 uv);

        //size for the quad
        public float Length;
        public float Height;

        //texture that is displayed
        public Texture texture;

        public uint id { get; private set; }
        uint vbo;
        uint ebo;

        public UiElement(string textureLoc, float length = 1, float height = 1)
        {
            Length = length;
            Height = height;

            texture = new Texture();
            texture.LoadTexture(textureLoc);
        }


        public unsafe void Bind()
        {
            //create quad
            UIVertex[] vert =
            {
                new(new(-Length, -Height, 0), new(0,0)),
                new(new(-Length,  Height, 0), new(1,0)),
                new(new(Length, Height, 0), new(0,1)),
                new(new(Length, -Height, 0), new(1,1)),
            };
            int[] ind = { 0, 1, 2, 0, 2, 3 };

            id = Globals.gl.GenVertexArray();
            Globals.gl.BindVertexArray(id);
            vbo = Globals.gl.GenBuffer();
            Globals.gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = Globals.gl.GenBuffer();
            Globals.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (UIVertex* buf = vert)
                Globals.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vert.Length * sizeof(Vector3) + sizeof(Vector2)), buf, BufferUsageARB.StreamDraw);
            fixed (int* buf = ind)
                Globals.gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(ind.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            Globals.gl.EnableVertexAttribArray(0);
            Globals.gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)(sizeof(Vector3) + sizeof(Vector2)), (void*)0);
            Globals.gl.EnableVertexAttribArray(1);
            Globals.gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)(sizeof(Vector3) + sizeof(Vector2)), (void*)(3 * sizeof(float)));
            Globals.gl.BindVertexArray(0);

        }

        public unsafe void Draw(in Shader shader)
        {
            Globals.gl.BindVertexArray(id);
            Globals.gl.ActiveTexture(GLEnum.Texture1);
            texture.Bind();
            Globals.gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            Globals.gl.BindVertexArray(0);
        }

        public void Dispose()
        {
            Globals.gl.DeleteBuffer(vbo);
            Globals.gl.DeleteBuffer(ebo);
            Globals.gl.DeleteVertexArray(id);
            GC.SuppressFinalize(this);
        }

    }

    public static class UiRenderer
    {
        static Shader uiShader;

        public static void Init()
        {
            uiShader = new Shader(Globals.gl, "Ui.vert", "Ui.frag");

        }

        public static void Draw()
        {
            UiElement test = new UiElement("", 1920 / 2f, 1080 / 2f);
            test.Bind();
            Globals.gl.UseProgram(uiShader.shader);
            test.Draw(in uiShader);
        }
    }
}