using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

//engine stuff
using static SpatialEngine.Globals;

namespace SpatialEngine.Rendering
{
    //offset is the vertex offset and offsetbyte is the indices offset in bytes
    record MeshOffset(int offset, int offsetByte);

    public class RenderSet : IDisposable
    {

        public uint vao { get; protected set; }
        public uint vbo { get; protected set; }
        public uint ebo { get; protected set; }
        List<MeshOffset> meshOffsets;
        Vertex[] vertices;
        uint[] indices;
        Matrix4x4[] modelMats;
        BufferObject<Matrix4x4> modelMatrixes;

        public RenderSet()
        {
            meshOffsets = new List<MeshOffset>();
            vertices = new Vertex[0];
            indices = new uint[0];
            modelMats = new Matrix4x4[0];
        }

        public unsafe void CreateDrawSet(in List<SpatialObject> objs, int countBE, int countTO)
        {
            if (countTO - countBE != modelMats.Length)
            {
                modelMats = new Matrix4x4[countTO - countBE];
            }
            
            int vertexSize = 0;
            int indiceSize = 0;
            for (int i = countBE; i < countTO; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                vertexSize += objs[i].mesh.vertexes.Length;
                indiceSize += objs[i].mesh.indices.Length;
            }
            
            if (countTO - countBE != vertices.Length)
            {
                vertices = new Vertex[vertexSize];
            }
            if (countTO - countBE != indices.Length)
            {
                indices = new uint[indiceSize];
            }
            
            int countV = 0;
            int countI = 0;
            int count = 0;
            for (int i = countBE; i < countTO; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                modelMats[count] = objs[i].mesh.modelMat;
                for (int j = 0; j < objs[i].mesh.vertexes.Length; j++)
                {
                    vertices[countV] = objs[i].mesh.vertexes[j];
                    countV++;
                }
                for (int j = 0; j < objs[i].mesh.indices.Length; j++)
                {
                    indices[countI] = objs[i].mesh.indices[j];
                    countI++;
                }
                count++;
            }

            modelMatrixes = new BufferObject<Matrix4x4>(modelMats.ToArray(), Settings.RendererSettings.ModelMatrixBuffer, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);

            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = vertices.ToArray())
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexSize * sizeof(Vertex)), buf, BufferUsageARB.StreamDraw);
            fixed (uint* buf = indices.ToArray())
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indiceSize * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(float)));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(6 * sizeof(float)));
            gl.BindVertexArray(0);
        }

        public unsafe void UpdateDrawSet(in List<SpatialObject> objs, int countBE, int countTO)
        {
            if (countTO - countBE != modelMats.Length)
            {
                modelMats = new Matrix4x4[countTO - countBE];
            }
            
            int vertexSize = 0;
            int indiceSize = 0;
            for (int i = countBE; i < countTO; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                vertexSize += objs[i].mesh.vertexes.Length;
                indiceSize += objs[i].mesh.indices.Length;
            }
            
            if (countTO - countBE != vertices.Length)
            {
                vertices = new Vertex[vertexSize];
            }
            if (countTO - countBE != indices.Length)
            {
                indices = new uint[indiceSize];
            }
            
            int countV = 0;
            int countI = 0;
            int count = 0;
            for (int i = countBE; i < countTO; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                modelMats[count] = objs[i].mesh.modelMat;
                for (int j = 0; j < objs[i].mesh.vertexes.Length; j++)
                {
                    vertices[countV] = objs[i].mesh.vertexes[j];
                    countV++;
                }
                for (int j = 0; j < objs[i].mesh.indices.Length; j++)
                {
                    indices[countI] = objs[i].mesh.indices[j];
                    countI++;
                }
                count++;
            }

            modelMatrixes.Realloc(modelMats.ToArray());

            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = vertices.ToArray())
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(Vertex)), buf, BufferUsageARB.StreamDraw);
            fixed (uint* buf = indices.ToArray())
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            gl.BindVertexArray(0);
        }

        public void UpdateModelBuffer(in List<SpatialObject> objs, int countBE, int countTO)
        {
            if (countTO - countBE != modelMats.Length)
            {
                modelMats = new Matrix4x4[countTO - countBE];
            }

            int count = 0;
            for (int i = countBE; i < countTO; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                modelMats[count] = objs[i].mesh.modelMat;
                count++;
            }
            
            modelMatrixes.Realloc(modelMats.ToArray());
        }

        public void Dispose()
        {
            gl.DeleteVertexArray(vao);
            gl.DeleteBuffer(vbo);
            gl.DeleteBuffer(ebo);
            GC.SuppressFinalize(this);
        }

        int GetOffsetIndex(int countBE, int count, int index, in List<SpatialObject> objs)
        {
            int offset = 0;
            int offsetByte = 0;
            for (int i = countBE; i < index; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                offset += objs[i].mesh.vertexes.Length;
                offsetByte += objs[i].mesh.indices.Length;
            }
            meshOffsets.Add(new MeshOffset(offset, offsetByte * sizeof(uint)));
            return meshOffsets.Count - 1;
        }

        /// <summary>
        /// Draws the RenderSet using MultiDraw, warning that on Mesa drivers gl_DrawID will have some issues
        /// </summary>
        /// <param name="objs">The objects to draw</param>
        /// <param name="countBE">The index to start from</param>
        /// <param name="countTO">The index to end to</param>
        /// <param name="shader">The shader to use</param>
        /// <param name="view">View matrix</param>
        /// <param name="proj">Projection matrix</param>
        /// <param name="camPos">Camera position</param>
        public unsafe void DrawSet(in List<SpatialObject> objs, int countBE, int countTO, ref Shader shader, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
        {
            //return if scene is empty as will crash because obarray and others are empty since countBE - countTO is 0
            if (objs.Count == 0)
                return;

            gl.BindVertexArray(vao);
            modelMatrixes.Bind();
            shader.setMat4("view", view);
            shader.setMat4("projection", proj);
            shader.setVec3("viewPos", camPos);
            shader.setBool("meshDraw", false);
            int count = 0;
            uint[] indCounts = new uint[countTO - countBE];
            int[] offsetBytes = new int[countTO - countBE];
            int[] offsets = new int[countTO - countBE];
            for (int i = countBE; i < countTO; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                int index = count;
                if (count >= meshOffsets.Count)
                    index = GetOffsetIndex(countBE, count, i, objs);
                indCounts[count] = (uint)objs[i].mesh.indices.Length;
                offsetBytes[count] = meshOffsets[index].offsetByte;
                offsets[count] = meshOffsets[index].offset;
                count++;
            }

            //indices paramater needed a array of void* and this allows for it as it creates pointers to each value and creates a pointer array with it
            int*[] obArray = new int*[countTO - countBE];

            for (int i = 0; i < offsetBytes.Length; i++)
            {
                obArray[i] = (int*)offsetBytes[i];
            }

            fixed (void* ptr = &obArray[0])
                gl.MultiDrawElementsBaseVertex(GLEnum.Triangles, indCounts, GLEnum.UnsignedInt, (void**)ptr, offsets);
            drawCallCount++;
            gl.BindVertexArray(0);
        }

        /// <summary>
        /// Draws the RenderSet, but uses the shader from the SO_Object
        /// </summary>
        /// <param name="objs">The objects to draw</param>
        /// <param name="countBE">The index to start from</param>
        /// <param name="countTO">The index to end to</param>
        public unsafe void DrawSetObject(in List<SpatialObject> objs, int countBE, int countTO)
        {
            gl.BindVertexArray(vao);
            modelMatrixes.Bind();
            int count = 0;
            for (int i = countBE; i < countTO; i++)
            {
                //early from the current object
                if (objs[i].shader is null)
                    continue;
                
                if(!objs[i].enabled)
                    continue;

                int index = count;
                if (count >= meshOffsets.Count)
                    index = GetOffsetIndex(countBE, count, i, objs);
                //Because of opengls stupid documentation this draw call is suppose to take in the offset in indices by bytes then take in the offset in vertices instead of the offset in indices
                // and its not the indices that are stored it wants the offsets as the indcies are already in a buffer which is what draw elements is using
                //
                //    indices
                //        Specifies a pointer to the location where the indices are stored.
                //    basevertex
                //        Specifies a constant that should be added to each element of indices when chosing elements from the enabled vertex arrays. 
                //
                //This naming is so fucking bad and has caused me multiple hours in trying to find what the hell the problem is

                //use the object shader
                gl.UseProgram(objs[i].shader.shader);
                if(objs[i].texture != null)
                    objs[i].texture.Bind();
                else
                    Renderer.defaultTexture.Bind();
                
                gl.DrawElementsBaseVertex(GLEnum.Triangles, (uint)objs[i].mesh.indices.Length, GLEnum.UnsignedInt, (void*)meshOffsets[index].offsetByte, meshOffsets[index].offset);
                drawCallCount++;
                count++;
            }
            gl.BindVertexArray(0);
        }
        
        /// <summary>
        /// Draws the RenderSet, using a normal draw call for each object
        /// slower, but should be fine on any driver
        /// </summary>
        /// <param name="objs">The objects to draw</param>
        /// <param name="countBE">The index to start from</param>
        /// <param name="countTO">The index to end to</param>
        /// <param name="shader">The shader to use</param>
        /// <param name="view">View matrix</param>
        /// <param name="proj">Projection matrix</param>
        /// <param name="camPos">Camera position</param>
        public unsafe void DrawSetObject(in List<SpatialObject> objs, int countBE, int countTO, ref Shader shader, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
        {
            gl.BindVertexArray(vao);
            modelMatrixes.Bind();
            int count = 0;
            for (int i = countBE; i < countTO; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                int index = count;
                if (count >= meshOffsets.Count)
                    index = GetOffsetIndex(countBE, count, i, objs);
                //Because of opengls stupid documentation this draw call is suppose to take in the offset in indices by bytes then take in the offset in vertices instead of the offset in indices
                // and its not the indices that are stored it wants the offsets as the indcies are already in a buffer which is what draw elements is using
                //
                //    indices
                //        Specifies a pointer to the location where the indices are stored.
                //    basevertex
                //        Specifies a constant that should be added to each element of indices when chosing elements from the enabled vertex arrays. 
                //
                //This naming is so fucking bad and has caused me multiple hours in trying to find what the hell the problem is

                //use the object shader
                shader.setMat4("view", view);
                shader.setMat4("projection", proj);
                shader.setVec3("viewPos", camPos);
                shader.setBool("meshDraw", true);
                shader.setInt("modelIndex", count);
                if(objs[i].texture != null)
                    objs[i].texture.Bind();
                else
                    Renderer.defaultTexture.Bind();

                gl.DrawElementsBaseVertex(GLEnum.Triangles, (uint)objs[i].mesh.indices.Length, GLEnum.UnsignedInt, (void*)meshOffsets[index].offsetByte, meshOffsets[index].offset);
                drawCallCount++;
                count++;
            }
            gl.BindVertexArray(0);
        }

        /// <summary>
        /// Draws but does not bind any shader inside the method
        /// </summary>
        /// <param name="objs">The objects to draw</param>
        /// <param name="countBE">The index to start from</param>
        /// <param name="countTO">The index to end to</param>
        public unsafe void DrawSetNoAssign(in List<SpatialObject> objs, int countBE, int countTO)
        {
            //return if scene is empty as will crash because obarray and others are empty since countBE - countTO is 0
            if (objs.Count == 0)
                return;

            gl.BindVertexArray(vao);
            modelMatrixes.Bind();
            int count = 0;
            uint[] indCounts = new uint[countTO - countBE];
            int[] offsetBytes = new int[countTO - countBE];
            int[] offsets = new int[countTO - countBE];
            for (int i = countBE; i < countTO; i++)
            {
                if(!objs[i].enabled)
                    continue;
                
                int index = count;
                if (count >= meshOffsets.Count)
                    index = GetOffsetIndex(countBE, count, i, objs);
                indCounts[count] = (uint)objs[i].mesh.indices.Length;
                offsetBytes[count] = meshOffsets[index].offsetByte;
                offsets[count] = meshOffsets[index].offset;
                count++;
            }

            //indices paramater needed a array of void* and this allows for it as it creates pointers to each value and creates a pointer array with it
            int*[] obArray = new int*[countTO - countBE];

            for (int i = 0; i < offsetBytes.Length; i++)
            {
                obArray[i] = (int*)offsetBytes[i];
            }

            fixed (void* ptr = &obArray[0])
                gl.MultiDrawElementsBaseVertex(GLEnum.Triangles, indCounts, GLEnum.UnsignedInt, (void**)ptr, offsets);
            drawCallCount++;
            gl.BindVertexArray(0);
        }
    }
}
