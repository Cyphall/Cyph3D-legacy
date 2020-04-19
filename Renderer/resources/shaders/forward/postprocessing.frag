#version 460 core

in vec2 texCoords;

uniform sampler2D screenTexture;
uniform vec2 pixelSize;

out vec4 out_Color;

vec4 reinhard_tone_mapping(vec3 hdrColor);
vec3 toSRGB(vec3 linear);
vec3 toLinear(vec3 sRGB);

void main()
{
	vec3 color = texture(screenTexture, texCoords).rgb;

	// tone mapping & gamma corection
	out_Color = reinhard_tone_mapping(color);
//	out_Color = vec4(toSRGB(color), 1);
}

vec4 reinhard_tone_mapping(vec3 color)
{
	color = color / (color + 1);
	
	// sRGB correction
	color = toSRGB(color);
	
	return vec4(color, 1);
}

vec3 chromatic_aberration(float strengh)
{
	vec2 r_pos = texCoords + (strengh * pixelSize);
	vec2 g_pos = texCoords + (strengh * pixelSize * 2);
	vec2 b_pos = texCoords + (strengh * pixelSize * 3);

	return vec3(
	texture(screenTexture, r_pos).r,
	texture(screenTexture, g_pos).g,
	texture(screenTexture, b_pos).b
	);
}

vec3 toSRGB(vec3 linear)
{
	bvec3 cutoff = lessThan(linear, vec3(0.0031308));
	vec3 higher = vec3(1.055) * pow(linear, vec3(1.0/2.4)) - vec3(0.055);
	vec3 lower = linear * vec3(12.92);

	return mix(higher, lower, cutoff);
}