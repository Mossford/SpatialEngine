#version 460 core
layout (location = 0) in vec2 aPos;

uniform int index;

out int baseVertex;

void main()
{
    baseVertex = index;
    gl_Position = vec4(aPos, 0.0, 1.0); 
}  