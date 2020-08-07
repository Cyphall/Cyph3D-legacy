using System;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.Helper
{
	public static class ShaderHelper
	{
		public static string TypeToExtension(ShaderType type)
		{
			return type switch
			{
				ShaderType.VertexShader => "vert",
				ShaderType.FragmentShader => "frag",
				_ => throw new NotSupportedException($"Shader type {type} is not currently supported")
			};
		}
	}
}