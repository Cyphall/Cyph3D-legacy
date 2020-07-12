#version 460 core

in vec3 frag_Normal;
in vec3 frag_viewDir;

uniform vec3 color;

layout (location = 0) out vec4 Out_Color;

void main()
{
	Out_Color = vec4(color * (dot(frag_Normal, frag_viewDir) + 1 * 0.5), 1);
}