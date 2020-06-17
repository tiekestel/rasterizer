#version 420

in vec2 Texcoords;

out vec4 outputColor;

layout(binding = 1) uniform sampler2D depthmap;

void main() {
	float depthValue = texture(depthmap, Texcoords).r;
	outputColor = vec4(vec3(depthValue),1);
}