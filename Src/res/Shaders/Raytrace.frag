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

flat in int baseVertex;

void main()
{
    mat4 test = model.modelMat[baseVertex];
    vertex test2 = vertexBuf.vert[baseVertex];
    uint test3 = indiceBuf.ind[baseVertex];
    out_color = vec4(vec3(1 / baseVertex + 1), 1.0);
}