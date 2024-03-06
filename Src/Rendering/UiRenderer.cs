using Silk.NET.OpenGL;
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
        //size for the quad
        public float length;
        public float height;

        //transform
        public Vector2 position;
        public float rotation;
        public float scale;

        //texture that is displayed
        public Texture texture;

        public UiElement(string textureLoc, Vector2 pos, float rot = 0f, float scale = 1f, float length = 100, float height = 100)
        {
            texture = new Texture();
            texture.LoadTexture(textureLoc);
            this.position = pos;
            this.rotation = rot;
            this.scale = scale;
            this.length = length;
            this.height = height;
        }

        public void Dispose()
        {
            texture.Dispose();
            GC.SuppressFinalize(this);
        }

    }

    public static class UiRenderer
    {
        //just have one quad to use
        public static class UiQuad
        {
            struct UIVertex
            {
                public Vector2 pos;
                public Vector2 uv;

                public UIVertex(Vector2 pos, Vector2 uv)
                {
                    this.pos = pos;
                    this.uv = uv;
                }
            }

            static uint id;
            static uint vbo;
            static uint ebo;
            public static unsafe void Bind()
            {
                //create quad
                UIVertex[] vert =
                {
                new(new(-1, -1), new(0,0)),
                new(new(1, -1), new(1,0)),
                new(new(-1, 1), new(0,1)),
                new(new(1, 1), new(1,1))
                };
                int[] ind = { 0, 1, 2, 1, 3, 2 };

                id = Globals.gl.GenVertexArray();
                Globals.gl.BindVertexArray(id);
                vbo = Globals.gl.GenBuffer();
                Globals.gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                ebo = Globals.gl.GenBuffer();
                Globals.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

                fixed (UIVertex* buf = vert)
                    Globals.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vert.Length * sizeof(UIVertex)), buf, BufferUsageARB.StreamDraw);
                fixed (int* buf = ind)
                    Globals.gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(ind.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

                Globals.gl.EnableVertexAttribArray(0);
                Globals.gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(UIVertex), (void*)0);
                Globals.gl.EnableVertexAttribArray(1);
                Globals.gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(UIVertex), (void*)(2 * sizeof(float)));
                Globals.gl.BindVertexArray(0);

            }

            public static unsafe void Draw(in Shader shader, in Texture texture)
            {
                Globals.gl.BindVertexArray(id);
                Globals.gl.ActiveTexture(GLEnum.Texture0);
                texture.Bind();
                Globals.gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
                Globals.gl.BindVertexArray(0);
                Globals.DrawCallCount++;
            }

            public static void Dispose()
            {
                Globals.gl.DeleteBuffer(vbo);
                Globals.gl.DeleteBuffer(ebo);
                Globals.gl.DeleteVertexArray(id);
            }
        }

        static Shader uiShader;
        static List<UiElement> uiElements;

        public static void Init()
        {
            uiShader = new Shader(Globals.gl, "Ui.vert", "Ui.frag");

            UiQuad.Bind();

            uiElements = new List<UiElement>();
            uiElements.Add(new UiElement("RedDebug.png", new(0,0), 0f, 1.0f));
        }

        public static void Draw()
        {
            Globals.gl.UseProgram(uiShader.shader);
            for (int i = 0; i < uiElements.Count; i++)
            {
                Matrix4x4 model = Matrix4x4.Identity;
                model *= Matrix4x4.CreateScale(uiElements[i].length * uiElements[i].scale, uiElements[i].height * uiElements[i].scale, 1f);
                model *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, uiElements[i].rotation);
                model *= Matrix4x4.CreateTranslation(new(uiElements[i].position.X, uiElements[i].position.Y, 0f));
                model *= Matrix4x4.CreateOrthographic(Globals.SCR_WIDTH, Globals.SCR_HEIGHT, -1, 1);

                uiShader.setMat4("model", model);
                UiQuad.Draw(in uiShader, in uiElements[i].texture);
            }
        }
    }
}
