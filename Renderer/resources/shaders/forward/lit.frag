#version 460 core

const float PI = 3.14159265359;
const float HALF_PI = 1.57079632679;

in FRAG {
	vec2 TexCoords;
	vec3 TangentLightPos;
	vec3 TangentViewPos;
	vec3 TangentFragPos;
} frag;

uniform sampler2D colorMap;
uniform sampler2D normalMap;
uniform sampler2D roughnessMap;
uniform sampler2D displacementMap;
uniform sampler2D metallicMap;

out vec3 out_Color;

vec3 getColor(vec2 texCoords);
vec3 getNormal(vec2 texCoords);
float getRoughness(vec2 texCoords);
float getHeight(vec2 texCoords);
float getDepth(vec2 texCoords);
float getMetallic(vec2 texCoords);

float DistributionGGX(vec3 N, vec3 H, float roughness);
float linearDot(vec3 a, vec3 b);
vec2 POM(vec2 texCoords, vec3 viewDir);
vec3 toLinear(vec3 sRGB);

void main()
{
	// setup
	vec3  lightDir       = normalize(frag.TangentLightPos - frag.TangentFragPos);
	vec3  viewDir        = normalize(frag.TangentViewPos - frag.TangentFragPos);
	vec3  halfwayDir     = normalize(lightDir + viewDir);
	vec2  texCoords      = frag.TexCoords;
	      texCoords      = POM(texCoords, viewDir);
	vec3  normal         = getNormal(texCoords);
	float roughness      = getRoughness(texCoords);
	float metalness      = getMetallic(texCoords);
	vec3  color          = getColor(texCoords);
	vec3  lightColor     = vec3(1, 1, 1);
	

	// Diffuse calculation
	float diffuseIntensity = max(dot(normal, lightDir), 0);
	
	vec3 diffuseMetallic = vec3(0);
	vec3 diffuseNonMetallic = color * diffuseIntensity;
	
	// Specular calculation
	float specularIntensity = DistributionGGX(normal, halfwayDir, roughness);

	vec3 specularMetallic = color * specularIntensity;
	vec3 specularNonMetallic = lightColor * specularIntensity * pow((1 - roughness) / 2 + 0.1, 4);
	
	
	vec3 metallicPart = max(diffuseMetallic, specularMetallic);
	vec3 nonMetallicPart = max(diffuseNonMetallic, specularNonMetallic);

	out_Color = metallicPart * metalness + nonMetallicPart * (1 - metalness);
}

vec3 getColor(vec2 texCoords)
{
	return texture(colorMap, texCoords).rgb;
}

vec3 getNormal(vec2 texCoords)
{
	return normalize(texture(normalMap, texCoords).rgb * 2.0 - 1.0);
}

float getRoughness(vec2 texCoords)
{
	return texture(roughnessMap, texCoords).r;
}

float getHeight(vec2 texCoords)
{
	return texture(displacementMap, texCoords).r;
}

float getDepth(vec2 texCoords)
{
	return 1 - getHeight(texCoords);
}

float getMetallic(vec2 texCoords)
{
	return texture(metallicMap, texCoords).r;
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

float linearDot(vec3 a, vec3 b)
{
	float angle = 1 - acos(dot(a, b)) / PI;
	return angle * 2 - 1;
}

vec2 POM(vec2 texCoords, vec3 viewDir)
{
	const float depthScale           = 0.05;
	const int   layerCount           = 16;
	const int   resamplingLayerCount = 10;

	// First sampling
	vec2 currentTexCoords = texCoords;

	float currentTexDepth  = getDepth(currentTexCoords);
	float previousTexDepth = currentTexDepth;

	if (currentTexDepth == 1) return texCoords;

	vec2  stepTexCoordsOffset = -(viewDir.xy / viewDir.z) / layerCount * depthScale;
	float stepDepthOffset     = 1.0 / layerCount;

	float currentDepth = 0;

	while (currentDepth < currentTexDepth)
	{
		currentTexCoords += stepTexCoordsOffset;

		previousTexDepth = currentTexDepth;
		currentTexDepth = getDepth(currentTexCoords);

		currentDepth += stepDepthOffset;
	}
	
	// Resampling to multiply precision by 10
	currentDepth -= stepDepthOffset;
	currentTexCoords -= stepTexCoordsOffset;
	currentTexDepth = previousTexDepth;

	stepTexCoordsOffset /= resamplingLayerCount;
	stepDepthOffset /= resamplingLayerCount;

	while (currentDepth < currentTexDepth)
	{
		currentTexCoords += stepTexCoordsOffset;

		currentTexDepth = getDepth(currentTexCoords);

		currentDepth += stepDepthOffset;
	}

	// Interpolation
	vec2 prevTexCoords = currentTexCoords - stepTexCoordsOffset;

	float afterDepth  = currentTexDepth - currentDepth;
	float beforeDepth = getDepth(prevTexCoords) - currentDepth + stepDepthOffset;

	float weight = afterDepth / (afterDepth - beforeDepth);
	texCoords = prevTexCoords * weight + currentTexCoords * (1.0 - weight);

	return texCoords;
}

vec3 toLinear(vec3 sRGB)
{
	bvec3 cutoff = lessThan(sRGB, vec3(0.04045));
	vec3 higher = pow((sRGB + vec3(0.055)) / vec3(1.055), vec3(2.4));
	vec3 lower = sRGB / vec3(12.92);

	return mix(higher, lower, cutoff);
}