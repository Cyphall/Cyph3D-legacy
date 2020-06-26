#version 330 core
in vec3 TexCoords;

uniform samplerCube skybox;

layout(location = 0) out vec3 position;
layout(location = 1) out vec3 normal;
layout(location = 2) out vec3 color;
layout(location = 3) out vec4 material;

void main()
{
	position = vec3(0);
	normal = vec3(0);
	color = texture(skybox, TexCoords).xyz;
	material = vec4(0, 0, 0, 0);
}