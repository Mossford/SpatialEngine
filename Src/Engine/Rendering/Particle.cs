using System.Numerics;
using System.Runtime.InteropServices;

namespace SpatialEngine.Rendering
{
    public enum ParticleSettings
    {
        Kinematic,
        Gravity
    }

    /// <summary>
    /// For the gpu
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct ParticleGpu
    {
        public Vector3 position;
        public float size;
        public Quaternion rotation;
        public Vector4Byte color;
        //will tell which particle to use in the shader
        public uint id;
        public Vector2 packing;

        public ParticleGpu()
        {
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            size = 1f;
            color = new Vector4Byte(255, 255, 255, 255);
            //0xABC, 0xCBA for debugging to show end of a particle
            packing = new Vector2(1.81037415e-32f, 2.86578376e-31f);
        }

        public ParticleGpu(Vector3 position, Quaternion rotation, float size, Vector4Byte color, uint id)
        {
            this.position = position;
            this.rotation = rotation;
            this.size = size;
            this.color = color;
            this.id = id;
            //0xABC, 0xCBA for debugging to show end of a particle
            packing = new Vector2(1.81037415e-32f, 2.86578376e-31f);
        }
    }

    /// <summary>
    /// for cpu calculations
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct Particle
    {
        public Vector3 velocity;
        public float timeSpawn;
        public float lifetime;
        public int id;
        public bool enabled;
        public ParticleSettings settings;

        public Particle()
        {
            velocity = Vector3.Zero;
            lifetime = 0f;
            id = -1;
            enabled = false;
            timeSpawn = 0f;
        }

        public Particle(Vector3 velocity, float lifetime, float timeSpawn, int id, bool enabled, ParticleSettings settings)
        {
            this.velocity = velocity;
            this.lifetime = lifetime;
            this.id = id;
            this.enabled = enabled;
            this.timeSpawn = timeSpawn;
            this.settings = settings;
        }
    }
}