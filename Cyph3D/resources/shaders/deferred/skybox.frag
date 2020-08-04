#version 330 core
#extension GL_ARB_bindless_texture : enable

in vec3 TexCoords;

layout(bindless_sampler) uniform samplerCube skybox;

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