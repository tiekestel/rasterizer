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
layout (binding = 0) uniform sampler2D pixels;		// texture sampler
layout (binding = 1) uniform sampler2D normalMap;
uniform pointlight pointlights[20];
uniform directionalLight directionalLights[20];
uniform spotlight spotlights[20];
uniform int directionalLightCount;
uniform int pointlightCount;
uniform int spotlightCount;
uniform bool isNormalMap;
vec3 normalVec;
vec3 texColor;

void Phong(in vec3 lightDirection, in float strength, in vec3 color, inout vec3 outColor);

// shader output
layout(location = 0) out vec4 outputColor;

// fragment shader
void main()
{
    //Ambient light
    texColor = texture( pixels, uv ).xyz;
	normalVec = normal.xyz;
	vec3 color = vec3(0);

	if(isNormalMap){
		normalVec *= 2 * texture( normalMap, uv ).xyz - 1;
	}

	//Directional lights
	for(int i = 0; i < directionalLightCount; ++i) {
		Phong(directionalLights[i].direction, directionalLights[i].strength, directionalLights[i].color, color);
	}

	//Pointlights
	for(int i = 0; i < pointlightCount; ++i) {
		vec3 lightDirection = pointlights[i].position - position;
		Phong(normalize(lightDirection), pointlights[i].strength / (12.456 * length(lightDirection) * length(lightDirection)), pointlights[i].color, color);
	}

	//Spotlights
	for(int i = 0; i < spotlightCount; ++i) {
		vec3 lightDirection = spotlights[i].position - position;
		if(dot(normalize(lightDirection), -spotlights[i].direction) > spotlights[i].angle) {
			Phong(normalize(lightDirection), spotlights[i].strength / (12.456 * length(lightDirection) * length(lightDirection)), spotlights[i].color, color);
		}
	}

	outputColor = vec4(color, 1);
} 

void Phong(in vec3 lightDirection, in float strength, in vec3 color, inout vec3 outColor) {
	//Diffuse light
	float angle = max(dot(lightDirection, normalVec), 0);

	//Specular light
	float specular = 0.0;
	if(angle > 0) {
		vec3 reflection = reflect(lightDirection, normalVec);
		vec3 view = normalize(-position);
		float specularAngle = max(dot(reflection, view), 0);
		specular = pow(specularAngle, 40);
	}	
	outColor += (texColor * strength + vec3(angle * strength) + vec3(specular * strength)) * color;	
}
