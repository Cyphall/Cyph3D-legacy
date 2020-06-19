#version 460 core

layout(location = 0) in vec3 in_Vertex;
layout(location = 1) in vec2 in_UV;
layout(location = 2) in vec3 in_Normals;
layout(location = 3) in vec3 in_tangents;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out FRAG {
	vec2 TexCoords;
	vec3 T;
	vec3 B;
	vec3 N;
	vec3 FragPos;
} frag;

void main()
{
	frag.TexCoords = in_UV;
	frag.FragPos = vec3(model * vec4(in_Vertex, 1.0));

	mat3 normalMatrix = transpose(inverse(mat3(model)));
	frag.T = normalMatrix * in_tangents;
	frag.N = normalMatrix * in_Normals;
	frag.B = cross(frag.N, frag.T);

	gl_Position = projection * view * model * vec4(in_Vertex, 1.0);
}