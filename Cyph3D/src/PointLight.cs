using System.Runtime.InteropServices;
using Cyph3D.Misc;
using GlmSharp;

namespace Cyph3D
{
	public class PointLight
	{
		public Transform Transform { get; }
		private vec3 _sRGBColor;
		private vec3 _linearColor;

		public vec3 Color
		{
			get => _sRGBColor;
			set
			{
				_sRGBColor = value;
				_linearColor = ToLinear(value);
			}
		}
		public float Intensity { get; }

		public PointLight(vec3? position, vec3 color, float intensity, Transform parent = null)
		{
			Transform = new Transform("PointLight", parent, position);
			Color = color;
			Intensity = intensity;
		}
		
		private static vec3 ToLinear(vec3 sRGB)
		{
			bvec3 cutoff = vec3.LesserThan(sRGB, new vec3(0.04045f));
			vec3 higher = vec3.Pow((sRGB + new vec3(0.055f)) / new vec3(1.055f), new vec3(2.4f));
			vec3 lower = sRGB / new vec3(12.92f);

			return vec3.Mix(higher, lower, new vec3(cutoff.x ? 1 : 0, cutoff.y ? 1 : 0, cutoff.z ? 1 : 0));
		}

		public NativePointLight GLLight =>
			new NativePointLight
			{
				Pos = Transform.WorldPosition,
				Color = _linearColor,
				Intensity = Intensity
			};

		[StructLayout(LayoutKind.Explicit)]
		public struct NativePointLight
		{
			[FieldOffset(0)] public vec3  Pos;
			[FieldOffset(12)]public float Intensity;
			[FieldOffset(16)]public vec3  Color;
			[FieldOffset(28)]public float Padding28;
		}
	}
}