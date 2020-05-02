#version 460 core

/* ------ consts ------ */
const float PI = 3.14159265359;
const float HALF_PI = 1.57079632679;

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

uniform int debug;
uniform int numLights;

/* ------ outputs ------ */
out vec4 out_Color;

/* ------ function declarations ------ */
vec3 getPosition();
vec3 getColor();
vec3 getNormal();
float getRoughness();
float getMetallic();
float getEmissive();
int isLit();
vec3 saturate(vec3 color);

float DistributionGGX(vec3 N, vec3 H, float roughness);
vec3 toSRGB(vec3 linear);
vec4 reinhard_tone_mapping(vec3 hdrColor);

/* ------ code ------ */
void main()
{
	if (debug == 1)
	{
		vec2 texCoords = frag.TexCoords;
		if (texCoords.x >= 0.5 && texCoords.y >= 0.5)
		{
			texCoords.x = (texCoords.x - 0.5) * 2;
			texCoords.y = (texCoords.y - 0.5) * 2;
			out_Color = texture(positionTexture, texCoords);
			return;
		}
		else if (texCoords.x >= 0.5 && texCoords.y < 0.5)
		{
			texCoords.x = (texCoords.x - 0.5) * 2;
			texCoords.y = texCoords.y * 2;
			out_Color = texture(normalTexture, texCoords);
			return;
		}
		else if (texCoords.x < 0.5 && texCoords.y >= 0.5)
		{
			texCoords.x = texCoords.x * 2;
			texCoords.y = (texCoords.y - 0.5) * 2;
			out_Color = reinhard_tone_mapping(texture(colorTexture, texCoords).rgb);
			return;
		}
		else if (texCoords.x < 0.5 && texCoords.y < 0.5)
		{
			texCoords.x = texCoords.x * 2;
			texCoords.y = texCoords.y * 2;
			out_Color = texture(materialTexture, texCoords);
			return;
		}
	}

	if (isLit() == 0)
	{
		out_Color = reinhard_tone_mapping(getColor());
		return;
	}

	// setup
	vec3  fragPos           = getPosition();
	vec3  viewDir           = normalize(frag.ViewPos - fragPos);
	vec3  normal            = getNormal();
	float roughness         = getRoughness();
	float metalness         = getMetallic();
	vec3  color             = getColor();
	float emissiveIntensity = getEmissive();

	vec3 totalDiffuseNonMetallic = saturate(color) * emissiveIntensity;
	vec3 totalSpecularMetallic = vec3(0);
	vec3 totalSpecularNonMetallic = vec3(0);

	for (int i = 0; i < lights.length(); i++)
	{
		vec3  lightDir       = normalize(lights[i].pos - fragPos);
		float distance       = distance(lights[i].pos, fragPos);
		vec3  halfwayDir     = normalize(lightDir + viewDir);
		vec3  lightColor     = lights[i].color;
		float lightIntensity = lights[i].intensity / (1 + distance * distance);

		// Diffuse calculation
		float diffuseIntensity = max(dot(normal, lightDir), 0);

		vec3 diffuseNonMetallic = color * lightColor * lightIntensity * diffuseIntensity;

		// Specular calculation
		float specularIntensity = DistributionGGX(normal, halfwayDir, roughness);

		vec3 specularMetallic =  color * lightColor * lightIntensity * specularIntensity;
		vec3 specularNonMetallic = lightColor * lightIntensity * specularIntensity * pow((1 - roughness) / 2 + 0.1, 4);

		totalDiffuseNonMetallic = max(totalDiffuseNonMetallic, diffuseNonMetallic);

		totalSpecularMetallic += specularMetallic;
		totalSpecularNonMetallic += specularNonMetallic;
	}

	vec3 metallicPart = totalSpecularMetallic;
	vec3 nonMetallicPart = max(totalDiffuseNonMetallic, totalSpecularNonMetallic);

	vec3 HDRColor = metallicPart * metalness + nonMetallicPart * (1 - metalness);

	out_Color = reinhard_tone_mapping(HDRColor);
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

int isLit()
{
	return int(texture(materialTexture, frag.TexCoords).a);
}

vec3 saturate(vec3 color)
{
	float highest = max(max(color.r, color.g), color.b);
	return color / highest;
}

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