using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

//engine stuff
using static SpatialEngine.Globals;

namespace SpatialEngine
{
    public class Renderer
    {
        record MeshOffset(int offset, int offsetByte, int index);

        public class RenderSet
        {
            public int MaxRenders { get; protected set; }

            public uint vao { get; protected set; }
            public uint vbo { get; protected set; }
            public uint ebo { get; protected set; }
            int prevMeshLocation = 0;
            List<MeshOffset> meshOffsets;
            Vertex[] vertexes;
            uint[] indices;

            public RenderSet(int maxRenders = 10000)
            {
                MaxRenders = maxRenders;
                meshOffsets = new List<MeshOffset>();
            }

            public unsafe void CreateDrawSet(in List<SpatialObject> objs)
            {
                vertexes = new Vertex[0];
                indices = new uint[0];
                int vertexSize = 0;
                int indiceSize = 0;
                for (int i = 0; i < objs.Count; i++)
                {
                    vertexSize += objs[i].SO_mesh.vertexes.Length;
                    indiceSize += objs[i].SO_mesh.indices.Length;
                }

                if (vertexes.Length != vertexSize)
                {
                    vertexes = new Vertex[vertexSize];
                    indices = new uint[indiceSize];
                    for (int i = 0; i < objs.Count; i++)
                    {
                        for (int j = 0; j < objs[i].SO_mesh.vertexes.Length; j++)
                        {
                            vertexes[j + objs[i].SO_mesh.vertexes.Length * i] = objs[i].SO_mesh.vertexes[j];
                        }
                        for (int j = 0; j < objs[i].SO_mesh.indices.Length; j++)
                        {
                            indices[j + objs[i].SO_mesh.indices.Length * i] = objs[i].SO_mesh.indices[j];
                        }
                    }
                }

                vao = gl.GenVertexArray();
                gl.BindVertexArray(vao);
                vbo = gl.GenBuffer();
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                ebo = gl.GenBuffer();
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

                fixed (Vertex* buf = vertexes)
                    gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexes.Length * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
                fixed (uint* buf = indices)
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

                gl.EnableVertexAttribArray(0);
                gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)0);
                gl.EnableVertexAttribArray(1);
                gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
                gl.EnableVertexAttribArray(2);
                gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
                gl.BindVertexArray(0);
            }

            public unsafe void UpdateDrawSet(in List<SpatialObject> objs)
            {
                int vertexSize = 0;
                int indiceSize = 0;
                for (int i = 0; i < objs.Count; i++)
                {
                    vertexSize += objs[i].SO_mesh.vertexes.Length;
                    indiceSize += objs[i].SO_mesh.indices.Length;
                }

                if (vertexes.Length != vertexSize)
                {
                    vertexes = new Vertex[vertexSize];
                    indices = new uint[indiceSize];
                    for (int i = 0; i < objs.Count; i++)
                    {
                        for (int j = 0; j < objs[i].SO_mesh.vertexes.Length; j++)
                        {
                            vertexes[j + objs[i].SO_mesh.vertexes.Length * i] = objs[i].SO_mesh.vertexes[j];
                        }
                        for (int j = 0; j < objs[i].SO_mesh.indices.Length; j++)
                        {
                            indices[j + objs[i].SO_mesh.indices.Length * i] = objs[i].SO_mesh.indices[j];
                        }
                    }
                }

                gl.BindVertexArray(vao);
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

                fixed (Vertex* buf = vertexes)
                    gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexes.Length * sizeof(Vertex)), buf, BufferUsageARB.StaticDraw);
                fixed (uint* buf = indices)
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

                gl.BindVertexArray(0);
            }

            int GetOffsetIndex(int index, in List<SpatialObject> objs)
            {
                int offsetByte = 0;
                int offset = 0;
                for (int g = 0; g < index; g++)
                {
                    offset += objs[g].SO_mesh.indices.Length;
                    offsetByte += objs[g].SO_mesh.indices.Length * sizeof(uint);
                }
                meshOffsets.Add(new MeshOffset(offset, offsetByte, index));
                prevMeshLocation = index;
                return meshOffsets.Count - 1;
            }

            public unsafe void DrawSet(in List<SpatialObject> objs, ref Shader shader, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
            {
                gl.BindVertexArray(vao);
                shader.setMat4("view", view);
                shader.setMat4("projection", proj);
                shader.setVec3("viewPos", camPos);
                for (int i = 0; i < objs.Count; i++)
                {
                    int index = i;
                    if (i >= meshOffsets.Count)
                        index = GetOffsetIndex(i, objs);
                    shader.setMat4("model", objs[i].SO_mesh.modelMat);
                    gl.DrawElementsBaseVertex(GLEnum.Triangles, (uint)objs[i].SO_mesh.indices.Length, GLEnum.UnsignedInt, (void*)meshOffsets[index].offsetByte, meshOffsets[index].offset);
                }
                gl.BindVertexArray(0);
            }
        }

        List<RenderSet> renderSets;

        public Renderer()
        {
            renderSets = new List<RenderSet>();
        }

        public void Init(in Scene scene)
        {
            renderSets.Add(new RenderSet());
            renderSets[0].CreateDrawSet(scene.SpatialObjects);
        }

        public void Draw(in Scene scene, ref Shader shader, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
        {
            renderSets[0].UpdateDrawSet(scene.SpatialObjects);
            renderSets[0].DrawSet(scene.SpatialObjects, ref shader, view, proj, camPos);
        }

    }
}
