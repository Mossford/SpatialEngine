#version 460 core
layout (location = 0) in vec2 aPos;

uniform int uindex;
uniform int uindOffset;
uniform int uindEnd;

out int index;
out int indOffset;
out int indEnd;

void main()
{
    index = uindex;
    indOffset = uindOffset;
    indEnd = uindEnd;
    gl_Position = vec4(aPos, 0.0, 1.0); 
}  