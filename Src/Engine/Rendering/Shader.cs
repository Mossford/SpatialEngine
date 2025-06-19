using System;
using System.Numerics;
using Silk.NET.OpenGL;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpatialEngine.Rendering
{

    public struct ShaderUniform
    {
        public string name;
        public int location;

        public ShaderUniform(string name, int location)
        {
            this.name = name;
            this.location = location;
        }
    }

    public class Shader : IDisposable
    {
        uint vertShaderU;
        uint fragShaderU;
        uint compShaderU;
        GL gl;
        public uint shader;
        int prevLocationIndex;
        public List<ShaderUniform> uniformList;

        /// <summary>
        /// Only holds the vertex file name but not the shader path
        /// </summary>
        public string vertPath { get; private set; }
        /// <summary>
        /// Only holds the fragment file name but not the shader path
        /// </summary>
        public string fragPath { get; private set; }
        public string compPath { get; private set; }

        public unsafe Shader(GL gl, string vertPath, string fragPath)
        {
            //get shader file code
            this.vertPath = vertPath;
            this.fragPath = fragPath;
            string vertexCode = File.ReadAllText(Resources.ShaderPath + vertPath);
            string fragCode = File.ReadAllText(Resources.ShaderPath + fragPath);
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

            uniformList = new List<ShaderUniform>();
            prevLocationIndex = 0;

            this.gl = gl;
        }
        
        public unsafe Shader(GL gl, string compPath)
        {
            //get shader file code
            this.compPath = compPath;
            string compCode = File.ReadAllText(Resources.ShaderPath + compPath);
            //compile shader
            compShaderU = gl.CreateShader(ShaderType.ComputeShader);
            gl.ShaderSource(compShaderU, compCode);
            gl.CompileShader(compShaderU);
            gl.GetShader(compShaderU, ShaderParameterName.CompileStatus, out int cStatus);
            if (cStatus != (int)GLEnum.True)
                throw new Exception("Compute shader failed to compile: " + gl.GetShaderInfoLog(compShaderU));

            shader = gl.CreateProgram();

            //link and attach shader
            gl.AttachShader(shader, compShaderU);
            gl.LinkProgram(shader);
            gl.GetProgram(shader, ProgramPropertyARB.LinkStatus, out int lStatus);
            if (lStatus != (int) GLEnum.True)
                throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(shader));
            
            //detach shader
            gl.DetachShader(shader, compShaderU);
            gl.DeleteShader(compShaderU);

            uniformList = new List<ShaderUniform>();
            prevLocationIndex = 0;

            this.gl = gl;
        }

        int GetUniformLocation(string name)
        {
            if (prevLocationIndex != 0 && uniformList[prevLocationIndex].name == name)
                return uniformList[prevLocationIndex].location;
            for (int i = 0; i < uniformList.Count; i++)
            {
                if (name == uniformList[i].name)
                {
                    prevLocationIndex = i;
                    return uniformList[i].location;
                }
            }

            int loc = gl.GetUniformLocation(shader, name);
            uniformList.Add(new ShaderUniform(name, loc));
            return loc;
        }

        public void setBool(string name, bool value)
        {
            int location = GetUniformLocation(name);
            if(value)
                gl.Uniform1(location, 1);
            else
                gl.Uniform1(location, 0);
        }
        
        public void setBool(int location, bool value)
        {
            if(value)
                gl.Uniform1(location, 1);
            else
                gl.Uniform1(location, 0);
        }
        
        public void setInt(string name, int value)
        {
            int location = GetUniformLocation(name);
            gl.Uniform1(location, value);
        }
        public void setInt(int location, int value)
        {
            gl.Uniform1(location, value);
        }
        public void setFloat(string name, float value)
        {
            int location = GetUniformLocation(name);
            gl.Uniform1(location, value);
        }
        public void setFloat(int location, float value)
        {
            gl.Uniform1(location, value);
        }
        public unsafe void setVec2(string name, Vector2 value)
        {
            int location = GetUniformLocation(name);
            gl.Uniform2(location, value);
        }
        public unsafe void setVec2(int location, Vector2 value)
        {
            gl.Uniform2(location, value);
        }
        public unsafe void setVec2(string name, float x, float y)
        {
            int location = GetUniformLocation(name);
            gl.Uniform2(location, new Vector2(x,y));
        }
        public unsafe void setVec2(int location, float x, float y)
        {
            gl.Uniform2(location, new Vector2(x,y));
        }
        public unsafe void setVec3(string name, Vector3 value)
        {
            int location = GetUniformLocation(name);
            gl.Uniform3(location, value);
        }
        public unsafe void setVec3(int location, Vector3 value)
        {
            gl.Uniform3(location, value);
        }
        public unsafe void setVec3(string name, float x, float y, float z)
        {
            int location = GetUniformLocation(name);
            gl.Uniform3(location, new Vector3(x, y, z));
        }
        public unsafe void setVec3(int location, float x, float y, float z)
        {
            gl.Uniform3(location, new Vector3(x, y, z));
        }
        public unsafe void setVec4(string name, Vector4 value)
        {
            int location = GetUniformLocation(name);
            gl.Uniform4(location, value);
        }
        public unsafe void setVec4(int location, Vector4 value)
        {
            gl.Uniform4(location, value);
        }
        public unsafe void setVec4(string name, float x, float y, float z, float w)
        {
            int location = GetUniformLocation(name);
            gl.Uniform4(location, new Vector4(x, y, z, w));
        }
        public unsafe void setVec4(int location, float x, float y, float z, float w)
        {
            gl.Uniform4(location, new Vector4(x, y, z, w));
        }
        public unsafe void setMat4(string name, Matrix4x4 mat)
        {
            int location = GetUniformLocation(name);
            gl.UniformMatrix4(location, 1, false, (float*)&mat);
        }
        public unsafe void setMat4(int location, Matrix4x4 mat)
        {
            gl.UniformMatrix4(location, 1, false, (float*)&mat);
        }
        public unsafe void setMat3x2(string name, Matrix3x2 mat)
        {
            int location = GetUniformLocation(name);
            gl.UniformMatrix3x2(location, 1, false, (float*)&mat);
        }
        public unsafe void setMat3x2(int location, Matrix3x2 mat)
        {
            gl.UniformMatrix3x2(location, 1, false, (float*)&mat);
        }
        
        public void Dispose()
        {
            gl.DeleteProgram(shader);
        }
    }
}