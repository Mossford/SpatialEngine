using JoltPhysicsSharp;
using Silk.NET.Vulkan;
using SpatialEngine.Rendering;
using System;
using System.Numerics;
using static SpatialEngine.Globals;
using Vertex = SpatialEngine.Rendering.Vertex;

namespace SpatialEngine.Terrain
{
    public class Terrain
    {
        public int id { get; private set; }

        public int width { get; private set; }
        public int length { get; private set; }
        public float scale { get; private set; }

        public Terrain(int width, int length, float scale = 1f)
        {
            this.width = width;
            this.length = length;
            this.scale = scale;
            CreateTerrain();
        }


        public void CreateTerrain()
        {
            Vertex[] vertexes = new Vertex[width * length];
            uint[] ind = new uint[((width -1) * (length - 1)) * 6];
            float lengthOffset = (length - 1) / -2f;
            float widthOffset = (width - 1) / 2f;

            int vertIndex = 0;
            int indIndex = 0;
            Random rand = new Random();
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    Vector3 pos = new Vector3((lengthOffset + x) * scale, Math.SimplexNoise2D(x, y) * scale, (widthOffset - y) * scale);
                    Vector2 uv = new Vector2(x / (float)width, y / (float)length);
                    vertexes[vertIndex] = new Vertex(pos, Vector3.UnitY, uv);
                    if(x < length - 1 && y < width - 1)
                    {
                        ind[indIndex] = (uint)vertIndex;
                        ind[indIndex + 1] = (uint)(vertIndex + width + 1);
                        ind[indIndex + 2] = (uint)(vertIndex + width);
                        indIndex += 3;

                        ind[indIndex] = (uint)(vertIndex + width + 1);
                        ind[indIndex + 1] = (uint)(vertIndex);
                        ind[indIndex + 2] = (uint)(vertIndex + 1);
                        indIndex += 3;
                    }
                    vertIndex++;
                }
            }

            id = scene.SpatialObjects.Count;
            scene.AddSpatialObject(new Mesh(vertexes, ind, new Vector3(0, 4, 0), Quaternion.Identity), new Vector3(((length - 1) * scale) / 2f, 1, ((width - 1) * scale - 1) / 2f), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);

        }
    }
}
