using System;
using System.Numerics;
using SpatialEngine.SpatialMath;

namespace SpatialEngine.Rendering
{
    public enum ParticleEmitterMode
    {
        Spherical
    }

    public struct ParticleEmitterSettings
    {
        public Vector3 position;
        public Vector3 rotation;
        public int count;
        public ParticleEmitterMode mode;
        public int particlePerUpdate;
        public float minLifeTime;
        public float maxLifeTime;
        public float speed;
        public Vector4Byte color;
        public float size;
        public ParticleSettings particleSettings;

        public ParticleEmitterSettings(Vector3 position, Vector3 rotation, int count, 
            ParticleEmitterMode mode, int particlePerUpdate, float minLifeTime, 
            float maxLifeTime, float speed, Vector4Byte color, float size, ParticleSettings particleSettings)
        {
            this.position = position;
            this.rotation = rotation;
            this.count = count;
            this.mode = mode;
            this.particlePerUpdate = particlePerUpdate;
            this.minLifeTime = minLifeTime;
            this.maxLifeTime = maxLifeTime;
            this.speed = speed;
            this.color = color;
            this.size = size;
            this.particleSettings = particleSettings;
        }
    }

    public class ParticleEmitter
    {
        ParticleEmitterSettings settings;
        public bool finished;

        int currentParticleCount;
        
        public ParticleEmitter(ParticleEmitterSettings settings)
        {
            this.settings = settings;
            finished = false;
        }

        public void Update(float dt)
        {
            //if we put in a negative value, the emitter will run for infinite time
            if (currentParticleCount >= settings.count)
            {
                finished = true;
                return;
            }
            
            switch (settings.mode)
            {
                case ParticleEmitterMode.Spherical:
                {
                    for (int i = 0; i < settings.particlePerUpdate; i++)
                    {
                        float x = MathF.Acos(2f * ParticleManager.random.NextSingle() - 1f);
                        float y = ParticleManager.random.NextSingle() * 2f * MathF.PI;
                        Vector3 target = MathS.GetRotDir(new Vector2(x, y));
                        int index = ParticleManager.RequestParticleIndex();
                        ParticleManager.particles[index] = new Particle(target * settings.speed, settings.minLifeTime + (ParticleManager.random.NextSingle() * settings.maxLifeTime), 0f, index, true, settings.particleSettings);
                        ParticleManager.particlesGpu[index] = new ParticleGpu(settings.position, Quaternion.CreateFromYawPitchRoll(target.X, target.Y, target.Z), settings.size, settings.color, (uint)index);
                        currentParticleCount++;
                    }
                    
                    break;
                }
            }
        }
    }
}