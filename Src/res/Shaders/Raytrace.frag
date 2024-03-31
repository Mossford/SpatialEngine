#version 460 core

out vec4 out_color;

struct vertex 
{
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    //vec4 FragPosLightSpace;
};

layout (std140, binding = 3) restrict buffer models
{
    mat4 modelMat[];
} model;

layout (std140, binding = 4) restrict buffer vertexes
{
    vertex vert[];
} vertexBuf;

layout (std140, binding = 5) restrict buffer indices
{
    uint ind[];
} indiceBuf;

flat in int index;
flat in int indOffset;
flat in int indEnd;

void main()
{
    out_color = vec4(vec3(1), 1.0);
}