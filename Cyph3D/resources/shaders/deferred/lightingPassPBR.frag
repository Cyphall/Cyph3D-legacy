#version 460 core

/* ------ consts ------ */
const float PI = 3.14159265359;

/* ------ inputs from vertex shader ------ */
in FRAG {
	vec2  TexCoords;
	vec3  ViewPos;
} frag;

/* ------ data structures ------ */
struct Light
{
	vec3  pos;
	float intensity;
	vec3  color;
};

/* ------ uniforms ------ */
layout(std430, binding = 0) buffer Lights
{
	Light lights[];
};

uniform sampler2D positionTexture;
uniform sampler2D normalTexture;
uniform sampler2D colorTexture;
uniform sampler2D materialTexture;
uniform sampler2D depthTexture;

uniform int debug;

/* ------ outputs ------ */
out vec4 out_Color;

/* ------ function declarations ------ */
vec4 debugView();
vec4 lighting();

vec3 getPosition();
vec3 getColor();
vec3 getNormal();
float getRoughness();
float getMetallic();
float getEmissive();
float getDepth();
int isLit();

vec3 saturate(vec3 color);
vec3 toSRGB(vec3 linear);
vec4 reinhard_tone_mapping(vec3 hdrColor);

float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlick(float cosTheta, vec3 F0);

/* ------ code ------ */
void main()
{
	if (debug == 1)
	{
		out_Color = debugView();
	}
	else if (getDepth() == 1)
	{
		discard;
	}
	else if (isLit() == 0)
	{
		out_Color = reinhard_tone_mapping(getColor());
	}
	else
	{
		out_Color = lighting();
	}
}

vec4 debugView()
{
	vec2 texCoords = frag.TexCoords;
	if (texCoords.x >= 0.5 && texCoords.y >= 0.5)
	{
		texCoords.x = (texCoords.x - 0.5) * 2;
		texCoords.y = (texCoords.y - 0.5) * 2;
		return texture(positionTexture, texCoords);
	}
	else if (texCoords.x >= 0.5 && texCoords.y < 0.5)
	{
		texCoords.x = (texCoords.x - 0.5) * 2;
		texCoords.y = texCoords.y * 2;
		return texture(normalTexture, texCoords);
	}
	else if (texCoords.x < 0.5 && texCoords.y >= 0.5)
	{
		texCoords.x = texCoords.x * 2;
		texCoords.y = (texCoords.y - 0.5) * 2;
		return reinhard_tone_mapping(texture(colorTexture, texCoords).rgb);
	}
	else
	{
		texCoords.x = texCoords.x * 2;
		texCoords.y = texCoords.y * 2;
		return texture(materialTexture, texCoords);
	}
}

vec4 lighting()
{
	// setup
	vec3  fragPos           = getPosition();
	vec3  viewDir           = normalize(frag.ViewPos - fragPos);
	vec3  normal            = getNormal();
	float roughness         = getRoughness();
	float metalness         = getMetallic();
	vec3  color             = getColor();
	float emissiveIntensity = getEmissive();

	// Modified version of the code at learnopengl.com/PBR/Lighting
	vec3 F0 = vec3(0.04);
	F0 = mix(F0, color, metalness);

	// reflectance equation
	vec3 Lo = vec3(0.0);
	for(int i = 0; i < lights.length(); ++i)
	{
		// calculate per-light radiance
		vec3 L = normalize(lights[i].pos - fragPos);
		vec3 H = normalize(viewDir + L);
		float distance    = length(lights[i].pos - fragPos);
		float attenuation = 1.0 / (1 + distance * distance);
		vec3 radiance     = lights[i].color * lights[i].intensity * attenuation;

		// cook-torrance brdf
		float NDF = DistributionGGX(normal, H, roughness);
		float G   = GeometrySmith(normal, viewDir, L, roughness);
		vec3 F    = fresnelSchlick(max(dot(H, viewDir), 0.0), F0);

		vec3 kS = F;
		vec3 kD = vec3(1.0) - kS;
		kD *= 1.0 - metalness;

		vec3 numerator    = NDF * G * F;
		float denominator = 4.0 * max(dot(normal, viewDir), 0.0) * max(dot(normal, L), 0.0);
		vec3 specular     = numerator / max(denominator, 0.001);

		// add to outgoing radiance Lo
		float NdotL = max(dot(normal, L), 0.0);
		Lo += (kD * color / PI + specular) * radiance * NdotL;
	}

	return reinhard_tone_mapping(Lo + saturate(color) * emissiveIntensity);
}

vec3 getPosition()
{
	return texture(positionTexture, frag.TexCoords).rgb;
}

vec3 getColor()
{
	return texture(colorTexture, frag.TexCoords).rgb;
}

vec3 getNormal()
{
	return normalize(texture(normalTexture, frag.TexCoords).rgb * 2.0 - 1.0);
}

float getRoughness()
{
	return texture(materialTexture, frag.TexCoords).r;
}

float getMetallic()
{
	return texture(materialTexture, frag.TexCoords).g;
}

float getEmissive()
{
	return texture(materialTexture, frag.TexCoords).b;
}

float getDepth()
{
	return texture(depthTexture, frag.TexCoords).r;
}

int isLit()
{
	return int(texture(materialTexture, frag.TexCoords).a);
}

vec3 saturate(vec3 color)
{
	float highest = max(max(color.r, color.g), color.b);
	return color / highest;
}

vec3 toSRGB(vec3 linear)
{
	bvec3 cutoff = lessThan(linear, vec3(0.0031308));
	vec3 higher = vec3(1.055) * pow(linear, vec3(1.0/2.4)) - vec3(0.055);
	vec3 lower = linear * vec3(12.92);

	return mix(higher, lower, cutoff);
}

vec4 reinhard_tone_mapping(vec3 color)
{
	float exposure = 2;
	color *= exposure/(1. + color / exposure);

	// sRGB correction
	color = toSRGB(color);

	return vec4(color, 1);
}

// Normal Distribution Function
float DistributionGGX(vec3 N, vec3 H, float roughness)
{
	float a      = roughness*roughness;
	float a2     = a*a;
	float NdotH  = max(dot(N, H), 0.0);
	float NdotH2 = NdotH*NdotH;

	float num   = a2;
	float denom = (NdotH2 * (a2 - 1.0) + 1.0);
	denom = PI * denom * denom;

	return num / denom;
}

// Geometry Shadowing Function
float GeometrySchlickGGX(float NdotV, float roughness)
{
	float r = (roughness + 1.0);
	float k = (r*r) / 8.0;

	float num   = NdotV;
	float denom = NdotV * (1.0 - k) + k;

	return num / denom;
}
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
	float NdotV = max(dot(N, V), 0.0);
	float NdotL = max(dot(N, L), 0.0);
	float ggx2  = GeometrySchlickGGX(NdotV, roughness);
	float ggx1  = GeometrySchlickGGX(NdotL, roughness);

	return ggx1 * ggx2;
}

// Fresnel Function
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
	return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}