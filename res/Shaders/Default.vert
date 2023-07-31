#version 330 core

layout (location = 0) in vec3 aPosition;

uniform mat4 uModel;
uniform mat4 uProj;
uniform mat4 uView;

void main()
{
    gl_Position = uProj * uView * uModel * vec4(aPosition, 1.0);
}