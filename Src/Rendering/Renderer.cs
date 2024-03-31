using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

//engine stuff
using static SpatialEngine.Globals;

namespace SpatialEngine.Rendering
{
    public static class Renderer
    {
        //offset is the vertex offset and offsetbyte is the indices offset in bytes
        record MeshOffset(int offset, int offsetByte);

        public class RenderSet : IDisposable
        {

            public uint vao { get; protected set; }
            public uint vbo { get; protected set; }
            public uint ebo { get; protected set; }
            List<MeshOffset> meshOffsets;
            BufferObject<Matrix4x4> modelMatrixes;

            public RenderSet()
            {
                meshOffsets = new List<MeshOffset>();
            }

            public unsafe void CreateDrawSet(in List<SpatialObject> objs, int countBE, int countTO)
            {
                Matrix4x4[] models = new Matrix4x4[countTO - countBE];
                int vertexSize = 0;
                int indiceSize = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    vertexSize += objs[i].SO_mesh.vertexes.Length;
                    indiceSize += objs[i].SO_mesh.indices.Length;
                }

                Vertex[] verts = new Vertex[vertexSize];
                uint[] inds = new uint[indiceSize];
                int countV = 0;
                int countI = 0;
                int count = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    models[count] = objs[i].SO_mesh.modelMat;
                    for (int j = 0; j < objs[i].SO_mesh.vertexes.Length; j++)
                    {
                        verts[countV] = objs[i].SO_mesh.vertexes[j];
                        countV++;
                    }
                    for (int j = 0; j < objs[i].SO_mesh.indices.Length; j++)
                    {
                        inds[countI] = objs[i].SO_mesh.indices[j];
                        countI++;
                    }
                    count++;
                }

                modelMatrixes = new BufferObject<Matrix4x4>(models, 3, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);

                vao = gl.GenVertexArray();
                gl.BindVertexArray(vao);
                vbo = gl.GenBuffer();
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                ebo = gl.GenBuffer();
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

