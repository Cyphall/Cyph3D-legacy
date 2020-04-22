using System;
using GlmSharp;
using OpenGL;

namespace Renderer.Renderer
{
	public interface IRenderer
	{
		void Render(mat4 view, mat4 projection, vec3 viewPos);
	}
}