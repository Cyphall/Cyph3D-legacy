using System.Runtime.InteropServices;
using Cyph3D.Misc;
using GlmSharp;

namespace Cyph3D.Lighting
{
	public class DirectionalLight : Light
	{
		public DirectionalLight(Transform parent, vec3 srgbColor, float intensity, string name = "DirectionalLight", vec3? position = null, vec3? rotation = null):
			base(parent, srgbColor, intensity, name, position, rotation)
		{
		}
		
		public NativeLightData NativeLight =>
			new NativeLightData
			{
				FragToLightDirection = (Transform.WorldMatrix * new vec4(0, 1, 0, 1)).xyz - Transform.WorldPosition,
				Color = LinearColor,
				Intensity = Intensity
			};

		[StructLayout(LayoutKind.Sequential)]
		public struct NativeLightData
		{
			public vec3  FragToLightDirection;
			public float Intensity;
			public vec3  Color;
			public float  Padding28;
		}
	}
}