                fixed (Vertex* buf = verts)
                    gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexSize * sizeof(Vertex)), buf, BufferUsageARB.StreamDraw);
                fixed (uint* buf = inds)
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indiceSize * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

                gl.EnableVertexAttribArray(0);
                gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
                gl.EnableVertexAttribArray(1);
                gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(float)));
                gl.EnableVertexAttribArray(2);
                gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(6 * sizeof(float)));
                gl.BindVertexArray(0);
            }

            public unsafe void UpdateDrawSet(in List<SpatialObject> objs, int countBE, int countTO)
            {
                Matrix4x4[] models = new Matrix4x4[countTO - countBE];
                int vertexSize = 0;
                int indiceSize = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    //maybe move offset calculation into here?
                    vertexSize += objs[i].SO_mesh.vertexes.Length;
                    indiceSize += objs[i].SO_mesh.indices.Length;
                }

                Vertex[] verts = new Vertex[vertexSize];
                uint[] inds = new uint[indiceSize];
                int countV = 0;
                int countI = 0;
                int count = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    models[count] = objs[i].SO_mesh.modelMat;
                    for (int j = 0; j < objs[i].SO_mesh.vertexes.Length; j++)
                    {
                        verts[countV] = objs[i].SO_mesh.vertexes[j];
                        countV++;
                    }
                    for (int j = 0; j < objs[i].SO_mesh.indices.Length; j++)
                    {
                        inds[countI] = objs[i].SO_mesh.indices[j];
                        countI++;
                    }
                    count++;
                }

                modelMatrixes.Update(models);

                gl.BindVertexArray(vao);
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

                fixed (Vertex* buf = verts)
                    gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(verts.Length * sizeof(Vertex)), buf, BufferUsageARB.StreamDraw);
                fixed (uint* buf = inds)
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(inds.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

                gl.BindVertexArray(0);
            }

            public void UpdateModelBuffer(in List<SpatialObject> objs, int countBE, int countTO)
            {
                Matrix4x4[] models = new Matrix4x4[countTO - countBE];

                int count = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    models[count] = objs[i].SO_mesh.modelMat;
                    count++;
                }

                modelMatrixes.Update(models);
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
                    offset += objs[i].SO_mesh.vertexes.Length;
                    offsetByte += objs[i].SO_mesh.indices.Length;
                }
                meshOffsets.Add(new MeshOffset(offset, offsetByte * sizeof(uint)));
                return meshOffsets.Count - 1;
            }

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
                int count = 0;
                uint[] indCounts = new uint[countTO - countBE];
                int[] offsetBytes = new int[countTO - countBE];
                int[] offsets = new int[countTO - countBE];
                for (int i = countBE; i < countTO; i++)
                {
                    int index = count;
                    if (count >= meshOffsets.Count)
                        index = GetOffsetIndex(countBE, count, i, objs);
                    indCounts[count] = (uint)objs[i].SO_mesh.indices.Length;
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
                DrawCallCount++;
                gl.BindVertexArray(0);
            }

            //needs to have the shader be set as the objects shader
            public unsafe void DrawSetObject(in List<SpatialObject> objs, int countBE, int countTO)
            {
                gl.BindVertexArray(vao);
                modelMatrixes.Bind();
                int count = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    //early from the current object
                    if (objs[i].SO_shader is null)
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
                    gl.UseProgram(objs[i].SO_shader.shader);

                    gl.DrawElementsBaseVertex(GLEnum.Triangles, (uint)objs[i].SO_mesh.indices.Length, GLEnum.UnsignedInt, (void*)meshOffsets[index].offsetByte, meshOffsets[index].offset);
                    DrawCallCount++;
                    count++;
                }
                gl.BindVertexArray(0);
            }
        }

        public static int MaxRenders;
        public static List<RenderSet> renderSets;
        static int objectBeforeCount = 0;

        public static void Init(in Scene scene, int maxRenders = 10000)
        {
            renderSets = new List<RenderSet>();
            MaxRenders = maxRenders;
            renderSets.Add(new RenderSet());
            renderSets[0].CreateDrawSet(in scene.SpatialObjects, 0, scene.SpatialObjects.Count);
            //UiRenderer.Init();

            RayTracer.Init(scene, maxRenders);
        }

        public static void Draw(in Scene scene, ref Shader shader, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
        {

            RayTracer.Draw(scene, view, proj, camPos);

            /*int objTotalCount = scene.SpatialObjects.Count;

            // add a new render set if there is more objects than there is rendersets avaliable
            if (objTotalCount > MaxRenders * renderSets.Count)
            {
                renderSets.Add(new RenderSet());
                int countADD = scene.SpatialObjects.Count;
                int beCountADD = 0;
                int objCountADD = 0;
                for (int i = 0; i < renderSets.Count; i++)
                {
                    beCountADD = objCountADD;
                    objCountADD = (int)MathF.Min(MaxRenders, countADD) + (i * MaxRenders);
                    countADD -= MaxRenders;
                }
                renderSets[^1].CreateDrawSet(in scene.SpatialObjects, beCountADD, objCountADD);
            }

            // update a renderset if there is more objects but less than needed for a new renderset
            int count = objTotalCount;
            int beCount = 0;
            switch(Settings.RendererSettings.OptimizeUpdatingBuffers)
            {
                case 0:
                {
                    for (int i = 0; i < renderSets.Count; i++)
                    {
                        int objCount = (int)MathF.Min(MaxRenders, count) + (i * MaxRenders);
                        renderSets[i].UpdateDrawSet(in scene.SpatialObjects, beCount, objCount);
                        count -= MaxRenders;
                        beCount = objCount;
                    }
                    break;
                }
                case 1:
                {
                    if(GetTime() % 1 >= 0.95f || objectBeforeCount != objTotalCount)
                    {
                        for (int i = 0; i < renderSets.Count; i++)
                        {
                            int objCount = (int)MathF.Min(MaxRenders, count) + (i * MaxRenders);
                            renderSets[i].UpdateDrawSet(in scene.SpatialObjects, beCount, objCount);
                            count -= MaxRenders;
                            beCount = objCount;
                        }
                    }
                    break;
                }
                case 2:
                {
                    if (objectBeforeCount != objTotalCount)
                    {
                        for (int i = 0; i < renderSets.Count; i++)
                        {
                            int objCount = (int)MathF.Min(MaxRenders, count) + (i * MaxRenders);
                            renderSets[i].UpdateDrawSet(in scene.SpatialObjects, beCount, objCount);
                            count -= MaxRenders;
                            beCount = objCount;
                        }
                    }
                    break;
                }
            }

            // draw the rendersets
            count = objTotalCount;
            beCount = 0;
            for (int i = 0; i < renderSets.Count; i++)
            {
                int objCount = (int)MathF.Min(MaxRenders, count) + (i * MaxRenders);
                renderSets[i].UpdateModelBuffer(in scene.SpatialObjects, beCount, objCount);
                renderSets[i].DrawSet(in scene.SpatialObjects, beCount, objCount, ref shader, view, proj, camPos);
                count -= MaxRenders;
                beCount = objCount;
            }
            objectBeforeCount = objTotalCount;

            //UiRenderer.Draw();
            */
        }

    }
}
