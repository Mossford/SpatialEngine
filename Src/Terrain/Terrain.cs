using JoltPhysicsSharp;
using Silk.NET.Vulkan;
using SpatialEngine.Rendering;
using System;
using System.Numerics;
using static SpatialEngine.Globals;
using Vertex = SpatialEngine.Rendering.Vertex;
using SpatialEngine.SpatialMath;

namespace SpatialEngine.Terrain
{
    public class Terrain
    {
        public int id { get; private set; }

        public int width { get; private set; }
        public int length { get; private set; }
        public float scale { get; private set; }
        public int density { get; private set; }

        public Terrain(int width, int length, float scale = 1f, int density = 1)
        {
            this.width = width * density;
            this.length = length * density;
            this.scale = scale;
            this.density = density;
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
                    Vector3 pos = new Vector3((lengthOffset + x) * scale / density, Noise.CalcPixel2D(x, y, 0.1f) * 0.01f / density, (widthOffset - y) * scale / density);
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

            /*for (int i = 0; i < ind.Length; i += 3)
            {
                int indexA = i;
                int indexB = i + 1;
                int indexC = i + 2;

                Vertex vertA = vertexes[ind[indexA]];
                Vertex vertB = vertexes[ind[indexB]];
                Vertex vertC = vertexes[ind[indexC]];

                Vector3 u = vertB.position - vertA.position;
                Vector3 v = vertC.position - vertA.position;
                Vector3 normal = Vector3.Cross(u,v);

                vertexes[ind[indexA]].normal = normal;
                vertexes[ind[indexB]].normal = normal;
                vertexes[ind[indexC]].normal = normal;
            }*/


            id = scene.SpatialObjects.Count;
            scene.AddSpatialObject(new Mesh(vertexes, ind, new Vector3(0, 4, 0), Quaternion.Identity), new Vector3(((length - 1) * scale) / 2f, 1, ((width - 1) * scale - 1) / 2f), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
            scene.SpatialObjects[id].SO_mesh.CalculateNormalsSmooth();
        }
    }
}
