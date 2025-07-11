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

ivec3 getIndex(int i)
{
	return ivec3(indiceBuf.ind[i]) + vertStart;
}

bool rayTriIntersect(vec3 orig, vec3 dir, int triIndex, vec3 a, vec3 b, vec3 c, out float t, out vec2 uv)
{
	ivec3 triangle = getIndex(triIndex);

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
	uv.x = u;
	uv.y = v;
		return t > 0.0;
}

void main()
{
	vec2 fragPos = (gl_FragCoord.xy / vec2(1920.0, 1080.0)) * 2.0 - 1.0;
	vec4 target = inverse(uProj) * vec4(fragPos, 1.0, 1.0);
	vec3 rayDir = vec3(inverse(uView) * vec4(normalize(vec3(target) / target.w), 0.0));

	float closestT = 1e20;
	vec3 lighting;
	bool hit = false;

	for (int i = 0; i < triCount; i++)
	{
		ivec3 triangle = getIndex(i);
		//skip if facing away from camera
		vec3 a = vec3(model.modelMat[uindex] * vertexBuf.vert[triangle.x].pos);
		vec3 b = vec3(model.modelMat[uindex] * vertexBuf.vert[triangle.y].pos);
		vec3 c = vec3(model.modelMat[uindex] * vertexBuf.vert[triangle.z].pos);
		vec3 normal = normalize(cross(b - a, c - a));

		if(dot(normal, rayDir) > 0.0)
			continue;

		float t;
		vec2 uv;
		if (rayTriIntersect(ucamPos, rayDir, i, a, b, c, t, uv) && t < closestT)
		{
			closestT = t;

			vec2 uvA = vertexBuf.vert[triangle.x].uv.xy;
			vec2 uvB = vertexBuf.vert[triangle.y].uv.xy;
			vec2 uvC = vertexBuf.vert[triangle.z].uv.xy;
			uv = (1.0 - uv.x - uv.y) * uvA + uv.x * uvB + uv.y * uvC;

			vec3 hitPos = ucamPos + rayDir * t;
			vec3 color = texture(diffuseTexture, uv).rgb;
			vec3 lightColor = vec3(1.0);
			float lightPower = 2.0;
			float distance = length(lightPos - hitPos);
			// ambient
			vec3 ambient = 0.15 * lightColor;
			// diffuse
			vec3 lightDir = normalize(lightPos - hitPos);
			float diff = max(dot(lightDir, normal), 0.0);
			vec3 diffuse = diff * lightColor;
			// specular
			vec3 viewDir = normalize(ucamPos - hitPos);
			float spec = 0.0;
			vec3 halfwayDir = normalize(lightDir + viewDir);
			spec = pow(max(dot(normal, halfwayDir), 0.0), 128.0);
			vec3 specular = spec * lightColor;
			lighting = (ambient + (1.0) * (diffuse + specular)) * color;
			hit = true;
		}
	}

	if (hit)
		out_color = vec4(lighting, 1.0);
	else
		discard;
}