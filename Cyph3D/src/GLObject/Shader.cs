using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenGL;
using Renderer.Misc;

namespace Renderer.GLObject
{
	public class Shader : IDisposable
	{
		private uint _ID;
		public string FileName { get; }

		public static implicit operator uint(Shader shader) => shader._ID;
		
		private static Dictionary<(string, ShaderType), Shader> _shaders = new Dictionary<(string, ShaderType), Shader>();
		
		private Shader(string fileName, ShaderType type)
		{
			FileName = fileName;
			
			_ID = Gl.CreateShader(type);

			if (_ID == 0)
			{
				throw new InvalidOperationException($"Unable to create shader instance for {FileName}");
			}

			string[] source = new string[1];

			try
			{
				source[0] = File.ReadAllText($"resources/shaders/{FileName}");
			}
			catch (IOException)
			{
				Logger.Error($"Unable to open shader file \"{FileName}\"");
				throw;
			}

			Gl.ShaderSource(_ID, source);
			Gl.CompileShader(_ID);
			
			Gl.GetShader(_ID, ShaderParameterName.CompileStatus, out int compileSuccess);
			
			if(compileSuccess == Gl.FALSE)
			{
				Gl.GetShader(_ID, ShaderParameterName.InfoLogLength, out int length);
				
				StringBuilder error = new StringBuilder(length);
				Gl.GetShaderInfoLog(_ID, length, out _, error);
		
				throw new InvalidOperationException($"Error while compiling shader {FileName}: {error}");
			}
		}
		
		public static Shader Get(string name, ShaderType type)
		{
			if (!_shaders.ContainsKey((name, type)))
			{
				Logger.Info($"Loading shader \"{name}\"");
				_shaders.Add((name, type), new Shader(name, type));
				Logger.Info($"Shader \"{name}\" loaded");
			}

			return _shaders[(name, type)];
		}
		
		public void Dispose()
		{
			Gl.DeleteShader(_ID);
			_ID = 0;
		}
		
		public static void DisposeAll()
		{
			foreach (Shader shader in _shaders.Values)
			{
				shader.Dispose();
			}
		}
	}
}