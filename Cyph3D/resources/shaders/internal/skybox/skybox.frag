in vec3 TexCoords;

layout(bindless_sampler) uniform samplerCube skybox;

layout(location = 2) out vec3 color;

void main()
{
	color = texture(skybox, TexCoords).xyz;
}