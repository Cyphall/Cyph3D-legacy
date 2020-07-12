#version 460 core

layout(location = 0) in vec3 in_Vertex;
layout(location = 2) in vec3 in_Normal;

uniform mat4 mvp;
uniform mat4 model;
uniform vec3 viewDir;

out vec3 frag_Normal;
out vec3 frag_viewDir;

void main()
{
	gl_Position = mvp * vec4(in_Vertex, 1);
	frag_Normal = in_Normal;

	frag_viewDir = (inverse(model) * vec4(viewDir, 1)).xyz;
}