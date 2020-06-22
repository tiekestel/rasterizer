#version 420 core
in vec2 Texcoords;

out vec4 outputColor;

layout(binding = 1) uniform sampler2D tex;

uniform float intensity;

void main()
{    
    outputColor = texture(tex,Texcoords);

	outputColor *= pow(2,intensity);

}