using System.Runtime.InteropServices;
using Cyph3D.Extension;
using GlmSharp;

namespace Cyph3D.Light
{
	public class PointLight : Light<PointLight.NativePointLight>
	{
		public PointLight(Transform parent, vec3 srgbColor, float intensity, string name = "PointLight", vec3? position = null, vec3? rotation = null):
			base(parent, srgbColor, intensity, name, position, rotation)
		{
		}
		
		public override NativePointLight NativeLight =>
			new NativePointLight
			{
				Pos = Transform.WorldPosition,
				Color = LinearColor,
				Intensity = Intensity
			};

		[StructLayout(LayoutKind.Sequential)]
		public struct NativePointLight
		{
			public vec3  Pos;
			public float Intensity;
			public vec3  Color;
			public float Padding28;
		}
	}
}