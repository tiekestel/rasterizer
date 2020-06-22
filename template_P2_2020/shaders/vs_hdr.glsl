#version 420

layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoords;

out vec2 Texcoords;

void main() {
	gl_Position = vec4(aPos, 0, 1);
	Texcoords = aTexCoords;
}