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
		
		private static Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();
		
		private Shader(string fileName, ShaderType type)
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
			
			_shaders.Add(fileName, this);
		}
		
		public static Shader Get(string name, ShaderType type)
		{
			if (!_shaders.ContainsKey(name))
			{
				Logger.Info($"Loading shader \"{name}\"");
				Shader shader = new Shader(name, type);
				Logger.Info($"Shader \"{name}\" loaded (id: {shader._ID})");
			}

			return _shaders[name];
		}
		
		public void Dispose()
		{
			GL.DeleteShader(_ID);
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