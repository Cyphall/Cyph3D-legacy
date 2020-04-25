#version 460 core

in FRAG {
	vec2 TexCoords;
	mat3 TangentToWorld;
	vec3 FragPos;
} frag;

uniform vec3 viewPos;

uniform sampler2D colorMap;
uniform sampler2D normalMap;
uniform sampler2D roughnessMap;
uniform sampler2D displacementMap;
uniform sampler2D metallicMap;

uniform int isLit;

layout(location = 0) out vec3 position;
layout(location = 1) out vec3 normal;
layout(location = 2) out vec3 color;
layout(location = 3) out vec3 material;

float getDepth(vec2 texCoords);
vec2 POM(vec2 texCoords, vec3 viewDir);

void main()
{
	vec2 texCoords = POM(frag.TexCoords, transpose(frag.TangentToWorld) * normalize(viewPos - frag.FragPos));

	color = texture(colorMap, texCoords).rgb;
	
	position = frag.FragPos;

	normal = normalize(texture(normalMap, texCoords).rgb * 2.0 - 1.0);
	normal = frag.TangentToWorld * normal;
	normal = (normal + 1) * 0.5;

	material.r = texture(roughnessMap, texCoords).r;
	material.g = texture(metallicMap, texCoords).r;
	material.b = isLit;
}

float getDepth(vec2 texCoords)
{
	return 1 - texture(displacementMap, texCoords).r;
}

vec2 POM(vec2 texCoords, vec3 viewDir)
{
	const float depthScale           = 0.1;
	const int   layerCount           = 16;
	const int   resamplingLoopCount  = 4;

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