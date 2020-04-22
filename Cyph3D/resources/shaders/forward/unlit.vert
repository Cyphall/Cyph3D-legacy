#version 460 core

layout(location = 0) in vec3 in_Vertex;
layout(location = 1) in vec2 in_UV;
layout(location = 2) in vec3 in_Normals;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 frag_UV;
flat out vec3 frag_Normals;

void main()
{
    gl_Position = projection * view * model * vec4(in_Vertex, 1);

    frag_UV = in_UV;
    frag_Normals = in_Normals;
}