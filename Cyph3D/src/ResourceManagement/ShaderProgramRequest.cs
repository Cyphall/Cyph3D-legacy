using System.Collections.Generic;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.ResourceManagement
{
	public class ShaderProgramRequest
	{
		public Dictionary<ShaderType, string[]> Data { get; } = new Dictionary<ShaderType, string[]>();

		public ShaderProgramRequest WithShader(ShaderType type, params string[] files)
		{
			Data.Add(type, files);
				
			return this;
		}
	}
}