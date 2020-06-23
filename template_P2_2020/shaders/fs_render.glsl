#version 420

in vec2 Texcoords;

out vec4 outputColor;
layout (binding = 1) uniform sampler2D renderedTexture;
layout (binding = 2) uniform sampler2D hdrTexture;

void main() {
	//tone mapping and mixing hdr with normal rendered scene
	vec2 distVector = Texcoords - vec2(0.5);
	vec3 color = texture(renderedTexture, Texcoords).rgb;
	vec3 hdrColor = texture(hdrTexture, Texcoords).rgb;
	if(hdrColor.x < 0) hdrColor.x = 0;
	if(hdrColor.y < 0) hdrColor.y = 0;
	if(hdrColor.z < 0) hdrColor.z = 0;
	color += hdrColor;
	color = vec3(1) - exp(-color * 1);
	color = pow(color, vec3(1 / 1.2));
	outputColor = vec4(color, 1);
}