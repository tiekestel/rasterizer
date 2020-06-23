#version 330

in vec2 Texcoords;

out vec4 outputColor;
uniform sampler2D renderedTexture;

uniform bool horizontal;
uniform float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

void main() {
	//try box filter for hdr
	vec2 texel = 1.0 / textureSize(renderedTexture, 0);
	vec3 color = texture(renderedTexture, Texcoords).rgb * weight[0];
	
	if(horizontal){
		for(int i = 1; i < 5; ++i) {
			color += (texture(renderedTexture, Texcoords + vec2(texel.x * i, 0)).rgb - 1) * weight[i];
			color += (texture(renderedTexture, Texcoords - vec2(texel.x * i, 0)).rgb - 1) * weight[i];
		}
	}
	else{
		for(int i = 1; i < 5; ++i){
			color += (texture(renderedTexture, Texcoords + vec2(0, texel.y * i)).rgb - 1) * weight[i];
			color += (texture(renderedTexture, Texcoords - vec2(0, texel.y * i)).rgb - 1) * weight[i];
		}
	}

	outputColor = vec4(color, 1);
}