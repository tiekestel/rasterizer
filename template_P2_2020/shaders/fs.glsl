#version 420

struct pointlight{
	vec3 position;
	float strength;
	vec3 color;
	samplerCube shadowMap;
};

struct directionalLight{
	vec3 direction;
	float strength;
	vec3 color;
	mat4 lightSpace;
	sampler2D shadowMap;
};
 
struct spotlight {
	vec3 direction;
	vec3 position;
	float strength;
	vec3 color;
	float angle;
	mat4 lightSpace;
	sampler2D shadowMap;
};

// shader input
in vec2 uv;											// interpolated texture coordinates
in vec4 normal;										// interpolated normal
in vec3 position;
in mat3 TBN;
layout (binding = 0) uniform sampler2D pixels;		// texture sampler
layout (binding = 1) uniform sampler2D normalMap;
layout (binding = 2) uniform samplerCube cubeMap;
uniform pointlight pointlights[20];
uniform directionalLight directionalLights[20];
uniform spotlight spotlights[40];
uniform int pointlightCount;
uniform int directionalLightCount;
uniform int spotlightCount;
uniform vec4 viewPos;
uniform bool isNormalMap;
uniform int cubeMapType;
uniform float intensity;
vec3 normalVec;
vec3 texColor;
vec3 viewDirection;

void Phong(in vec3 lightDirection, in vec3 normal, in float strength, in vec3 lightColor, inout vec3 color);
bool Shadow(vec4 positionLightspace, sampler2D shadowMap, vec3 lightDirection, float zFar);
bool Shadow(vec3 lightPosition, samplerCube shadowMap);
// shader output
layout(location = 0) out vec4 outputColor;
layout(location = 1) out vec4 brightColor;

// fragment shader
void main()
{
	viewDirection = normalize(position -vec3(-viewPos.x, viewPos.y, -viewPos.z));
	viewDirection.y *= -1;
    texColor = texture( pixels, uv ).xyz * pow(2, intensity);

	if(isNormalMap){
		normalVec = 2 * texture( normalMap, uv ).xyz - 1;
		normalVec = TBN * normalVec;
	}
	else{
		normalVec = normalize(normal.xyz);
	}
	vec3 color = texColor * 0.15;

	for(int i = 0; i < directionalLightCount; ++i) {
		if(Shadow(directionalLights[i].lightSpace * vec4(position,1),directionalLights[i].shadowMap, directionalLights[i].direction, 1000)){
			Phong(normalize(directionalLights[i].direction), normalVec, directionalLights[i].strength, directionalLights[i].color, color);
		}
	}

	for(int i = 0; i < pointlightCount; ++i) {
		if(Shadow(pointlights[i].position, pointlights[i].shadowMap)){
			vec3 lightDirection = pointlights[i].position - position;
			float strength = pointlights[i].strength / (length(lightDirection) * length(lightDirection));
			lightDirection = normalize(lightDirection);
			Phong(lightDirection, normalVec, strength, pointlights[i].color, color);
		}
//		vec3 lightDirection = pointlights[i].position - position;
//		float strength = pointlights[i].strength / (length(lightDirection) * length(lightDirection));
//		lightDirection = normalize(lightDirection);
//		Phong(lightDirection, normalVec, strength, pointlights[i].color, color);
	}

	for(int i = 0; i < spotlightCount; ++i) {
		vec3 lightDirection = spotlights[i].position - position;
		if(dot(normalize(lightDirection), -spotlights[i].direction) > spotlights[i].angle) {
//			if(Shadow(spotlights[i].lightSpace * vec4(position,1),spotlights[i].shadowMap, -spotlights[i].direction, 1000)){
//				Phong(normalize(lightDirection), normalVec, spotlights[i].strength / (length(lightDirection) * length(lightDirection)), spotlights[i].color, color);
//			}
Phong(normalize(lightDirection), normalVec, spotlights[i].strength / (length(lightDirection) * length(lightDirection)), spotlights[i].color, color);
		}
	}

	if(cubeMapType == 1){
		vec3 ray = reflect(viewDirection, normalVec);
		color *= 0.5;
		color += texture(cubeMap, ray).xyz * 0.5;
	}
	else if(cubeMapType == 2){

	}

	outputColor = vec4(color, 1);

	
	float brightness = dot(color, vec3(0.2126, 0.7152, 0.0722));
	if(brightness > 1)
		brightColor = outputColor;
	else
		brightColor = vec4(vec3(0),1);
} 

void Phong(in vec3 lightDirection, in vec3 normal, in float strength, in vec3 lightColor, inout vec3 color) {
	float diffuse = max(dot(normalVec, lightDirection), 0);
	vec3 reflectDirection = reflect(viewDirection, normalVec);
	float specular = 0;
	if(dot(lightDirection, reflectDirection) > 0){
		specular = pow(max(dot(lightDirection, reflectDirection), 0), 32);
	}
	color += (texColor * (diffuse + specular) * lightColor) * strength;
}

bool Shadow(vec4 positionLightspace, sampler2D shadowMap, vec3 lightDirection, float zFar){
	vec3 projCoord = positionLightspace.xyz / positionLightspace.w;
	projCoord = projCoord *0.5 + 0.5;
	float mapdepth = texture(shadowMap,projCoord.xy).r;
	float depth = positionLightspace.z / zFar;
	float bias = max(0.05 * 1.0 - dot(normalVec, lightDirection),0.001);
	return depth - bias <= mapdepth;
}

bool Shadow(vec3 lightPosition, samplerCube shadowMap){
	vec3 fragToLight = position - lightPosition;
	float mapdepth = texture(shadowMap, fragToLight).r;
	float depth = length(fragToLight) / 500;
	float bias = 0.05;
	return depth <= mapdepth;
}

