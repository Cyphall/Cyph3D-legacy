using System.Collections.Generic;
using GlmSharp;
using Renderer.Misc;

namespace Renderer
{
	public static unsafe class Context
	{
		public static Window Window { get; set; }
		public static List<RenderObject> ObjectContainer { get; } = new List<RenderObject>();
		public static Transform SceneRoot { get; } = new Transform();
		public static LightManager LightManager { get; } = new LightManager();
	}
}