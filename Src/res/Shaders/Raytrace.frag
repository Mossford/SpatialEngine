#version 460 core

out vec4 out_color;

struct vertex 
{
    vec3 pos;
    vec3 normal;
    vec2 uv;
};

layout (std140, binding = 3) restrict buffer models
{
    mat4 modelMat[];
} model;

layout (std140, binding = 4) restrict buffer vertexes
{
    vertex vert[];
} vertexBuf;

//passed in as size in bytes
layout (std140, binding = 5) restrict buffer indices
{
    uint ind[];
} indiceBuf;

uniform int uindex;
uniform int uindOffset;
uniform int uindEnd;
uniform vec3 ucamPos;
uniform mat4 uView;
uniform mat4 uProj;

vec3 lightsource = vec3(0.0, 20.0, 20);

bool rayTriIntersect(vec3 orig, vec3 dir, int index)
{
	vec3 aPos = vec3(vec4(vertexBuf.vert[indiceBuf.ind[index * 3] / 4].pos, 1.0));
	vec3 bPos = vec3(vec4(vertexBuf.vert[indiceBuf.ind[index + 1 * 3] / 4].pos, 1.0));
	vec3 cPos = vec3(vec4(vertexBuf.vert[indiceBuf.ind[index + 2 * 3] / 4].pos, 1.0));
	vec3 edgeAB = bPos - aPos;
	vec3 edgeAC = cPos - aPos;
	vec3 normal = cross(edgeAB, edgeAC);
	vec3 ao = orig - aPos;
	vec3 dao = cross(ao, dir);

	float dt = -dot(dir, normal);
	float invDt = 1.0 / dt;

	float dist = dot(ao, normal) * invDt;
	float u = dot(edgeAC, dao) * invDt;
	float v = -dot(edgeAB, dao) * invDt;
	float w = 1 - u - v;

	return dt >= 0.0000001 && dist >= 0 && u >= 0 && v >= 0 && w >= 0;
}

vec3 castRay(vec3 orig, vec3 dir, int index)
{
	float t0;
	float diffuse_light_intensity = 0.0;
	vec3 normal;
	vec3 hit;
	vec3 color;

	if (rayTriIntersect(orig, dir, index))
	{
		hit = orig + dir * t0;
		normal = vec3(1);
		color = vec3(0,0,0);
		float diffuse_light_intensity = 0.0;
		vec3 light_dir = lightsource - hit;
		diffuse_light_intensity = 1.5 * max(0.0, dot(light_dir,normal));
		return vec3(1.0, 0, 0);
	}
	return vec3(102.0 / 255.0, 178.0 / 255.0, 204.0 / 255.0);
}

void main()
{
	
	vec2 fragPos = vec2(gl_FragCoord.x / (1920.0 + 1.0) - 0.5, gl_FragCoord.y / (1080.0 + 1.0) - 0.5);
	vec4 target = inverse(uProj) * vec4(fragPos, 1.0, 1.0);
	vec3 rayDir = vec3(inverse(uView) * vec4(normalize(vec3(target) / target.w), 0.0));

	vec3 color = vec3(0);

	color = castRay(ucamPos, rayDir, 0);
	color = castRay(ucamPos, rayDir, 1);

    out_color = vec4(color, 1.0);
}