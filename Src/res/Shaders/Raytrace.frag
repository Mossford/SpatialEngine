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

vec3 lightsource = vec3(0.0, 1.0, 0.7);

bool rayTriIntersect(vec3 orig, vec3 dir, uint a, uint b, uint c)
{
	vec3 aPos = (model.modelMat[0] * vec4(vertexBuf.vert[a].pos, 1.0)).xyz;
	vec3 bPos = (model.modelMat[0] * vec4(vertexBuf.vert[b].pos, 1.0)).xyz;
	vec3 cPos = (model.modelMat[0] * vec4(vertexBuf.vert[c].pos, 1.0)).xyz;
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

	return dt >= 0.000001 && dist >= 0 && u >= 0 && v >= 0 && w >= 0;
}

vec3 castRay(vec3 orig, vec3 dir, uint a, uint b, uint c)
{
	float sphere_dist = 10000.0;
	float t0;
	float diffuse_light_intensity = 0.0;
	vec3 normal;
	vec3 hit;
	vec3 color;


	if (rayTriIntersect(orig, dir, a, b, c))
	{
		hit = orig + dir * t0;
		normal = vec3(1);
		color = vec3(0,0,0);
		float diffuse_light_intensity = 0.0;
		vec3 light_dir = lightsource - hit;
		diffuse_light_intensity = 1.5 * max(0.0, dot(light_dir,normal));
		return vec3(1.0, 0,0) * diffuse_light_intensity;
	}
	return vec3(0.2471, 0.8784, 0.902);
}

void main()
{
	
	vec2 fragPos = vec2(gl_FragCoord.x / (1920.0 + 1) - 0.5, gl_FragCoord.y / (1080.0 + 1.0) - 0.5);
	vec4 target = inverse(uProj) * vec4(fragPos, 1.0, 1.0);
	vec3 rayDir = vec3(inverse(uView) * vec4(normalize(vec3(target) / target.w), 0.0));

	vec3 color = vec3(0);

	for(int i = 0; i < indiceBuf.ind.length(); i += 3)
	{
		uint a = indiceBuf.ind[i];
		uint b = indiceBuf.ind[i + 1];
		uint c = indiceBuf.ind[i + 2];
		//color = cast_ray(vec3(0), dir, objects);
		color = castRay(ucamPos, rayDir, a, b, c);
	}

	//color = cast_ray(ucamPos, rayDir, objects);

    out_color = vec4(color, 1.0);
}