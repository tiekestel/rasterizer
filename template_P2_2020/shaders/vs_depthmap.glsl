#version 420
// shader input
in vec3 vPosition;			// untransformed vertex position

uniform mat4 camera;
uniform mat4 transform;
 
 out vec3 position;
// vertex shader
void main()
{
	// transform vertex using supplied matrix
	gl_Position = camera * transform * vec4(vPosition, 1.0);
	position = vec3(camera * transform * vec4(vPosition,1.0));
}