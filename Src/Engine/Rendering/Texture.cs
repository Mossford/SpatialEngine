using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using System.Linq;
using StbImageSharp;

using static SpatialEngine.Globals;
using System.IO;
using System;
using System.Drawing;
using System.Numerics;
using Silk.NET.Vulkan;

namespace SpatialEngine.Rendering
{

    public static class MissingTexture
    {
        public static byte[,,] pixels { get; private set; }
        public static int size = 128;
        static bool created = false;

        public static void Create()
        {
            if(created)
            {
                return;
            }
            pixels = new byte[size, size, 3];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
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
            created = true;
        }
    }


    /// <summary>
    /// Image textures are in activetexture 0
    /// </summary>
    public class Texture : IDisposable
    {
        public uint id;
        public string textLocation;
        public Vector2 size;
        public InternalFormat internalFormat;
        public GLEnum format;
        
        public unsafe void CreateTextureImage2D(int width, int height, InternalFormat internalFormat, GLEnum format, uint location)
        {
            this.size = new Vector2(width, height);
            this.internalFormat = internalFormat;
            this.format = format;

            id = gl.GenTexture();
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, id);
            gl.TextureParameter(id, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
            gl.TextureParameter(id, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
            gl.TextureParameter(id, GLEnum.TextureWrapS, (int)GLEnum.MirroredRepeat);
            gl.TextureParameter(id, GLEnum.TextureWrapT, (int)GLEnum.MirroredRepeat);
            gl.TexImage2D(GLEnum.Texture2D, 0, internalFormat, (uint)width, (uint)height, 0, format, GLEnum.Float, null);
            gl.BindImageTexture(location, id, 0, false, 0, GLEnum.ReadOnly, internalFormat);
        }
        
        public unsafe void CreateTextureImage3D(int width, int height, InternalFormat internalFormat, GLEnum format, uint location, int border)
        {
            this.size = new Vector2(width, height);
            this.internalFormat = internalFormat;
            this.format = format;

            id = gl.GenTexture();
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2DArray, id);
            gl.TexImage2D(GLEnum.Texture2DArray, 0, internalFormat, (uint)width, (uint)height, border, format, GLEnum.Float, null);
            gl.TexParameterI(GLEnum.Texture2DArray, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
            gl.TexParameterI(GLEnum.Texture2DArray, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
            gl.TexParameterI(GLEnum.Texture2DArray, GLEnum.TextureWrapS, (int)GLEnum.ClampToBorder);
            gl.TexParameterI(GLEnum.Texture2DArray, GLEnum.TextureWrapT, (int)GLEnum.ClampToBorder);
            gl.BindImageTexture(location, id, 0, false, 0, GLEnum.ReadOnly, internalFormat);
        }

        public unsafe void LoadTexture(string location)
        {
            StbImage.stbi_set_flip_vertically_on_load(1);
            bool noImage = false;
            ImageResult result = new ImageResult();

            id = gl.GenTexture();
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, id);

            if (!File.Exists(Resources.ImagePath + location))
            {
                noImage = true;
                MissingTexture.Create();
                gl.TextureParameter(id, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
                gl.TextureParameter(id, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
                gl.TextureParameter(id, GLEnum.TextureWrapS, (int)GLEnum.MirroredRepeat);
                gl.TextureParameter(id, GLEnum.TextureWrapT, (int)GLEnum.MirroredRepeat);
            }
            else
            {
                result = ImageResult.FromMemory(File.ReadAllBytes(Resources.ImagePath + location), ColorComponents.RedGreenBlueAlpha);
                gl.TextureParameter(id, GLEnum.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
                gl.TextureParameter(id, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
                gl.TextureParameter(id, GLEnum.TextureWrapS, (int)GLEnum.MirroredRepeat);
                gl.TextureParameter(id, GLEnum.TextureWrapT, (int)GLEnum.MirroredRepeat);
            }

            if (noImage)
            {
                fixed (byte* data = MissingTexture.pixels)
                {
                    gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgb, (uint)MissingTexture.size, (uint)MissingTexture.size, 0, GLEnum.Rgb, PixelType.UnsignedByte, data);
                    internalFormat = InternalFormat.Rgb;
                    format = GLEnum.Rgb;
                    size = new Vector2(MissingTexture.size, MissingTexture.size);
                }
            }
            else
            {
                fixed (byte* data = result.Data)
                {
                    gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width, (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                    internalFormat = InternalFormat.Rgba;
                    format = GLEnum.Rgba;
                    size = new Vector2(result.Width, result.Height);
                }
            }
            gl.GenerateMipmap(GLEnum.Texture2D);
            gl.BindTexture(GLEnum.Texture2D, 0);
            textLocation = location;
        }

        public unsafe void LoadTexture(in byte[] pixels, int width, int height, InternalFormat internalFormat,  GLEnum format)
        {
            id = gl.GenTexture();
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, id);

            gl.TextureParameter(id, GLEnum.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
            gl.TextureParameter(id, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
            gl.TextureParameter(id, GLEnum.TextureWrapS, (int)GLEnum.MirroredRepeat);
            gl.TextureParameter(id, GLEnum.TextureWrapT, (int)GLEnum.MirroredRepeat);

            fixed (byte* data = pixels)
            {
                gl.TexImage2D(GLEnum.Texture2D, 0, internalFormat, (uint)width, (uint)height, 0, format, PixelType.UnsignedByte, data);
                this.internalFormat = internalFormat;
                this.format = format;
            }
            size = new Vector2(width, height);
            gl.GenerateMipmap(GLEnum.Texture2D);
            gl.BindTexture(GLEnum.Texture2D, 0);
        }

        public unsafe void UpdateTexture(in byte[] pixels, int width, int height)
        {
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, id);

            fixed (byte* data = pixels)
            {
                gl.TexImage2D(GLEnum.Texture2D, 0, internalFormat, (uint)width, (uint)height, 0, format, PixelType.UnsignedByte, data);
            }
            gl.GenerateMipmap(GLEnum.Texture2D);
            gl.BindTexture(GLEnum.Texture2D, 0);
        }

        public unsafe void CreateFromFrameBuffer(in FrameBuffer buf)
        {
            size = new Vector2(buf.width, buf.height);
            id = gl.GenTexture();
            gl.BindTexture(GLEnum.Texture2D, id);
            if(buf.mask == ClearBufferMask.ColorBufferBit)
                gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgb, buf.width, buf.height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (void*)null);
            else if(buf.mask == ClearBufferMask.DepthBufferBit)
                gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.DepthComponent, buf.width, buf.height, 0, PixelFormat.DepthComponent, PixelType.Float, (void*)null);
            else if(buf.mask == ClearBufferMask.StencilBufferBit)
                gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgb, buf.width, buf.height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (void*)null);
            gl.TextureParameter(id, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
            gl.TextureParameter(id, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
            gl.TextureParameter(id, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
            gl.TextureParameter(id, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
            buf.Bind();
            if (buf.mask == ClearBufferMask.ColorBufferBit)
                gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, GLEnum.Texture2D, id, 0);
            else if (buf.mask == ClearBufferMask.DepthBufferBit)
                gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, GLEnum.Texture2D, id, 0);
            else if (buf.mask == ClearBufferMask.StencilBufferBit)
                gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, GLEnum.Texture2D, id, 0);
            buf.Unbind();
        }
        
        public void Bind()
        {
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, id);
        }

        public void Dispose()
        {
            gl.DeleteTexture(id);
            GC.SuppressFinalize(this);
        }
    }
}