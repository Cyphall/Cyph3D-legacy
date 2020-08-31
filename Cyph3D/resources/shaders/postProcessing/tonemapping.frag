vec3 toSRGB(vec3 linear);
vec3 reinhard_tone_mapping(vec3 color);

layout(bindless_sampler) uniform sampler2D colorTexture;

in vec2 TexCoords;

out vec4 color;

void main()
{
	vec4 rawColor = texture(colorTexture, TexCoords);

	color = vec4(reinhard_tone_mapping(rawColor.rgb), rawColor.a);
}

vec3 toSRGB(vec3 linear)
{
	bvec3 cutoff = lessThan(linear, vec3(0.0031308));
	vec3 higher = vec3(1.055) * pow(linear, vec3(1.0/2.4)) - vec3(0.055);
	vec3 lower = linear * vec3(12.92);

	return mix(higher, lower, cutoff);
}

vec3 reinhard_tone_mapping(vec3 color)
{
	float exposure = 2;
	color *= exposure/(1. + color / exposure);

	// sRGB correction
	color = toSRGB(color);

	return color;
}