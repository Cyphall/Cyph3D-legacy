using GlmSharp;
using Renderer.Misc;

namespace Renderer
{
	public class PointLight
	{
		public Transform Transform { get; } = new Transform();
		public vec3 Color { get; }
		public float Intensity { get; }

		public PointLight(vec3 position, vec3 color, float intensity)
		{
			Transform.Position = position;
			Color = color;
			Intensity = intensity;
		}
	}
}