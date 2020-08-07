in FRAG {
	vec2 TexCoords;
	mat3 TangentToWorld;
	mat3 WorldToTangent;
	vec3 FragPos;
} frag;

uniform vec3 viewPos;

layout(bindless_sampler) uniform sampler2D colorMap;
layout(bindless_sampler) uniform sampler2D normalMap;
layout(bindless_sampler) uniform sampler2D roughnessMap;
layout(bindless_sampler) uniform sampler2D displacementMap;
layout(bindless_sampler) uniform sampler2D metallicMap;
layout(bindless_sampler) uniform sampler2D emissiveMap;

uniform int isLit;

layout(location = 0) out vec3 position;
layout(location = 1) out vec3 normal;
layout(location = 2) out vec3 color;
layout(location = 3) out vec4 material;
layout(location = 4) out vec3 geometryNormal;

float getDepth(vec2 texCoords);
vec2 POM(vec2 texCoords, vec3 viewDir);

void main()
{
	vec2 texCoords = POM(frag.TexCoords, normalize(frag.WorldToTangent * (viewPos - frag.FragPos)));

	color = texture(colorMap, texCoords).rgb;

	position = frag.FragPos;

	normal = normalize(texture(normalMap, texCoords).rgb * 2.0 - 1.0);
	normal = frag.TangentToWorld * normal;
	normal = (normal + 1) * 0.5;

	material.r = texture(roughnessMap, texCoords).r;
	material.g = texture(metallicMap, texCoords).r;
	material.b = texture(emissiveMap, texCoords).r;
	material.a = isLit;
	
	geometryNormal = frag.TangentToWorld * vec3(0, 0, 1);
	geometryNormal = (geometryNormal + 1) * 0.5;
}

float getDepth(vec2 texCoords)
{
	return 1 - texture(displacementMap, texCoords).r;
}

vec2 POM(vec2 texCoords, vec3 viewDir)
{
	const float depthScale           = 0.05;
	const int   layerCount           = 8;
	const int   resamplingLoopCount  = 6;

	// Initial sampling pass
	vec2 currentTexCoords = texCoords;

	float currentTexDepth  = getDepth(currentTexCoords);
	float previousTexDepth;

	if (currentTexDepth == 0 || layerCount == 0) return texCoords;

	if (viewDir.z <= 0) return texCoords;

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
		texCoordsStepOffset *= 0.5;
		depthStepOffset *= 0.5;

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
	float beforeDepth = previousTexDepth - currentDepth + depthStepOffset;

	float weight = afterDepth / (afterDepth - beforeDepth);
	texCoords = previousTexCoords * weight + currentTexCoords * (1.0 - weight);

	return texCoords;
}