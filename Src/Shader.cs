using System;
using System.Numerics;
using Silk.NET.OpenGL;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpatialEngine
{

    public class ShaderUniform
    {
        public string name;
        public int location;

        public ShaderUniform(string name, int location)
        {
            this.name = name;
            this.location = location;
        }
    }

    public class Shader
    {
        uint vertShaderU;
        uint fragShaderU;
        GL gl;
        public uint shader;
        int prevLocationIndex;
        public List<ShaderUniform> uniformList;

        public unsafe Shader(GL gl, string vertPath, string fragPath)
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
        public void setInt(string name, int value)
        {
            int location = GetUniformLocation(name);
            gl.Uniform1(location, (float)value);
        }
        public void setFloat(string name, float value)
        {
            int location = GetUniformLocation(name);
            gl.Uniform1(location, (float)value);
        }
        public unsafe void setVec2(string name, Vector2 value)
        {
            int location = GetUniformLocation(name);
            gl.Uniform2(location, value);
        }
        public unsafe void setVec2(string name, float x, float y)
        {
            int location = GetUniformLocation(name);
            gl.Uniform2(location, new Vector2(x,y));
        }
        public unsafe void setVec3(string name, Vector3 value)
        {
            int location = GetUniformLocation(name);
            gl.Uniform3(location, value);
        }
        public unsafe void setVec3(string name, float x, float y, float z)
        {
            int location = GetUniformLocation(name);
            gl.Uniform3(location, new Vector3(x, y, z));
        }
        public unsafe void setVec4(string name, Vector4 value)
        {
            int location = GetUniformLocation(name);
            gl.Uniform4(location, value);
        }
        public unsafe void setVec4(string name, float x, float y, float z, float w)
        {
            int location = GetUniformLocation(name);
            gl.Uniform4(location, new Vector4(x, y, z, w));
        }
        public unsafe void setMat4(string name, Matrix4x4 mat)
        {
            int location = GetUniformLocation(name);
            gl.UniformMatrix4(location, 1, false, (float*)&mat);
        }
    }
}