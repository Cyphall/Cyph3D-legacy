#version 460 core

in vec2 frag_UV;
flat in vec3 frag_Normals;

uniform sampler2D baseColor;

out vec4 out_Color;

void main()
{
	out_Color = texture(baseColor, frag_UV);
}