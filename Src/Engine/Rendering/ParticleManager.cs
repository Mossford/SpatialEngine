using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.OpenGL;
using static SpatialEngine.Globals;

namespace SpatialEngine.Rendering
{
    public static class ParticleManager
    {
        public static Particle[] particles;
        public static ParticleGpu[] particlesGpu;
        public static ParticleGpu[] currentLoadedParticles;
        public static int[] currentLoadedParticleIndex;
        public static BufferObject<ParticleGpu> particleBuffer;
        public static Shader particleShader;
        public static Mesh quad;
        public static int currentParticles;
        public static Random random;
        static int currentSelectedParticle;
        static List<ParticleEmitter> emitters;
        
        public static uint vao { get; private set; }
        public static uint vbo { get; private set; }
        public static uint ebo { get; private set; }
        
        public static unsafe void Init()
        {
            particles = new Particle[Settings.RendererSettings.MaxParticles];
            particlesGpu = new ParticleGpu[Settings.RendererSettings.MaxParticles];
            currentLoadedParticles = new ParticleGpu[0];
            currentLoadedParticleIndex = new int[Settings.RendererSettings.MaxParticles];
            particleBuffer = new BufferObject<ParticleGpu>(particlesGpu, Settings.RendererSettings.ParticleBuffer, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);
            particleShader = new Shader(Globals.gl, "Particle.vert", "Particle.frag");
            quad = MeshUtils.Create2DQuad(Vector3.Zero, Quaternion.Identity);
            currentSelectedParticle = 0;
            random = new Random();
            emitters = new List<ParticleEmitter>();

            for (int i = 0; i < particles.Length; i++)
            {
                particles[i] = new Particle();
                particlesGpu[i] = new ParticleGpu();
            }
            
            vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vertex* buf = quad.vertexes)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(quad.vertexes.Length * sizeof(Vertex)), buf, BufferUsageARB.StreamDraw);
            fixed (uint* buf = quad.indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(quad.indices.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)0);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(float)));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex), (void*)(6 * sizeof(float)));
            gl.BindVertexArray(0);
        }

        public static void Update(float dt)
        {
            for (int i = 0; i < emitters.Count; i++)
            {
                emitters[i].Update(dt);
                if (emitters[i].finished)
                {
                    emitters.RemoveAt(i);
                }
            }
            
            currentParticles = 0;
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].enabled)
                {
                    particles[i].timeSpawn += dt;
                    switch (particles[i].settings)
                    {
                        case ParticleSettings.Kinematic:
                        {
                            particlesGpu[i].position += particles[i].velocity * dt;
                            break;
                        }
                        case ParticleSettings.Gravity:
                        {
                            particles[i].velocity += new Vector3(0, -9.81f, 0) * dt;
                            particlesGpu[i].position += particles[i].velocity * dt;
                            break;
                        }
                    }
                    if (particles[i].timeSpawn >= particles[i].lifetime)
                    {
                        particles[i].enabled = false;
                        particles[i].timeSpawn = 0f;
                    }
                    else
                    {
                        currentLoadedParticleIndex[currentParticles] = i;
                        currentParticles++;
                    }
                }
            }
            
            if(currentParticles == 0)
                return;

            currentLoadedParticles = new ParticleGpu[currentParticles];
            for (int i = 0; i < currentParticles; i++)
            {
                currentLoadedParticles[i] = particlesGpu[currentLoadedParticleIndex[i]];
            }
            
            particleBuffer.SubUpdate(currentLoadedParticles);
        }

        public static unsafe void Render(in Matrix4x4 view, in Matrix4x4 proj)
        {
            gl.Disable(GLEnum.CullFace);
            gl.UseProgram(particleShader.shader);
            particleBuffer.Bind();
            particleShader.setMat4("view", view);
            particleShader.setMat4("projection", proj);
            gl.BindVertexArray(vao);
            gl.DrawElementsInstanced(GLEnum.Triangles, (uint)quad.indices.Length, GLEnum.UnsignedInt, (void*)0, (uint)currentParticles);
            gl.BindVertexArray(0);
            gl.Enable(GLEnum.CullFace);

            drawCallCount++;
        }

        public static int RequestParticleIndex()
        {
            currentSelectedParticle %= Settings.RendererSettings.MaxParticles;
            int index = currentSelectedParticle;
            currentSelectedParticle++;
            return index;
        }

        public static void AddEmitter(ParticleEmitter emitter)
        {
            emitters.Add(emitter);
        }

        public static void Clean()
        {
            gl.DeleteVertexArray(vao);
            gl.DeleteBuffer(vbo);
            gl.DeleteBuffer(ebo);
        }
    }
}