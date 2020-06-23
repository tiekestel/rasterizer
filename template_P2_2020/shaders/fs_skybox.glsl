#version 330 core
out vec4 FragColor;

in vec3 TexCoords;

uniform samplerCube skybox;

void main()
{    
	//set color for output
    FragColor = texture(skybox, TexCoords);
}