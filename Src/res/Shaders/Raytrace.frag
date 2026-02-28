#version 460 core

out vec4 out_color;

struct RayVertex
{
	vec4 pos;
	vec4 uv;
};

layout (std140, binding = 3) restrict buffer models {
	mat4 modelMat[];
} model;

layout (std140, binding = 4) restrict buffer vertexes {
	RayVertex vert[];
} vertexBuf;

layout(std430, binding = 5) restrict buffer indices {
	vec4 ind[];
} indiceBuf;

uniform sampler2D diffuseTexture;
uniform int uindex;
uniform int vertStart;
uniform int triCount;
uniform vec3 ucamPos;
uniform vec3 ucamDir;
uniform vec3 lightPos;
uniform mat4 uView;
uniform mat4 uProj;
uniform int totalTriCount;

ivec4 GetIndex(int i)
{
	return ivec4(ivec3(indiceBuf.ind[i]) + vertStart, indiceBuf.ind[i].w);
}

ivec4 GetIndexGlobal(int i)
{
	return ivec4(indiceBuf.ind[i]);
}

bool RayTriIntersect(vec3 orig, vec3 dir, int triIndex, vec3 a, vec3 b, vec3 c, out float t, out vec2 uv)
{
	vec3 edge1 = b - a;
	vec3 edge2 = c - a;
	vec3 pvec = cross(dir, edge2);
	float det = dot(edge1, pvec);
	if (abs(det) < 1e-6)
		return false;

	float invDet = 1.0 / det;
	vec3 tvec = orig - a;
	float u = dot(tvec, pvec) * invDet;
	if (u < 0.0 || u > 1.0)
		return false;

	vec3 qvec = cross(tvec, edge1);
	float v = dot(dir, qvec) * invDet;
	if (v < 0.0 || u + v > 1.0)
		return false;

	t = dot(edge2, qvec) * invDet;
	uv = vec2(u, v);
	return t > 0.0;
}

bool RayAabbIntersect(vec3 orig, vec3 dir, vec3 minPos, vec3 maxPos)
{
	vec3 invDir = 1.0 / dir;
	vec3 t0s = (minPos - orig) * invDir;
	vec3 t1s = (maxPos - orig) * invDir;

	vec3 ts = min(t0s, t1s);
	vec3 tb = max(t0s, t1s);

	float tmin = max(max(ts.x, ts.y), ts.z);
	float tmax = min(min(tb.x, tb.y), tb.z);

	return tmax >= max(tmin, 0.0);
}

bool RayTrace(vec3 dir, vec3 start, out vec4 lighting)
{
	float closestT = 1e20;
	bool hit = false;
	vec3 normal;
	vec2 uv;
	vec3 hitPos;
	
	for (int i = 0; i < triCount; i++)
	{
		ivec4 triangle = GetIndex(i);

		vec3 a = vec3(model.modelMat[triangle.w] * vertexBuf.vert[triangle.x].pos);
		vec3 b = vec3(model.modelMat[triangle.w] * vertexBuf.vert[triangle.y].pos);
		vec3 c = vec3(model.modelMat[triangle.w] * vertexBuf.vert[triangle.z].pos);

		vec3 triMin = min(a, min(b, c));
		vec3 triMax = max(a, max(b, c));

		if (!RayAabbIntersect(start, dir, triMin, triMax))
			continue;

		float t;
		vec2 baryUV;
		if (RayTriIntersect(start, dir, i, a, b, c, t, baryUV) && t < closestT)
		{
			closestT = t;
			hit = true;

			hitPos = start + dir * t;
			normal = normalize(cross(b - a, c - a));

			vec2 uvA = vertexBuf.vert[triangle.x].uv.xy;
			vec2 uvB = vertexBuf.vert[triangle.y].uv.xy;
			vec2 uvC = vertexBuf.vert[triangle.z].uv.xy;
			uv = (1.0 - baryUV.x - baryUV.y) * uvA + baryUV.x * uvB + baryUV.y * uvC;
		}
	}
	
	if (!hit)
		return false;
	
	bool inShadow = false;
	vec3 lightDir = lightPos - hitPos;
	vec3 shadowOrigin = hitPos + normal * 0.001;
	float lightDist = length(lightDir);
	lightDir = normalize(lightDir);

	for (int g = 0; g < totalTriCount; g++)
	{
		ivec4 triangleS = GetIndexGlobal(g);

		vec3 aS = vec3(model.modelMat[triangleS.w] * vertexBuf.vert[triangleS.x].pos);
		vec3 bS = vec3(model.modelMat[triangleS.w] * vertexBuf.vert[triangleS.y].pos);
		vec3 cS = vec3(model.modelMat[triangleS.w] * vertexBuf.vert[triangleS.z].pos);

		vec3 triMinS = min(aS, min(bS, cS));
		vec3 triMaxS = max(aS, max(bS, cS));

		if (!RayAabbIntersect(shadowOrigin, lightDir, triMinS, triMaxS))
			continue;

		float tShadow;
		vec2 shadowBary;
		if (RayTriIntersect(shadowOrigin, lightDir, g, aS, bS, cS, tShadow, shadowBary))
		{
			if (tShadow < lightDist)
			{
				inShadow = true;
				break;
			}
		}
	}
	
	if (inShadow)
		lighting = vec4(0.1, 0.1, 0.1, 1.0);
	else
		lighting = vec4(0.5, 0.5, 0.5, 1.0);

	return true;
}

void main()
{
	vec2 fragPos = (gl_FragCoord.xy / vec2(1920.0, 1080.0)) * 2.0 - 1.0;
	vec4 target = inverse(uProj) * vec4(fragPos, 1.0, 1.0);
	vec3 rayDir = vec3(inverse(uView) * vec4(normalize(vec3(target) / target.w), 0.0));

	vec4 lighting;
	if(RayTrace(rayDir, ucamPos, lighting))
	{
		out_color = lighting;
	}
	else 
		discard;
}