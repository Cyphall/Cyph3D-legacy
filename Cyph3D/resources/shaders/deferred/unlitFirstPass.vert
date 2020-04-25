#version 460 core

layout(location = 0) in vec3 in_Vertex;
layout(location = 1) in vec2 in_UV;
layout(location = 2) in vec3 in_Normals;
layout(location = 3) in vec3 in_tangents;
layout(location = 4) in vec3 in_bitangents;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec3 viewPos;

out FRAG {
	vec2 TexCoords;
	mat3 TangentToWorld;
	vec3 ViewPos;
	vec3 FragPos;
} frag;

void main()
{
	frag.TexCoords = in_UV;
	frag.ViewPos = viewPos;
	frag.FragPos = vec3(model * vec4(in_Vertex, 1.0));

	mat3 normalMatrix = transpose(inverse(mat3(model)));
	vec3 T = normalize(normalMatrix * in_tangents);
	vec3 N = normalize(normalMatrix * in_Normals);
	T = normalize(T - dot(T, N) * N);
	vec3 B = cross(N, T);

	frag.TangentToWorld = mat3(T, B, N);

	gl_Position = projection * view * model * vec4(in_Vertex, 1.0);
}