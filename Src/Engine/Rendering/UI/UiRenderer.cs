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

    public enum UiElementType : byte
    {
        image = 0,
        text = 1,
    }

    public static class UiRenderer
    {
        static Shader uiTextShader;
        static Shader uiImageShader;
        public static List<UiElement> uiElements;
        public static List<UiButton> buttons;
        //will reuse this quad for all elements
        static UiQuad quad;

        public static void Init()
        {
            uiTextShader = new Shader(Globals.gl, "UiText.vert", "UiText.frag");
            uiImageShader = new Shader(Globals.gl, "UiImage.vert", "UiImage.frag");

            quad = new UiQuad();
            quad.Bind();

            uiElements = new List<UiElement>();
            buttons = new List<UiButton>();
        }

        public static void AddElement(Texture texture, Vector2 pos, float rotation, float scale, Vector2 dimension, UiElementType type)
        {
            uiElements.Add(new UiElement(texture, pos, rotation, scale, dimension.X, dimension.Y, type));
        }

        public static void DeleteElement(int index)
        {
            uiElements.RemoveAt(index);
        }

        public static void Update()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Update();
            }
        }

        public static void Draw()
        {
            float conv = MathF.PI / 180f;
            for (int i = 0; i < uiElements.Count; i++)
            {
                Matrix4x4 model = Matrix4x4.Identity;
                model *= Matrix4x4.CreateOrthographic(Window.size.X, Window.size.Y, -1, 1);
                model *= Matrix4x4.CreateScale(uiElements[i].width * uiElements[i].scale * Window.scaleFromBase.X, uiElements[i].height * uiElements[i].scale * Window.scaleFromBase.X, 1f);
                model *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, uiElements[i].rotation * conv);
                model *= Matrix4x4.CreateTranslation(new(uiElements[i].position.X, uiElements[i].position.Y, 0f));

                switch(uiElements[i].type)
                {
                    default:
                        quad.Draw(in uiImageShader, model, in uiElements[i].texture, uiElements[i].color);
                        break;
                    case UiElementType.image:
                        quad.Draw(in uiImageShader, model, in uiElements[i].texture, uiElements[i].color);
                        break;
                    case UiElementType.text:
                        quad.Draw(in uiTextShader, model, in uiElements[i].texture, uiElements[i].color);
                        break;
                }
            }
        }
    }
}
