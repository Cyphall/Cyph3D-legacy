using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cyph3D.Misc;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class ShaderProgram : IDisposable
	{
		private int _ID;

		private List<int> _shaders = new List<int>();
		private Dictionary<string, int> _uniforms = new Dictionary<string, int>();

		private bool _initialized;
		
		public static implicit operator int(ShaderProgram shaderProgramc) => shaderProgramc._ID;
		
		public ShaderProgram()
		{
			_ID = GL.CreateProgram();
			if (_ID == 0)
			{
				throw new InvalidOperationException("Unable to create shader program instance");
			}
		}

		public ShaderProgram Build()
		{
			GL.LinkProgram(_ID);

			for (int i = 0; i < _shaders.Count; i++)
			{
				int shader = _shaders[i];
				GL.DetachShader(_ID, shader);
			}
			
			GL.GetProgram(_ID, GetProgramParameterName.LinkStatus, out int linkSuccess);
			
			if(linkSuccess == (int)All.False)
			{
				GL.GetProgram(_ID, GetProgramParameterName.InfoLogLength, out int length);
				
				GL.GetProgramInfoLog(_ID, length, out _, out string error);
		
				throw new InvalidOperationException($"Error while linking shaders to program: {error}");
			}
			
			GL.GetProgramInterface(_ID, ProgramInterface.Uniform, ProgramInterfaceParameter.ActiveResources, out int uniformCount);

			for (int i = 0; i < uniformCount; i++)
			{
				int[] values = new int[1];
				GL.GetProgramResource(
					_ID,
					ProgramInterface.Uniform,
					i,
					1,
					new [] {ProgramProperty.NameLength},
					1,
					out int _,
					values
				);
				
				GL.GetProgramResourceName(_ID, ProgramInterface.Uniform, i, values[0], out int _, out string name);
				
				_uniforms.Add(name, i);
			}

			_initialized = true;

			return this;
		}

		public ShaderProgram WithShader(ShaderType type, params string[] files)
		{
			int shader = GL.CreateShader(type);

			if (shader == 0)
			{
				throw new InvalidOperationException("Unable to create shader instance");
			}

			string source = "";

			for (int i = 0; i < files.Length; i++)
			{
				try
				{
					source += File.ReadAllText($"resources/shaders/{files[i]}");
				}
				catch (IOException)
				{
					Logger.Error($"Unable to open shader file \"{files[i]}\"");
					throw;
				}
			}

			GL.ShaderSource(shader, source);
			GL.CompileShader(shader);
			
			GL.GetShader(shader, ShaderParameter.CompileStatus, out int compileSuccess);
			
			if(compileSuccess == (int)All.False)
			{
				GL.GetShader(shader, ShaderParameter.InfoLogLength, out int length);
				
				GL.GetShaderInfoLog(shader, length, out _, out string error);
		
				throw new InvalidOperationException($"Error while compiling shader: {error}");
			}
			
			GL.AttachShader(_ID, shader);
			_shaders.Add(shader);

			return this;
		}

		private int GetLocation(string variableName)
		{
			return _uniforms.GetValueOrDefault(variableName, -1);
		}

		public void Dispose()
		{
			GL.DeleteProgram(_ID);
			_ID = 0;
		}

		private void EnsureInitialized(string action)
		{
			if (!_initialized)
				throw new InvalidOperationException($"Unable to {action}, shader program is not initialized.");
		}
		
		public void Bind()
		{
			EnsureInitialized("bind");
			GL.UseProgram(_ID);
		}
		
		public void SetValue(string variableName, float data)
		{
			EnsureInitialized("set uniform value");
			GL.ProgramUniform1(_ID, GetLocation(variableName), data);
		}
		public void SetValue(string variableName, vec2 data)
		{
			EnsureInitialized("set uniform value");
			GL.ProgramUniform2(_ID, GetLocation(variableName), data.x, data.y);
		}
		public void SetValue(string variableName, vec3 data)
		{
			EnsureInitialized("set uniform value");
			GL.ProgramUniform3(_ID, GetLocation(variableName), data.x, data.y, data.z);
		}
		public void SetValue(string variableName, int data)
		{
			EnsureInitialized("set uniform value");
			GL.ProgramUniform1(_ID, GetLocation(variableName), data);
		}
		public void SetValue(string variableName, ivec2 data)
		{
			EnsureInitialized("set uniform value");
			GL.ProgramUniform2(_ID, GetLocation(variableName), data.x, data.y);
		}
		public void SetValue(string variableName, ivec3 data)
		{
			EnsureInitialized("set uniform value");
			GL.ProgramUniform3(_ID, GetLocation(variableName), data.x, data.y, data.z);
		}
		public void SetValue(string variableName, mat4 data)
		{
			EnsureInitialized("set uniform value");
			GL.ProgramUniformMatrix4(_ID, GetLocation(variableName), 1, false, data.ToArray());
		}
		public void SetValue(string variableName, Texture data)
		{
			EnsureInitialized("set uniform value");
			GL.Arb.ProgramUniformHandle(_ID, GetLocation(variableName), data.BindlessHandle);
		}
		public void SetValue(string variableName, Cubemap data)
		{
			EnsureInitialized("set uniform value");
			GL.Arb.ProgramUniformHandle(_ID, GetLocation(variableName), data.BindlessHandle);
		}
	}
}