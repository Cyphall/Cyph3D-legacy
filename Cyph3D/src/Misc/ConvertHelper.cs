using System.Numerics;
using GlmSharp;

namespace Cyph3D.Misc
{
	public static class ConvertHelper
	{
		public static vec3 Convert(Vector3 vec)
		{
			return new vec3(vec.X, vec.Y, vec.Z);
		}
		
		public static Vector3 Convert(vec3 vec)
		{
			return new Vector3(vec.x, vec.y, vec.z);
		}
		
		public static vec4 Convert(Vector4 vec)
		{
			return new vec4(vec.X, vec.Y, vec.Z, vec.W);
		}
		
		public static Vector4 Convert(vec4 vec)
		{
			return new Vector4(vec.x, vec.y, vec.z, vec.w);
		}
	}
}