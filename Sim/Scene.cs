using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.Maths;

namespace SpatialEngine
{
    public struct SpatialObject
    {
        public Mesh SO_mesh;
        uint SO_id;

        public SpatialObject(Mesh mesh, uint id)
        {
            SO_mesh = mesh;
            SO_id = id;
        }

        public uint GetSizeUsage()
        {
            uint total = 0;
            total += (uint)(8 * sizeof(float) * SO_mesh.vertexes.Length);
            total += (uint)(sizeof(uint) * SO_mesh.indices.Length);
            return total;
        }
    }

    public class Scene
    {
        public List<SpatialObject> SpatialObjects;
        public List<uint> idList;

        public Scene()
        {
            SpatialObjects = new List<SpatialObject>();
            idList = new List<uint>();
        }

        public void AddSpatialObject(Mesh mesh)
        {
            int id = SpatialObjects.Count;
            SpatialObjects.Add(new SpatialObject(mesh, (uint)id));
            SpatialObjects[id].SO_mesh.BufferGens();
            idList.Add((uint)id);
        }

        public void DrawSingle(ref Shader shader, Matrix4x4 view, Matrix4x4 proj)
        {
            for (int i = 0; i < SpatialObjects.Count; i++)
            {
                shader.SetUniform("uModel", SpatialObjects[i].SO_mesh.modelMat);
                shader.SetUniform("uView", view);
                shader.SetUniform("uProj", proj);
                SpatialObjects[i].SO_mesh.DrawMesh();
            }
        }

        public void DeleteObjects()
        {

        }

        public void SaveScene()
        {

        }

        public void LoadScene()
        {

        }
    }
}