using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialEngine.Rendering
{


    /// <summary>
    /// Framebuffers are in active texture 1
    /// </summary>
    public class FrameBuffer : IDisposable
    {
        public uint id;
        public uint width;
        public uint height;
        public Texture texture;
        public ClearBufferMask mask { get; private set; }

        public FrameBuffer(uint width, uint height, ClearBufferMask mask)
        {
            this.width = width;
            this.height = height;
            this.mask = mask;

            id = Globals.gl.GenFramebuffer();
            texture = new Texture();
            texture.CreateFromFrameBuffer(this);
            Globals.gl.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            if (mask == ClearBufferMask.ColorBufferBit)
            {
                Globals.gl.DrawBuffer(DrawBufferMode.ColorAttachment0);
                Globals.gl.ReadBuffer(ReadBufferMode.ColorAttachment0);
            }
            else if (mask == ClearBufferMask.DepthBufferBit)
            {
                Globals.gl.DrawBuffer(DrawBufferMode.None);
                Globals.gl.ReadBuffer(ReadBufferMode.None);
            }
            else if (mask == ClearBufferMask.StencilBufferBit)
            {
                Globals.gl.DrawBuffer(DrawBufferMode.ColorAttachment0);
                Globals.gl.ReadBuffer(ReadBufferMode.ColorAttachment0);
            }
            Globals.gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        //mask specifies weather it is in color, depth or stencil mode
        //https://registry.khronos.org/OpenGL-Refpages/gl4/html/glClear.xhtml

        public void Update(Action draw)
        {
            Globals.gl.Viewport(0, 0, width, height);
            Globals.gl.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            Globals.gl.Clear(mask);
            draw();
            Globals.gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind()
        {
            Globals.gl.BindFramebuffer(FramebufferTarget.Framebuffer, id);
        }

        public void Unbind()
        {
            Globals.gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose()
        {
            Globals.gl.DeleteFramebuffer(id);
        }
    }
}
