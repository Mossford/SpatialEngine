using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using System.Linq;
using StbImageSharp;

using static SpatialEngine.Globals;
using System.IO;
using System;

namespace SpatialEngine.Rendering
{
    public class Texture : IDisposable
    {
        public uint id;
        public string textLocation;

        public unsafe void LoadTexture(string location)
        {
            StbImage.stbi_set_flip_vertically_on_load(1);
            byte[,,] pixels = new byte[64,64,3];
            bool noImage = false;
            ImageResult result = new ImageResult();

            if (!File.Exists(location))
            {
                noImage = true;
                for (int x = 0; x < 64; x++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if ((x / 4 + y / 4) % 2 == 0)
                        {
                            pixels[x, y, 0] = 255;
                            pixels[x, y, 1] = 0;
                            pixels[x, y, 2] = 255;

                        }
                        else
                        {
                            pixels[x, y, 0] = 20;
                            pixels[x, y, 1] = 20;
                            pixels[x, y, 2] = 20;
                        }
                    }
                }
            }
            else
            {
                result = ImageResult.FromMemory(File.ReadAllBytes(location), ColorComponents.RedGreenBlueAlpha);
            }
            id = gl.GenTexture();
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, id);
            gl.TextureParameter(id, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            gl.TextureParameter(id, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
            gl.TextureParameter(id, GLEnum.TextureWrapS, (int)GLEnum.MirroredRepeat);
            gl.TextureParameter(id, GLEnum.TextureWrapT, (int)GLEnum.MirroredRepeat);

            if (noImage)
            {
                fixed (byte* data = pixels)
                {
                    gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgb, 64, 64, 0, GLEnum.Rgb, PixelType.UnsignedByte, data);
                }
            }
            else
            {
                fixed (byte* data = result.Data)
                {
                    gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width, (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                }
            }
            gl.GenerateMipmap(GLEnum.Texture2D);
            gl.BindTexture(GLEnum.Texture2D, 0);
            textLocation = location;
        }

        public void Bind()
        {
            gl.BindTexture(GLEnum.Texture2D, id);
        }

        public void Dispose()
        {
            gl.DeleteTexture(id);
            GC.SuppressFinalize(this);
        }
    }
}