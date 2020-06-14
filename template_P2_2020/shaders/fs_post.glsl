#version 330

in vec2 Texcoords;

out vec4 outputColor;
uniform sampler2D renderedTexture;

void main() {
	outputColor = texture(renderedTexture, Texcoords);
}