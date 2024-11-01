using Silk.NET.OpenGL;
using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Input;


namespace SpatialEngine.Rendering
{
    public class UiElement : IDisposable
    {
        //size for the quad
        public float width;
        public float height;

        //transform
        public Vector2 position;
        public float rotation;
        public float scale;
        public Vector3 color;

        //texture that is displayed
        public Texture texture;

        public UiElementType type;

        public UiElement(string textureLoc, Vector2 pos, float rot = 0f, float scale = 1f, float length = 100, float height = 100, UiElementType type = UiElementType.image)
        {
            texture = new Texture();
            texture.LoadTexture(textureLoc);
            this.position = pos;
            this.rotation = rot;
            this.scale = scale;
            this.width = length;
            this.height = height;
            this.type = type;
            color = Vector3.One;
        }

        public UiElement(Texture texture, Vector2 pos, float rot = 0f, float scale = 1f, float length = 100, float height = 100, UiElementType type = UiElementType.image)
        {
            this.texture = texture;
            this.position = pos;
            this.rotation = rot;
            this.scale = scale;
            this.width = length;
            this.height = height;
            this.type = type;
            color = Vector3.One;
        }

        public void Dispose()
        {
            texture.Dispose();
            GC.SuppressFinalize(this);
        }

    }
}
