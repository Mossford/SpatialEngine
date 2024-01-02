using System;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//engine stuff
using static SpatialEngine.Globals;
using System.Reflection;

namespace SpatialEngine.Rendering
{
    public unsafe class BufferObject<TDataType> : IDisposable where TDataType : unmanaged
    {
        public uint id { get; protected set; }
        public BufferTargetARB bufType;
        public BufferUsageARB bufAccess;
        public uint index;

        public BufferObject(Span<TDataType> data, uint index, BufferTargetARB bufferType, BufferUsageARB bufferAccess)
        {
            this.bufType = bufferType;
            this.bufAccess = bufferAccess;
            this.index = index;
            id = gl.GenBuffer();
            Bind();
            fixed (void* buf = data)
                gl.BufferData(bufType, (nuint)(data.Length * sizeof(TDataType)), buf, bufAccess);
        }

        public void Update(Span<TDataType> data)
        {
            Bind();
            fixed (void* buf = data)
                gl.BufferData(bufType, (nuint)(data.Length * sizeof(TDataType)), buf, bufAccess);
        }

        public void Bind()
        {
            gl.BindBufferBase(bufType, index, id);
        }

        public void Dispose() 
        {
            gl.DeleteBuffer(id);
        }
    }
}
