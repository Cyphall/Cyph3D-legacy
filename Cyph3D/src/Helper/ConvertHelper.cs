﻿using System.Json;
using System.Numerics;
using GlmSharp;
using Quaternion = Cyph3D.Misc.Quaternion;

namespace Cyph3D.Helper
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
		
		public static Vector4 QuatConvert(Quaternion q)
		{
			return new Vector4((float)q.X, (float)q.Y, (float)q.Z, (float)q.W);
		}
		
		public static Quaternion QuatConvert(Vector4 vec)
		{
			return new Quaternion(vec.X, vec.Y, vec.Z, vec.W);
		}
		
		public static JsonArray JsonConvert(vec3 vec)
		{
			return new JsonArray(vec.x, vec.y, vec.z);
		}
		
		public static JsonArray JsonConvert(vec2 vec)
		{
			return new JsonArray(vec.x, vec.y);
		}
	}
}