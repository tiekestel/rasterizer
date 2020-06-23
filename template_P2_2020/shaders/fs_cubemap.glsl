#version 420

// structs for lights
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
uniform pointlight pointlights[20];
uniform directionalLight directionalLights[20];
uniform spotlight spotlights[20];
uniform int pointlightCount;
uniform int directionalLightCount;
uniform int spotlightCount;
uniform vec4 viewPos;
vec3 normalVec;
vec3 texColor;
vec3 viewDirection;

//functions
void Phong(in vec3 lightDirection, in vec3 normal, in float strength, in vec3 lightColor, inout vec3 color);

// shader output
layout(location = 0) out vec4 outputColor;

// fragment shader
void main()
{
	//camera view direction
	viewDirection = normalize(position - viewPos.xyz);
    texColor = texture( pixels, uv ).xyz;
	
	//set normals
	normalVec = normalize(normal.xyz);

	//ambient color
	vec3 color = texColor * 0.15;

	//directional lights
	for(int i = 0; i < directionalLightCount; ++i) {
		Phong(normalize(directionalLights[i].direction), normalVec, directionalLights[i].strength, directionalLights[i].color, color);
	}

	//pointlights
	for(int i = 0; i < pointlightCount; ++i) {
		vec3 lightDirection = pointlights[i].position - position;
		float strength = pointlights[i].strength / (length(lightDirection) * length(lightDirection));
		lightDirection = normalize(lightDirection);
		Phong(lightDirection, normalVec, strength, pointlights[i].color, color);
	}

	//spotlights
	for(int i = 0; i < spotlightCount; ++i) {
		vec3 lightDirection = spotlights[i].position - position;
		if(dot(normalize(lightDirection), -spotlights[i].direction) > spotlights[i].angle) {
			Phong(normalize(lightDirection), normalVec, spotlights[i].strength / (length(lightDirection) * length(lightDirection)), spotlights[i].color, color); 
		}
	}

	//write to output
	outputColor = vec4(color, 1);
	
} 

void Phong(in vec3 lightDirection, in vec3 normal, in float strength, in vec3 lightColor, inout vec3 color) {
	float diffuse = max(dot(normalVec, lightDirection), 0);
	vec3 reflectDirection = reflect(-lightDirection, normalVec);
	float specular = pow(max(dot(viewDirection, reflectDirection), 0), 32);
	color += (texColor * (diffuse + specular) * lightColor) * strength;
}

