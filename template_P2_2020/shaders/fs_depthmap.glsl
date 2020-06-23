#version 330 core
in vec3 position;
void main()
{    
	//write distance from directional light
	if(position.z < 1000){
		gl_FragDepth = position.z/1000;
	}
	else{
		gl_FragDepth = 1;
	}
}