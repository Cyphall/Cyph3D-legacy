using System.Collections.Generic;
using Renderer.Misc;

namespace Renderer
{
	public static class Context
	{
		public static Window Window { get; set; }
		public static List<RenderObject> ObjectContainer { get; } = new List<RenderObject>();
		public static Transform SceneRoot { get; } = new Transform("Root");
		public static LightManager LightManager { get; } = new LightManager();
	}
}