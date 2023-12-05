using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Silk.NET.Maths;

namespace SpatialEngine
{
    public struct SpatialObject
    {
        public Mesh SO_mesh;
        public uint SO_id;

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
            if(mesh == null)
                return;
            int id = SpatialObjects.Count;
            SpatialObjects.Add(new SpatialObject(mesh, (uint)id));
            SpatialObjects[id].SO_mesh.BufferGens();
            idList.Add((uint)id);
        }

        public void DrawSingle(ref Shader shader, Matrix4x4 view, Matrix4x4 proj, Vector3 camPos)
        {
            shader.SetUniform("view", view);
            shader.SetUniform("projection", proj);
            shader.SetUniform("viewPos", camPos);
            for (int i = 0; i < SpatialObjects.Count; i++)
            {
                shader.SetUniform("model", SpatialObjects[i].SO_mesh.modelMat);
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