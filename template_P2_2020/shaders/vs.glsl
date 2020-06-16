#version 420
// shader input
in vec2 vUV;				// vertex uv coordinate
in vec3 vNormal;			// untransformed vertex normal
in vec3 vPosition;			// untransformed vertex position
in vec3 vTangent;
in vec3 vBitangent;


// shader output
out vec4 normal;			// transformed vertex normal
out vec2 uv;	
out vec3 position;
out mat3 TBN;
uniform mat4 transform;
uniform mat4 camera;
 
// vertex shader
void main()
{
	// transform vertex using supplied matrix

	gl_Position = camera * transform  * vec4(vPosition, 1.0);
	position = (transform  * vec4(vPosition, 1.0)).xyz;
	// forward normal and uv coordinate; will be interpolated over triangle

	normal = mat4(transpose(inverse(transform))) * vec4(vNormal, 0);
	uv = vUV;
	vec3 T = normalize(vec3(transform * vec4(vTangent, 0)));
	vec3 B = normalize(vec3(transform * vec4(vBitangent, 0)));
	vec3 N = normalize(vec3(transform * vec4(vNormal, 0)));
	TBN = mat3(T,B,N);
}
