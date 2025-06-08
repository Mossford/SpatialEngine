#version 460 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (std140, binding = 3) restrict buffer models
{
    mat4 modelMat[];
} model;

out VS_OUT 
{
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    //vec4 FragPosLightSpace;
} vs_out;

uniform mat4 projection;
uniform mat4 view;
uniform bool meshDraw;
uniform int modelIndex;
//uniform mat4 lightSpaceMatrix;

void main()
{
    int index = gl_DrawID;
    mat4 matrix = mat4(0.0);
    if(!meshDraw)
    {
        matrix = model.modelMat[index];
    }
    if(meshDraw)
    {
        matrix = model.modelMat[modelIndex];
    }
    vs_out.FragPos = vec3(matrix * vec4(aPos, 1.0));
    vs_out.Normal = transpose(inverse(mat3(matrix))) * aNormal;
    vs_out.TexCoords = aTexCoords;
    //vs_out.FragPosLightSpace = lightSpaceMatrix * vec4(vs_out.FragPos, 1.0);
    gl_Position = projection * view * matrix * vec4(aPos, 1.0);
}