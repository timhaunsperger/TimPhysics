#version 330 core

in vec3 aPosition;
in vec3 aNormal;
in vec2 aTexCoord;

out vec3 Normal;
out vec3 FragPos;
out vec2 texCoord;

uniform mat4 view;
uniform mat4 projection;


void main(void)
{
    gl_Position = vec4(aPosition, 1.0) * view * projection;
    texCoord = aTexCoord;
    FragPos = aPosition;
    Normal = aNormal;
}