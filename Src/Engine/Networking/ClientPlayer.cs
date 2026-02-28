using System.Numerics;

namespace SpatialEngine.Networking
{
    public class ClientPlayer
    {
        public int id;
        public Vector3 position;
        public Quaternion rotation;

        public ClientPlayer(int id, Vector3 position, Quaternion rotation)
        {
            this.id = id;
            this.position = position;
            this.rotation = rotation;
        }
    }
}