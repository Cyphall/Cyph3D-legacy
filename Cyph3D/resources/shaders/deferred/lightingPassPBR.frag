#version 460 core

/* ------ consts ------ */
const float PI = 3.14159265359;

/* ------ inputs from vertex shader ------ */
in VERT2FRAG {
	vec2  TexCoords;
	vec3  ViewPos;
} vert2frag;

/* ------ data structures ------ */
struct Light
{
	vec3  pos;
	float intensity;
	vec3  color;
};

struct FragData
{
	vec3  pos;
	vec3  viewDir;
	vec3  normal;
	float roughness;
	float metalness;
	vec3  color;
	float emissiveIntensity;
	vec3  F0;
} fragData;

/* ------ uniforms ------ */
layout(std430, binding = 0) buffer UselessNameBecauseItIsNeverUsedAnywhere1
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

vec3 calculateLighting(vec3 radiance, vec3 L, vec3 H);

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
	vec2 texCoords = vert2frag.TexCoords;
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

// Based on the code at https://learnopengl.com/PBR/Lighting by Joey de Vries (https://twitter.com/JoeyDeVriez)
vec4 lighting()
{
	// Fragment parameters initialization
	fragData.pos               = getPosition();
	fragData.viewDir           = normalize(vert2frag.ViewPos - fragData.pos);
	fragData.normal            = getNormal();
	fragData.roughness         = getRoughness();
	fragData.metalness         = getMetallic();
	fragData.color             = getColor();
	fragData.emissiveIntensity = getEmissive();
	fragData.F0                = mix(vec3(0.04), fragData.color, fragData.metalness);
	
	// reflectance equation
	vec3 Lo = saturate(fragData.color) * fragData.emissiveIntensity;
	for(int i = 0; i < lights.length(); ++i)
	{
		// calculate light parameters
		vec3  lightDir    = normalize(lights[i].pos - fragData.pos);
		vec3  halfwayDir  = normalize(fragData.viewDir + lightDir);
		float distance    = length(lights[i].pos - fragData.pos);
		float attenuation = 1.0 / (1 + distance * distance);
		vec3  radiance    = lights[i].color * lights[i].intensity * attenuation;
		
		Lo += calculateLighting(radiance, lightDir, halfwayDir);
	}

	return reinhard_tone_mapping(Lo);
}

vec3 calculateLighting(vec3 radiance, vec3 L, vec3 H)
{
	// cook-torrance brdf
	float NDF = DistributionGGX(fragData.normal, H, fragData.roughness);
	float G   = GeometrySmith(fragData.normal, fragData.viewDir, L, fragData.roughness);
	vec3 F    = fresnelSchlick(max(dot(H, fragData.viewDir), 0.0), fragData.F0);

	vec3 kS = F;
	vec3 kD = vec3(1.0) - kS;
	kD *= 1.0 - fragData.metalness;

	vec3  numerator    = NDF * G * F;
	float denominator = 4.0 * max(dot(fragData.normal, fragData.viewDir), 0.0) * max(dot(fragData.normal, L), 0.0);
	vec3  specular     = numerator / max(denominator, 0.001);

	// add to outgoing radiance Lo
	float NdotL = max(dot(fragData.normal, L), 0.0);
	return (kD * fragData.color / PI + specular) * radiance * NdotL;
}

vec3 getPosition()
{
	return texture(positionTexture, vert2frag.TexCoords).rgb;
}

vec3 getColor()
{
	return texture(colorTexture, vert2frag.TexCoords).rgb;
}

vec3 getNormal()
{
	return normalize(texture(normalTexture, vert2frag.TexCoords).rgb * 2.0 - 1.0);
}

float getRoughness()
{
	return texture(materialTexture, vert2frag.TexCoords).r;
}

float getMetallic()
{
	return texture(materialTexture, vert2frag.TexCoords).g;
}

float getEmissive()
{
	return texture(materialTexture, vert2frag.TexCoords).b;
}

float getDepth()
{
	return texture(depthTexture, vert2frag.TexCoords).r;
}

int isLit()
{
	return int(texture(materialTexture, vert2frag.TexCoords).a);
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