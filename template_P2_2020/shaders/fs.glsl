#version 420

struct pointlight{
	vec3 position;
	float strength;
	vec3 color;
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
uniform spotlight spotlights[20];
uniform int pointlightCount;
uniform int directionalLightCount;
uniform int spotlightCount;
uniform vec4 viewPos;
uniform bool isNormalMap;
uniform int cubeMapType;
vec3 normalVec;
vec3 texColor;
vec3 viewDirection;

void Phong(in vec3 lightDirection, in vec3 normal, in float strength, in vec3 lightColor, inout vec3 color);
bool Shadow(vec4 positionLightspace, directionalLight d);
// shader output
layout(location = 0) out vec4 outputColor;

// fragment shader
void main()
{
	viewDirection = normalize(position -vec3(-viewPos.x, viewPos.y, -viewPos.z));
	viewDirection.y *= -1;
    texColor = texture( pixels, uv ).xyz;

	if(isNormalMap){
		normalVec = 2 * texture( normalMap, uv ).xyz - 1;
		normalVec = TBN * normalVec;
	}
	else{
		normalVec = normalize(normal.xyz);
	}
	vec3 color = texColor * 0.15;

	for(int i = 0; i < directionalLightCount; ++i) {
		if(Shadow(directionalLights[i].lightSpace * vec4(position,1),directionalLights[i])){
			Phong(normalize(directionalLights[i].direction), normalVec, directionalLights[i].strength, directionalLights[i].color, color);
		}
		else{
			color = vec3(1);
		}
	}

	for(int i = 0; i < pointlightCount; ++i) {
		vec3 lightDirection = pointlights[i].position - position;
		float strength = pointlights[i].strength / (length(lightDirection) * length(lightDirection));
		lightDirection = normalize(lightDirection);
		Phong(lightDirection, normalVec, strength, pointlights[i].color, color);
	}

	for(int i = 0; i < spotlightCount; ++i) {
		vec3 lightDirection = spotlights[i].position - position;
		if(dot(normalize(lightDirection), -spotlights[i].direction) > spotlights[i].angle) {
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
	
//	float value = texture(directionalLights[0].shadowMap, uv).r;
//	outputColor = vec4(vec3(value),1);

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

bool Shadow(vec4 positionLightspace, directionalLight d){
	vec3 projCoord = positionLightspace.xyz / positionLightspace.w;
	projCoord = projCoord *0.5 + 0.5;
	float mapdepth = texture(d.shadowMap,projCoord.xy).r;
	float depth = projCoord.z;
	return depth > mapdepth;
}

