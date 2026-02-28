#version 460 core

layout (location = 0) in vec3 aPos;

struct particle
{
    //xyz, size
    vec4 position;
    vec4 rotation;
    uint color;
    int id;
};

layout (std430, binding = 6) restrict buffer particles
{
    particle[] parts;
} particleBuffer;

uniform mat4 projection;
uniform mat4 view;

out flat int index;

vec3 rotateByQuat(vec3 v, vec4 q)
{
    return v + 2.0 * cross(q.xyz, cross(q.xyz, v) + q.w * v);
}

void main()
{
    index = gl_InstanceID;
    particle p = particleBuffer.parts[index];
    
    vec3 rotatedPos = rotateByQuat(aPos * p.position.w, p.rotation);
    
    gl_Position = projection * view * vec4(rotatedPos + p.position.xyz, 1.0);
}