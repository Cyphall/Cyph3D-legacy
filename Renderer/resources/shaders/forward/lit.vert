#version 460 core

layout(location = 0) in vec3 in_Vertex;
layout(location = 1) in vec2 in_UV;
layout(location = 2) in vec3 in_Normals;
layout(location = 3) in vec3 in_tangents;
layout(location = 4) in vec3 in_bitangents;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec3  lightPos;
uniform vec3  lightColor;
uniform float lightIntensity;

uniform vec3  viewPos;

out FRAG {
	vec2  TexCoords;
	vec3  TangentLightPos;
	vec3  LightColor;
	float LightIntensity;
	vec3  TangentViewPos;
	vec3  TangentFragPos;
} frag;

void main()
{
	frag.TexCoords = in_UV;

	mat3 normalMatrix = transpose(inverse(mat3(model)));
	vec3 T = normalize(normalMatrix * in_tangents);
	vec3 N = normalize(normalMatrix * in_Normals);
	T = normalize(T - dot(T, N) * N);
	vec3 B = cross(N, T);

	mat3 TBN = transpose(mat3(T, B, N));
	frag.TangentLightPos = TBN * lightPos;
	frag.TangentViewPos  = TBN * viewPos;
	frag.TangentFragPos  = TBN * vec3(model * vec4(in_Vertex, 1.0));
	
	frag.LightColor = lightColor;
	frag.LightIntensity = lightIntensity;

	gl_Position = projection * view * model * vec4(in_Vertex, 1.0);
}