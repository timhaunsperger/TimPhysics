#version 330 core

in vec3 aPosition;
in vec3 aColor;

out vec3 ourColor;

void main()
{
    gl_Position = vec4(aPosition, 1.0);
    ourColor = aColor;
}