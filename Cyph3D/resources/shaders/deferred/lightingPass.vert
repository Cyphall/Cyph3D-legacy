#version 460 core

layout(location = 0) in vec2 in_Vertex;
layout(location = 1) in vec2 in_UV;

uniform vec3 viewPos;

out FRAG {
    vec2  TexCoords;
    vec3  ViewPos;
} frag;

void main()
{
    gl_Position = vec4(in_Vertex, 0, 1);

    frag.TexCoords = in_UV;
    frag.ViewPos = viewPos;
}