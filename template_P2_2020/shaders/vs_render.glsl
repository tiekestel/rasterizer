#version 330

layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoords;

out vec2 Texcoords;

void main() {
	// transform vertex using supplied matrix
	gl_Position = vec4(aPos, 0, 1);
	Texcoords = aTexCoords;
}