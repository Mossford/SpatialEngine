using System;
using System.Numerics;
using Silk.NET.OpenGL;
using System.IO;
using System.Collections.Generic;

namespace SpatialEngine
{
    public class Shader
    {
        uint vertShaderU;
        uint fragShaderU;
        GL gl;
        public uint shader;

        public Shader(GL gl, string vertPath, string fragPath)
        {
            //get shader file code
            string vertexCode = File.ReadAllText(vertPath);
            string fragCode = File.ReadAllText(fragPath);
            //compile shader
            vertShaderU = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vertShaderU, vertexCode);
            gl.CompileShader(vertShaderU);
            gl.GetShader(vertShaderU, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int) GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + gl.GetShaderInfoLog(vertShaderU));

            fragShaderU = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fragShaderU, fragCode);
            gl.CompileShader(fragShaderU);
            gl.GetShader(fragShaderU, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int) GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + gl.GetShaderInfoLog(fragShaderU));

            shader = gl.CreateProgram();

            //link and attach shader
            gl.AttachShader(shader, vertShaderU);
            gl.AttachShader(shader, fragShaderU);
            gl.LinkProgram(shader);
            gl.GetProgram(shader, ProgramPropertyARB.LinkStatus, out int lStatus);
            if (lStatus != (int) GLEnum.True)
                throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(shader));
            
            //detach shader
            gl.DetachShader(shader, vertShaderU);
            gl.DetachShader(shader, fragShaderU);
            gl.DeleteShader(vertShaderU);
            gl.DeleteShader(fragShaderU);

            this.gl = gl;
        }

        public unsafe void SetUniform<T>(string name, T value) where T : unmanaged
        {
            int location = gl.GetUniformLocation(shader, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            if(value is int || value is float)
                gl.Uniform1(location, (float)(object)value);
            if(value is Matrix4x4)
                gl.UniformMatrix4(location, 1, false, (float*) &value);
            if(value is Vector2)
                gl.Uniform2(location,  (Vector2)(object)value);
            if(value is Vector3)
                gl.Uniform3(location,  (Vector3)(object)value);
            if(value is Vector4)
                gl.Uniform4(location,  (Vector4)(object)value);
        }
    }
}