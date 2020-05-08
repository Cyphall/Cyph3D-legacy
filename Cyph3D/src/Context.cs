using System.Collections.Generic;
using GlmSharp;

namespace Renderer
{
	public static unsafe class Context
	{
		public static Window Window { get; set; }
		public static List<RenderObject> ObjectContainer { get; } = new List<RenderObject>();
		public static LightManager LightManager { get; } = new LightManager();
	}
}