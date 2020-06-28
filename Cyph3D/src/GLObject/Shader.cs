using System;
using System.Collections.Generic;
using System.IO;
using Cyph3D.Misc;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class Shader : IDisposable
	{
		private int _ID;
		public string FileName { get; }

		public static implicit operator int(Shader shader) => shader._ID;
		
		public Shader(string fileName, ShaderType type)
		{
			FileName = fileName;
			
			_ID = GL.CreateShader(type);

			if (_ID == 0)
			{
				throw new InvalidOperationException($"Unable to create shader instance for {FileName}");
			}

			string source = "";

			try
			{
				source = File.ReadAllText($"resources/shaders/{FileName}");
			}
			catch (IOException)
			{
				Logger.Error($"Unable to open shader file \"{FileName}\"");
				throw;
			}

			GL.ShaderSource(_ID, source);
			GL.CompileShader(_ID);
			
			GL.GetShader(_ID, ShaderParameter.CompileStatus, out int compileSuccess);
			
			if(compileSuccess == (int)All.False)
			{
				GL.GetShader(_ID, ShaderParameter.InfoLogLength, out int length);
				
				GL.GetShaderInfoLog(_ID, length, out _, out string error);
		
				throw new InvalidOperationException($"Error while compiling shader {FileName}: {error}");
			}
		}
		
		public void Dispose()
		{
			GL.DeleteShader(_ID);
			_ID = 0;
		}
	}
}