using Silk.NET.OpenGL;
using Silk.NET.SDL;
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
using static SpatialEngine.Rendering.Renderer;

namespace SpatialEngine.Rendering
{
    public static class RayTracer
    {
        //create vao and ebo that is just a quad

        //still use rendersets, model matrix buffer
        //for each object upload start index, model matrix index and any other index and do a draw call of the quad

        //frag shader just needs to do a intersect test
        //go through the whole amount of meshes and check if triangle intersect
        //if intersect for now set to red

        record MeshOffset(int offset, int offsetByte);

        public class RayTraceRenderSet : IDisposable
        {
            List<MeshOffset> meshOffsets;
            BufferObject<Matrix4x4> modelMatrixes;
            BufferObject<Vertex> vertexes;
            BufferObject<uint> indices;

            public RayTraceRenderSet()
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
                vertexes = new BufferObject<Vertex>(verts, 4, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);
                indices = new BufferObject<uint>(inds, 5, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);
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
                vertexes.Update(verts);
                indices.Update(inds);
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
                modelMatrixes.Dispose();
                vertexes.Dispose();
                indices.Dispose();
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
                meshOffsets.Add(new MeshOffset(offset, offsetByte));
                return meshOffsets.Count - 1;
            }

            public unsafe void DrawSetObject(in List<SpatialObject> objs, ref Shader shader, int countBE, int countTO, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
            {
                modelMatrixes.Bind();
                vertexes.Bind();
                indices.Bind();
                int count = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    int index = count;
                    if (count >= meshOffsets.Count)
                        index = GetOffsetIndex(countBE, count, i, objs);

                    gl.UseProgram(shader.shader);
                    shader.setMat4("uView", view);
                    shader.setMat4("uProj", proj);
                    shader.setVec3("ucamPos", camPos);
                    shader.setVec3("ucamDir", player.camera.GetCamDir());
                    shader.setInt("uindex", count);
                    shader.setInt("uindOffset", meshOffsets[index].offsetByte);
                    shader.setInt("uindEnd", (meshOffsets[index].offsetByte / sizeof(uint)) + objs[i].SO_mesh.indices.Length);
                    //Console.WriteLine(objs[0].SO_mesh.indices[0]);
                    //Console.WriteLine(objs[0].SO_mesh.indices[1]);
                    //Console.WriteLine(objs[0].SO_mesh.indices[2]);
                    //Console.WriteLine(objs[0].SO_mesh.indices[3]);
                    //Console.WriteLine(objs[0].SO_mesh.indices[4]);
                    //Console.WriteLine(objs[0].SO_mesh.indices[5]);
                    quad.Draw();
                    //gl.UseProgram(shader.shader);
                    //gl.DrawElementsBaseVertex(GLEnum.Triangles, (uint)objs[i].SO_mesh.indices.Length, GLEnum.UnsignedInt, (void*)meshOffsets[index].offsetByte, meshOffsets[index].offset);
                    drawCallCount++;
                    count++;
                }
                gl.BindVertexArray(0);
            }
        }





        public static int MaxRenders;
        public static List<RayTraceRenderSet> renderSets;
        static int objectBeforeCount = 0;

        //quad for the fragment shader
        static UiQuad quad;
        static Shader shader;

        public static void Init(in Scene scene, int maxRenders = 10000)
        {
            renderSets = new List<RayTraceRenderSet>();
            MaxRenders = maxRenders;
            renderSets.Add(new RayTraceRenderSet());
            renderSets[0].CreateDrawSet(in scene.SpatialObjects, 0, scene.SpatialObjects.Count);
            //UiRenderer.Init();


            //create the quad for the fragment shader
            quad = new UiQuad();
            quad.Bind();

            shader = new Shader(gl, "Raytrace.vert", "Raytrace.frag");
        }

        public static void Draw(in Scene scene, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
        {
            int objTotalCount = scene.SpatialObjects.Count;

            // add a new render set if there is more objects than there is rendersets avaliable
            if (objTotalCount > MaxRenders * renderSets.Count)
            {
                renderSets.Add(new RayTraceRenderSet());
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
            switch (Settings.RendererSettings.OptimizeUpdatingBuffers)
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
                        if (GetTime() % 1 >= 0.95f || objectBeforeCount != objTotalCount)
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
                renderSets[i].DrawSetObject(in scene.SpatialObjects, ref shader, beCount, objCount, view, proj, camPos);
                count -= MaxRenders;
                beCount = objCount;
            }
            objectBeforeCount = objTotalCount;

            //UiRenderer.Draw();
        }
    }
}
