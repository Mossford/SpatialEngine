using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

//engine stuff
using static SpatialEngine.Globals;

namespace SpatialEngine
{
    public class Renderer
    {
        record MeshOffset(int offset, int offsetByte, int index);

        public class RenderSet : IDisposable
        {
            // Max amount of objects a renderset can render

            public uint vao { get; protected set; }
            public uint vbo { get; protected set; }
            public uint ebo { get; protected set; }
            int prevMeshLocation = 0;
            List<MeshOffset> meshOffsets;
            //BufferObject<Matrix4x4> modelMatrixes;

            public RenderSet()
            {
                meshOffsets = new List<MeshOffset>();
            }

            public unsafe void CreateDrawSet(in List<SpatialObject> objs, int countBE, int countTO)
            {
                Span<Vertex> verts = new Span<Vertex>();
                Span<uint> inds = new Span<uint>();
                //Span<Matrix4x4> models = new Span<Matrix4x4>();
                int vertexSize = 0;
                int indiceSize = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    vertexSize += objs[i].SO_mesh.vertexes.Length;
                    indiceSize += objs[i].SO_mesh.indices.Length;
                }

                verts = stackalloc Vertex[vertexSize];
                inds = stackalloc uint[indiceSize];
                //models = stackalloc Matrix4x4[countTO - countBE];
                int countV = 0;
                int countI = 0;
                //int count = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    //models[count] = objs[i].SO_mesh.modelMat;
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
                    //count++;
                }

                //modelMatrixes = new BufferObject<Matrix4x4>(models, 3, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);

                vao = gl.GenVertexArray();
                gl.BindVertexArray(vao);
                vbo = gl.GenBuffer();
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                ebo = gl.GenBuffer();
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

                fixed (Vertex* buf = verts)
                    gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexSize * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
                fixed (uint* buf = inds)
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indiceSize * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

                gl.EnableVertexAttribArray(0);
                gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)0);
                gl.EnableVertexAttribArray(1);
                gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
                gl.EnableVertexAttribArray(2);
                gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
                gl.BindVertexArray(0);
            }

            public unsafe void UpdateDrawSet(in List<SpatialObject> objs, int countBE, int countTO)
            {
                Span<Vertex> verts = new Span<Vertex>();
                Span<uint> inds = new Span<uint>();
                //Span<Matrix4x4> models = new Span<Matrix4x4>();
                int vertexSize = 0;
                int indiceSize = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    vertexSize += objs[i].SO_mesh.vertexes.Length;
                    indiceSize += objs[i].SO_mesh.indices.Length;
                }

                if (verts.Length != vertexSize)
                {
                    verts = stackalloc Vertex[vertexSize];
                    inds = stackalloc uint[indiceSize];
                    //models = stackalloc Matrix4x4[countTO - countBE];
                    int countV = 0;
                    int countI = 0;
                    //int count = 0;
                    for (int i = countBE; i < countTO; i++)
                    {
                        //models[count] = objs[i].SO_mesh.modelMat;
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
                        //count++;
                    }
                }

                //modelMatrixes.Update(models);

                gl.BindVertexArray(vao);
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

                fixed (Vertex* buf = verts)
                    gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexSize * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
                fixed (uint* buf = inds)
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indiceSize * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

                gl.BindVertexArray(0);
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
                for (int i = countBE; i < index; i++)
                {
                    offset += objs[i].SO_mesh.indices.Length;
                }
                meshOffsets.Add(new MeshOffset(offset, offset * sizeof(uint), count));
                prevMeshLocation = count;
                return meshOffsets.Count - 1;
            }

            public unsafe void DrawSet(in List<SpatialObject> objs, int countBE, int countTO, ref Shader shader, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
            {
                gl.BindVertexArray(vao);
                //modelMatrixes.Bind();
                shader.setMat4("view", view);
                shader.setMat4("projection", proj);
                shader.setVec3("viewPos", camPos);
                int count = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    int index = count;
                    if (count >= meshOffsets.Count)
                        index = GetOffsetIndex(countBE, count, i, objs);
                    shader.setMat4("model", objs[i].SO_mesh.modelMat);
                    //shader.setMat4("model", bodyInterface.GetWorldTransform(objs[i].SO_rigidbody.rbID));
                    gl.DrawElementsBaseVertex(GLEnum.Triangles, (uint)objs[i].SO_mesh.indices.Length, GLEnum.UnsignedInt, (void*)meshOffsets[index].offsetByte, meshOffsets[index].offset);
                    count++;
                }
                gl.BindVertexArray(0);
            }
        }

        public int MaxRenders { get; protected set; }
        public List<RenderSet> renderSets { get; protected set; }
        int objectBeforeCount = 0;

        public Renderer(int maxRenders = 1000)
        {
            renderSets = new List<RenderSet>();
            MaxRenders = maxRenders;
        }

        public void Init(in Scene scene)
        {
            renderSets.Add(new RenderSet());
            renderSets[0].CreateDrawSet(scene.SpatialObjects, 0, scene.SpatialObjects.Count);
        }

        public void Draw(in Scene scene, ref Shader shader, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
        {
            int objTotalCount = scene.SpatialObjects.Count;

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
                renderSets[^1].CreateDrawSet(scene.SpatialObjects, beCountADD, objCountADD);
            }

            int count = objTotalCount;
            int beCount = 0;
            if (objectBeforeCount != objTotalCount)
            {
                for (int i = 0; i < renderSets.Count; i++)
                {
                    //Console.WriteLine("Updating renderset: " + i);
                    int objCount = (int)MathF.Min(MaxRenders, count) + (i * MaxRenders);
                    renderSets[i].UpdateDrawSet(scene.SpatialObjects, beCount, objCount);
                    count -= MaxRenders;
                    beCount = objCount;
                }
            }

            count = objTotalCount;
            beCount = 0;
            for (int i = 0; i < renderSets.Count; i++)
            {
                int objCount = (int)MathF.Min(MaxRenders, count) + (i * MaxRenders);
                //Console.WriteLine(beCount + " to " + objCount + " " + i);
                renderSets[i].DrawSet(scene.SpatialObjects, beCount, objCount, ref shader, view, proj, camPos);
                count -= MaxRenders;
                beCount = objCount;
            }
            objectBeforeCount = objTotalCount;
        }

    }
}
