#version 330
 
// shader input
in vec2 uv;						// interpolated texture coordinates
in vec4 normal;					// interpolated normal
in vec3 position;
uniform sampler2D pixels;		// texture sampler

// shader output
out vec4 outputColor;

// fragment shader
void main()
{
    //Ambient light
    outputColor = texture( pixels, uv );

    //Diffuse light
    vec3 directionalLight = normalize(vec3(1, -1, 0));
    float angle = max(dot(directionalLight, normal.xyz), 0);

    //Specular light
    float specular = 0.0;
    if(angle > 0) {
        vec3 reflection = reflect(directionalLight, normal.xyz);
        vec3 view = normalize(-position);
        float specularAngle = max(dot(reflection, view), 0);
        specular = pow(specularAngle, 40);
    }

    outputColor += vec4(angle, angle, angle, 0) + vec4(specular, specular, specular, 0);
} 
