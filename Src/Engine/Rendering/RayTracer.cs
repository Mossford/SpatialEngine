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
        
        //thank you rider
        struct RayVertex(Vector3 pos, Vector2 uv)
        {
            public Vector4 pos { get; init; } = new Vector4(pos, 1.0f);
            public Vector4 uv { get; init; } = new Vector4(uv, 1.0f, 1.0f);
        }

        public class RayTraceRenderSet : IDisposable
        {
            List<MeshOffset> meshOffsets;
            BufferObject<Matrix4x4> modelMatrixes;
            BufferObject<RayVertex> vertexes;
            BufferObject<Vector4> indices;

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

                RayVertex[] verts = new RayVertex[vertexSize];
                Vector4[] inds = new Vector4[indiceSize / 3];
                int countV = 0;
                int countI = 0;
                int count = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    models[count] = objs[i].SO_mesh.modelMat;
                    for (int j = 0; j < objs[i].SO_mesh.vertexes.Length; j++)
                    {
                        verts[countV] = new RayVertex(objs[i].SO_mesh.vertexes[j].position, objs[i].SO_mesh.vertexes[j].uv);
                        countV++;
                    }
                    for (int j = 0; j < objs[i].SO_mesh.indices.Length; j += 3)
                    {
                        inds[countI] = new Vector4(objs[i].SO_mesh.indices[j], objs[i].SO_mesh.indices[j + 1], objs[i].SO_mesh.indices[j + 2], 1f);
                        countI++;
                    }
                    count++;
                }

                modelMatrixes = new BufferObject<Matrix4x4>(models, 3, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);
                vertexes = new BufferObject<RayVertex>(verts, 4, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);
                indices = new BufferObject<Vector4>(inds, 5, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);
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

                RayVertex[] verts = new RayVertex[vertexSize];
                Vector4[] inds = new Vector4[indiceSize / 3];
                int countV = 0;
                int countI = 0;
                int count = 0;
                for (int i = countBE; i < countTO; i++)
                {
                    models[count] = objs[i].SO_mesh.modelMat;
                    for (int j = 0; j < objs[i].SO_mesh.vertexes.Length; j++)
                    {
                        verts[countV] = new RayVertex(objs[i].SO_mesh.vertexes[j].position, objs[i].SO_mesh.vertexes[j].uv);
                        countV++;
                    }
                    for (int j = 0; j < objs[i].SO_mesh.indices.Length; j += 3)
                    {
                        inds[countI] = new Vector4(objs[i].SO_mesh.indices[j], objs[i].SO_mesh.indices[j + 1], objs[i].SO_mesh.indices[j + 2], 1f);
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
                    
                    shader.setMat4("uView", view);
                    shader.setMat4("uProj", proj);
                    shader.setVec3("ucamPos", camPos);
                    shader.setVec3("ucamDir", player.camera.GetCamDir());
                    shader.setInt("uindex", count);
                    shader.setFloat("speedOfRay", 10);
                    shader.setFloat("mass", 100000000000);
                    shader.setInt("raySteps", 50);
                    shader.setInt("vertStart", meshOffsets[count].offset);
                    shader.setInt("triCount", objs[i].SO_mesh.indices.Length / 3);
    
                    quad.Draw();

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
        public static Shader shader;

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
