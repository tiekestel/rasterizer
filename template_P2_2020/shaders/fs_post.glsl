#version 330

in vec2 Texcoords;

out vec4 outputColor;
uniform sampler2D renderedTexture;

uniform bool horizontal;
uniform float weight[16] = float[] (0.22, 0.19, 0.16, 0.12, 0.7, 0.3, 0.1, 0.01, 0.001, 0.0001, 0.001, 0.0001, 0.0001, 0.0001, 0.0001, 0.0001);

void main() {
	vec2 texel = 1.0 / textureSize(renderedTexture, 0);
	vec3 color = vec3(0);
	
	for(int i = 0; i < 16; ++i) {
		color += (texture(renderedTexture, Texcoords + vec2(texel.x * i, 0)).rgb - 1) * weight[i];
		color += (texture(renderedTexture, Texcoords - vec2(texel.x * i, 0)).rgb - 1) * weight[i];
		color += (texture(renderedTexture, Texcoords + vec2(0, texel.y * i)).rgb - 1) * weight[i];
		color += (texture(renderedTexture, Texcoords - vec2(0, texel.y * i)).rgb - 1) * weight[i];

		if(i < 16 * 3.1415 * 0.25){
			color += (texture(renderedTexture, Texcoords + vec2(texel.x * i, texel.y * i)).rgb -1) * weight[i];
			color += (texture(renderedTexture, Texcoords - vec2(texel.x * i, texel.y * i)).rgb -1) * weight[i];
			color += (texture(renderedTexture, Texcoords + vec2(-texel.x * i, texel.y * i)).rgb -1) * weight[i];
			color += (texture(renderedTexture, Texcoords - vec2(-texel.x * i, texel.y * i)).rgb -1) * weight[i];
		}
	}

	outputColor = vec4(color, 1);
}