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
        public static int MaxRenders;
        public static List<RenderSet> renderSets;
        static int objectBeforeCount = 0;

        public static void Init(in Scene scene, int maxRenders = 10000)
        {
            renderSets = new List<RenderSet>();
            MaxRenders = maxRenders;
            renderSets.Add(new RenderSet());
            renderSets[0].CreateDrawSet(in scene.SpatialObjects, 0, scene.SpatialObjects.Count);

            //RayTracer.Init(scene, maxRenders);
        }

        public static void Draw(in Scene scene, ref Shader shader, in Matrix4x4 view, in Matrix4x4 proj, in Vector3 camPos)
        {

            int objTotalCount = scene.SpatialObjects.Count;

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

            //RayTracer.Draw(scene, view, proj, camPos);
        }

        public static void DrawNoShader(in Scene scene)
        {

            int objTotalCount = scene.SpatialObjects.Count;

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
                renderSets[i].DrawSetNoAssign(in scene.SpatialObjects, beCount, objCount);
                count -= MaxRenders;
                beCount = objCount;
            }
            objectBeforeCount = objTotalCount;

            //RayTracer.Draw(scene, view, proj, camPos);
        }

    }
}
