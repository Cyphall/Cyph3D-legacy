#version 460 core

in FRAG {
	vec2 TexCoords;
	mat3 TangentToWorld;
	vec3 ViewPos;
	vec3 FragPos;
} frag;

uniform sampler2D colorMap;

out vec3 position;
out vec3 normal;
out vec3 color;
out vec3 material;

void main()
{
	color = texture(colorMap, frag.TexCoord).rgb;

	position = frag.FragPos;
	
	normal = frag.TangentToWorld * vec3(0, 0, 1);
	normal = (normal + 1) * 0.5;
	
	material.b = 0;
}