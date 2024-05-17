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

        public UiElement(Texture texture, Vector2 pos, float rot = 0f, float scale = 1f, float length = 100, float height = 100)
        {
            this.texture = texture;
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

    public class UiQuad : IDisposable
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

        uint id;
        uint vbo;
        uint ebo;
        public unsafe void Bind()
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

        //custom shader usage
        public unsafe void Draw()
        {
            Globals.gl.BindVertexArray(id);
            Globals.gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            Globals.gl.BindVertexArray(0);
            Globals.DrawCallCount++;
        }

        //ui renderer usage
        public unsafe void Draw(in Shader shader, in Matrix4x4 mat, in Texture texture)
        {
            Globals.gl.BindVertexArray(id);
            texture.Bind();
            Globals.gl.UseProgram(shader.shader);
            shader.setMat4("model", mat);
            Globals.gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            Globals.gl.BindVertexArray(0);
            Globals.DrawCallCount++;
        }

        public void Dispose()
        {
            Globals.gl.DeleteBuffer(vbo);
            Globals.gl.DeleteBuffer(ebo);
            Globals.gl.DeleteVertexArray(id);
        }
    }

    public static class UiRenderer
    {
        static Shader uiShader;
        static List<UiElement> uiElements;
        //will reuse this quad for all elements
        static UiQuad quad;

        public static void Init()
        {
            uiShader = new Shader(Globals.gl, "Ui.vert", "Ui.frag");

            quad = new UiQuad();
            quad.Bind();

            uiElements = new List<UiElement>();
            //uiElements.Add(new UiElement("RedDebug.png", new(0,0), 0f, 1.0f));
        }

        public static void AddElement(Texture texture, Vector2 pos, float rotation, float scale, Vector2 dimension)
        {
            uiElements.Add(new UiElement(texture, pos, rotation, scale, dimension.X, dimension.Y));
        }

        public static void Draw()
        {
            for (int i = 0; i < uiElements.Count; i++)
            {
                Matrix4x4 model = Matrix4x4.Identity;
                model *= Matrix4x4.CreateScale(uiElements[i].length * uiElements[i].scale, uiElements[i].height * uiElements[i].scale, 1f);
                model *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, uiElements[i].rotation);
                model *= Matrix4x4.CreateTranslation(new(uiElements[i].position.X, uiElements[i].position.Y, 0f));
                model *= Matrix4x4.CreateOrthographic(Globals.SCR_WIDTH, Globals.SCR_HEIGHT, -1, 1);

                quad.Draw(in uiShader, model, in uiElements[i].texture);
            }
        }
    }
}
