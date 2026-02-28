using System;
using JoltPhysicsSharp;
using SpatialEngine.Rendering;

namespace SpatialEngine
{
    public class SpatialObject : IDisposable
    {
        public Mesh mesh;
        public RigidBody rigidbody;
        //references to a texture
        public Texture texture;
        //references to a shader
        public Shader shader;
        public int id;
        public bool enabled;

        public SpatialObject(Mesh mesh, int id)
        {
            this.mesh = new Mesh(mesh);
            texture = new Texture();
            SetTexture(Renderer.defaultTexture);
            this.id = id;
            this.enabled = true;
        }

        public SpatialObject(Mesh mesh, MotionType motion, ObjectLayer layer, Activation activation, int id)
        {
            this.mesh = new Mesh(mesh);
            texture = new Texture();
            SetTexture(Renderer.defaultTexture);
            rigidbody = new RigidBody(this.mesh, this.id, this.mesh.position, this.mesh.rotation, motion, layer);
            rigidbody.AddToPhysics(ref Globals.bodyInterface, activation);
            this.id = id;
            this.enabled = true;
        }

        public SpatialObject(Mesh mesh, MotionType motion, ObjectLayer layer, Activation activation, string vertPath, string fragPath, int id)
        {
            this.mesh = new Mesh(mesh);
            texture = new Texture();
            SetTexture(Renderer.defaultTexture);
            rigidbody = new RigidBody(this.mesh, this.id, this.mesh.position, this.mesh.rotation, motion, layer);
            shader = new Shader(Globals.gl, vertPath, fragPath);
            rigidbody.AddToPhysics(ref Globals.bodyInterface, activation);
            this.id = id;
            this.enabled = true;
        }

        public void SetTexture(in Texture texture)
        {
            this.texture = texture;
        }
        
        public void SetTexture(string textureLocation)
        {
            this.texture = TextureManager.RetrieveTexture(textureLocation);
        }

        public void SetShader(in Shader shader)
        {
            this.shader = shader;
        }

        public void SetMesh(in Mesh mesh)
        {
            this.mesh = new Mesh(mesh);
        }

        public uint GetSizeUsage()
        {
            uint total = 0;
            total += (uint)(8 * sizeof(float) * mesh.vertexes.Length);
            total += (uint)(sizeof(uint) * mesh.indices.Length);
            return total;
        }

        public void Dispose()
        {
            mesh.Dispose();
            rigidbody.Dispose();
            //remove when shader becomes a reference
            shader?.Dispose();
        }
    }
}