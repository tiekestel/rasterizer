#version 330 core
in vec3 position;
void main()
{    
	//write distance from pointlight
	if(position.z < 500){
		gl_FragDepth = length(position)/500;
	}
	else{
		gl_FragDepth = 1;
	}
}