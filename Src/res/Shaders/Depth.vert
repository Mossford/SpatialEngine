#version 460 core
layout (location = 0) in vec3 aPos;
layout (std140, binding = 3) restrict buffer models
{
    mat4 modelMat[];
} model;

uniform mat4 projection;
uniform mat4 view;
uniform bool meshDraw;
uniform mat4 modelMeshDraw;
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
        matrix = modelMeshDraw;
    }
    //funny fix to percision errors
    gl_Position = projection * view * matrix * vec4(aPos, 1.0);
    gl_Position.z = 2.0*log(gl_Position.w/1)/log(2000/1) - 1; 
    gl_Position.z *= gl_Position.w;
}