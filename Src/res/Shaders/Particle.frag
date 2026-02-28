#version 460 core

out vec4 out_color;
in flat int index;

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

void main()
{
    vec4 color = unpackUnorm4x8(particleBuffer.parts[index].color);
    out_color = color;
}
