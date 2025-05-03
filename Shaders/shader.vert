#version 460 core

layout(location = 0) in vec3 v_position;
out vec3 frag_position;

void main() {
	gl_Position = vec4(v_position, 1.0);
	frag_position = v_position;
}