#version 420

in vec2 Texcoords;

out vec4 outputColor;
layout (binding = 1) uniform sampler2D renderedTexture;
layout (binding = 2) uniform sampler2D hdrTexture;

void main() {
	vec2 distVector = Texcoords - vec2(0.5);
	//float vignet = distance(distVector, vec2(0));
	vec3 color = texture(renderedTexture, Texcoords).rgb;
	vec3 hdrColor = texture(hdrTexture, Texcoords).rgb;
	color += hdrColor;
	color = vec3(1) - exp(-color * 1);
	color = pow(color, vec3(1 / 1.2));
	//color -= vec3(vignet * vignet);
	outputColor = vec4(color, 1);
}