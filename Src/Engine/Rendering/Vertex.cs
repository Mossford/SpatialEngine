using System.Numerics;

namespace SpatialEngine.Rendering
{
    public struct Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        public Vertex(Vector3 pos, Vector3 nor, Vector2 tex)
        {
            position = pos;
            normal = nor;
            uv = tex;
        }
    }
}