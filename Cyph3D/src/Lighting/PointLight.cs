using System.Runtime.InteropServices;
using Cyph3D.Misc;
using GlmSharp;

namespace Cyph3D.Lighting
{
	public class PointLight : Light
	{
		public PointLight(Transform parent, vec3 srgbColor, float intensity, string name = "PointLight", vec3? position = null, vec3? rotation = null):
			base(parent, srgbColor, intensity, name, position, rotation)
		{
		}
		
		public NativeLightData NativeLight =>
			new NativeLightData
			{
				Pos = Transform.WorldPosition,
				Color = LinearColor,
				Intensity = Intensity
			};

		[StructLayout(LayoutKind.Sequential)]
		public struct NativeLightData
		{
			public vec3  Pos;
			public float Intensity;
			public vec3  Color;
			public float Padding28;
		}
	}
}