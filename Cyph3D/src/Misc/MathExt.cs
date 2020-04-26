﻿using GlmSharp;

namespace Renderer.Misc
{
	public static class MathExt
	{
		public static int Modulo(int num, int mod)
		{
			int result = num % mod;
			return result < 0 ? result + mod : result;
		}
		
		public static float Modulo(float num, float mod)
		{
			float result = num % mod;
			return result < 0 ? result + mod : result;
		}
		
		public static double Modulo(double num, double mod)
		{
			double result = num % mod;
			return result < 0 ? result + mod : result;
		}

		public static vec3 Modulo(vec3 vec, float mod)
		{
			return new vec3(
				Modulo(vec.x, mod),
				Modulo(vec.y, mod),
				Modulo(vec.z, mod)
				);
		}

		public static vec3 FromRGB(byte r, byte g, byte b)
		{
			return new vec3(r / 255f, g / 255f, b / 255f);
		}
	}
}