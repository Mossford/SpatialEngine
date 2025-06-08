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
uniform int indexOffset;
uniform int triCount;
uniform vec3 ucamPos;
uniform vec3 ucamDir;
uniform mat4 uView;
uniform mat4 uProj;

struct HitInfo
{
	vec3 color;
	bool hit;
};

vec3 lightsource = vec3(0.0, 20.0, 20);

bool rayTriIntersect(vec3 orig, vec3 dir, int index)
{
	vec3 aPos = vec3(model.modelMat[uindex] * vec4(vertexBuf.vert[indiceBuf.ind[index * 3 + indexOffset]].pos, 1.0));
	vec3 bPos = vec3(model.modelMat[uindex] * vec4(vertexBuf.vert[indiceBuf.ind[index * 3 + 1 + indexOffset]].pos, 1.0));
	vec3 cPos = vec3(model.modelMat[uindex] * vec4(vertexBuf.vert[indiceBuf.ind[index * 3 + 2 + indexOffset]].pos, 1.0));
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

HitInfo castRay(vec3 orig, vec3 dir, int index)
{
	float t0;
	float diffuse_light_intensity = 0.0;
	vec3 normal;
	vec3 hit;
	vec3 color;
	HitInfo info;

	if (rayTriIntersect(orig, dir, index))
	{
		hit = orig + dir * t0;
		normal = vec3(1);
		color = vec3(0,0,0);
		float diffuse_light_intensity = 0.0;
		vec3 light_dir = lightsource - hit;
		diffuse_light_intensity = 1.5 * max(0.0, dot(light_dir,normal));
		info.color = vec3(0.7, 0.7, 0.7);
		info.hit = true;
		return info;
	}
	info.color = vec3(40.0 / 255.0, 40.0 / 255.0, 40.0 / 255.0);
	info.hit = false;
	return info;
}

void main()
{
	
	vec2 fragPos = vec2(gl_FragCoord.x / (1920.0 + 1.0) - 0.5, gl_FragCoord.y / (1080.0 + 1.0) - 0.5);
	vec4 target = inverse(uProj) * vec4(fragPos, 1.0, 1.0);
	vec3 rayDir = vec3(inverse(uView) * vec4(normalize(vec3(target) / target.w), 0.0));

	vec3 color = vec3(0);

	HitInfo info;

	info = castRay(ucamPos, rayDir, triCount);
	if(info.hit == true)
	{
		out_color = vec4(info.color, 1.0);
	}
	else
	{

	}
	
	if(!info.hit)
		discard;
}