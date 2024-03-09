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

        public Terrain()
        {
            Vertex[] vertexes =
            {
                new(new(-1, 0, 1), new(0,1,0), new(0,0)),
                new(new(1, 0, 1), new(0,1,0), new(1,0)),
                new(new(-1, 0, -1), new(0,1,0), new(0,1)),
                new(new(1, 0, -1), new(0,1,0), new(1,1))
            };
            uint[] ind = { 0, 1, 2, 1, 3, 2 };

            id = scene.SpatialObjects.Count;
            scene.AddSpatialObject(new Mesh(vertexes, ind, new Vector3(0,4,0), Quaternion.Identity));
            scene.SpatialObjects[id].SO_mesh.SubdivideTriangle();
            scene.SpatialObjects[id].SO_mesh.SubdivideTriangle();
            scene.SpatialObjects[id].SO_mesh.SubdivideTriangle();
            scene.SpatialObjects[id].SO_mesh.SubdivideTriangle();
            scene.SpatialObjects[id].SO_mesh.SubdivideTriangle();
            scene.SpatialObjects[id].SO_mesh.SubdivideTriangle();
            for (int i = 0; i < scene.SpatialObjects[id].SO_mesh.vertexes.Length; i++)
            {
                Random rand = new Random();
                scene.SpatialObjects[id].SO_mesh.vertexes[i].position.Y = (float)rand.NextDouble() * 0.01f;
            }
        }
    }
}
