using System.Collections.Generic;
using GLFW;
using GlmSharp;

namespace Renderer
{
	public static class Context
	{
		public static Window Window { get; set; }
		public static ivec2 WindowSize { get; set; }
		public static List<RenderObject> ObjectContainer { get; } = new List<RenderObject>();
		public static LightManager LightManager { get; } = new LightManager();
	}
}