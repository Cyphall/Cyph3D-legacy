using Cyph3D.Extension;
using GlmSharp;

namespace Cyph3D.Light
{
	public abstract class Light<T> : SceneObject where T: struct
	{
		private vec3 _sRGBColor;
		private vec3 _linearColor;
		
		public float Intensity { get; set; }

		protected Light(Transform parent, vec3 srgbColor, float intensity, string name, vec3? position = null, vec3? rotation = null):
			base(parent, name, position, rotation)
		{
			SrgbColor = srgbColor;
			Intensity = intensity;
		}
		
		public vec3 SrgbColor
		{
			get => _sRGBColor;
			set
			{
				_sRGBColor = value;
				_linearColor = ToLinear(value);
			}
		}
		
		public vec3 LinearColor
		{
			get => _linearColor;
			set
			{
				_linearColor = value;
				_sRGBColor = ToSrgb(value);
			}
		}
		
		public abstract T NativeLight { get; }
		
		private static vec3 ToLinear(vec3 sRGB)
		{
			bvec3 cutoff = vec3.LesserThan(sRGB, 0.04045f);
			vec3 higher = vec3.Pow((sRGB + 0.055f) / 1.055f, 2.4f);
			vec3 lower = sRGB / 12.92f;
			
			return vec3.Mix(higher, lower, (vec3)cutoff);
		}
		
		private static vec3 ToSrgb(vec3 linear)
		{
			bvec3 cutoff = vec3.LesserThan(linear, 0.0031308f);
			vec3 higher = 1.055f * vec3.Pow(linear, 1.0f / 2.4f) - 0.055f;
			vec3 lower = linear * 12.92f;
			
			return vec3.Mix(higher, lower, (vec3)cutoff);
		}
	}
}