#version 460 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (std140, binding = 3) restrict buffer models
{
    mat4 modelMat[];
} model;

out vec2 TexCoords;

out VS_OUT 
{
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    //vec4 FragPosLightSpace;
} vs_out;

uniform mat4 projection;
uniform mat4 view;
//uniform mat4 model;
//uniform mat4 lightSpaceMatrix;

void main()
{
    int index = gl_DrawID;
    vs_out.FragPos = vec3(model.modelMat[index] * vec4(aPos, 1.0));
    vs_out.Normal = transpose(inverse(mat3(model.modelMat[index]))) * aNormal;
    vs_out.TexCoords = aTexCoords;
    //vs_out.FragPosLightSpace = lightSpaceMatrix * vec4(vs_out.FragPos, 1.0);
    gl_Position = projection * view * model.modelMat[index] * vec4(aPos, 1.0);
}