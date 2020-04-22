#version 460 core

const float PI = 3.14159265359;
const float HALF_PI = 1.57079632679;

in FRAG {
	vec2  TexCoords;
	vec3  TangentLightPos;
	vec3  LightColor;
	float LightIntensity;
	vec3  TangentViewPos;
	vec3  TangentFragPos;
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

void main()
{
	// setup
	vec3  lightDir       = normalize(frag.TangentLightPos - frag.TangentFragPos);
	vec3  viewDir        = normalize(frag.TangentViewPos - frag.TangentFragPos);
	vec3  halfwayDir     = normalize(lightDir + viewDir);
	vec2  texCoords      = POM(frag.TexCoords, viewDir);
	vec3  normal         = getNormal(texCoords);
	float roughness      = getRoughness(texCoords);
	float metalness      = getMetallic(texCoords);
	vec3  color          = getColor(texCoords);
	vec3  lightColor     = frag.LightColor;
	float lightIntensity = frag.LightIntensity;
	

	// Diffuse calculation
	float diffuseIntensity = max(dot(normal, lightDir), 0);
	
	vec3 diffuseMetallic = vec3(0);
	vec3 diffuseNonMetallic = min(color, lightColor) * lightIntensity * diffuseIntensity;
	
	// Specular calculation
	float specularIntensity = DistributionGGX(normal, halfwayDir, roughness);

	vec3 specularMetallic =  color * lightColor * lightIntensity * specularIntensity;
	vec3 specularNonMetallic = lightColor * lightIntensity * specularIntensity * pow((1 - roughness) / 2 + 0.1, 4);
	
	
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
	const float depthScale           = 0.04;
	const int   layerCount           = 16;
	const int   resamplingLoopCount  = 8;

	// Initial sampling pass
	vec2 currentTexCoords = texCoords;

	float currentTexDepth  = getDepth(currentTexCoords);
	float previousTexDepth;

	if (currentTexDepth == 0 || layerCount == 0) return texCoords;

	// Offsets applied at each steps
	vec2  texCoordsStepOffset = -(viewDir.xy / viewDir.z) / layerCount * depthScale;
	float depthStepOffset     = 1.0 / layerCount;

	float currentDepth = 0;

	while (currentDepth < currentTexDepth)
	{
		currentTexCoords += texCoordsStepOffset;

		previousTexDepth = currentTexDepth;
		currentTexDepth = getDepth(currentTexCoords);

		currentDepth += depthStepOffset;
	}

	vec2 previousTexCoords = currentTexCoords - texCoordsStepOffset;
	float previousDepth = currentDepth - depthStepOffset;
	
	// Resampling pass
	
	for (int i = 0; i < resamplingLoopCount; i++)
	{
		texCoordsStepOffset /= 2;
		depthStepOffset /= 2;

		vec2  halfwayTexCoords = previousTexCoords + texCoordsStepOffset;
		float halfwayTexDepth  = getDepth(halfwayTexCoords);
		float halfwayDepth     = previousDepth + depthStepOffset;
		
		// If we are still above the surface
		if (halfwayDepth < halfwayTexDepth)
		{
			previousTexCoords = halfwayTexCoords;
			previousTexDepth  = halfwayTexDepth;
			previousDepth     = halfwayDepth;
		}
		else
		{
			currentTexCoords = halfwayTexCoords;
			currentTexDepth  = halfwayTexDepth;
			currentDepth     = halfwayDepth;
		}
	}

	// Interpolation
	float afterDepth  = currentTexDepth - currentDepth;
	float beforeDepth = getDepth(previousTexCoords) - currentDepth + depthStepOffset;

	float weight = afterDepth / (afterDepth - beforeDepth);
	texCoords = previousTexCoords * weight + currentTexCoords * (1.0 - weight);

	return texCoords;
}