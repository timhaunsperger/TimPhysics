#version 330

out vec4 FragColor;
in vec3 Normal;
in vec3 FragPos;
in vec2 texCoord;


uniform sampler2D texture0;
uniform vec3 viewPos;
uniform sampler2D texture1;

void main()
{
    float ambientStrength = 0.8;
    vec3 lightColor = vec3(1,1,1);
    vec3 ambient = ambientStrength * lightColor;
    
    float diffuseStrength = 1.2;
    vec3 lightPos = vec3(0, 100, 0);
    vec3 lightDir = normalize(lightPos - FragPos);
    vec3 norm = normalize(Normal);
    float diff = sqrt(max(dot(norm, lightDir), 0.0));
    vec3 diffuse = diff * lightColor * diffuseStrength;
   
    
    float specularStrength = 0.2;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * lightColor;

    vec4 result = vec4((ambient+diffuse+specular), 1.0);
    FragColor = result * 0.2 + mix(texture(texture0, texCoord), texture(texture1, texCoord), 0.5);
}