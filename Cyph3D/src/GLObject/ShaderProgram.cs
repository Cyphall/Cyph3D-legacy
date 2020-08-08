using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cyph3D.Helper;
using Cyph3D.Misc;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class ShaderProgram : IDisposable
	{
		private int _ID;

		private Dictionary<string, int> _uniforms = new Dictionary<string, int>();
		
		public static implicit operator int(ShaderProgram shaderProgramc) => shaderProgramc._ID;
		
		public ShaderProgram(Dictionary<ShaderType, string[]> data)
		{
			_ID = GL.CreateProgram();
			if (_ID == 0)
			{
				throw new InvalidOperationException("Unable to create shader program instance");
			}
			
			List<int> shaders = new List<int>();

			foreach ((ShaderType type, string[] files) in data)
			{
				int shader = LoadShader(type, files);
				GL.AttachShader(_ID, shader);
				shaders.Add(shader);
			}
			
			GL.LinkProgram(_ID);

			for (int i = 0; i < shaders.Count; i++)
			{
				int shader = shaders[i];
				GL.DetachShader(_ID, shader);
				GL.DeleteShader(shader);
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
		}

		private int LoadShader(ShaderType type, params string[] files)
		{
			int shader = GL.CreateShader(type);

			if (shader == 0)
			{
				throw new InvalidOperationException("Unable to create shader instance");
			}

			string source;
			
			string extension = ShaderHelper.TypeToExtension(type);
			
			try
			{
				source = File.ReadAllText($"resources/shaders/internal/shaderHeader.{extension}");
			}
			catch (IOException)
			{
				Logger.Error($"Unable to open shader file \"internal/shaderHeader.{extension}\"");
				throw;
			}

			for (int i = 0; i < files.Length; i++)
			{
				try
				{
					source += File.ReadAllText($"resources/shaders/{files[i]}.{extension}");
				}
				catch (IOException)
				{
					Logger.Error($"Unable to open shader file \"{files[i]}.{extension}\"");
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

			return shader;
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
		
		public void Bind()
		{
			GL.UseProgram(_ID);
		}
		
		public void SetValue(string variableName, float data)
		{
			GL.ProgramUniform1(_ID, GetLocation(variableName), data);
		}
		public void SetValue(string variableName, vec2 data)
		{
			GL.ProgramUniform2(_ID, GetLocation(variableName), data.x, data.y);
		}
		public void SetValue(string variableName, vec3 data)
		{
			GL.ProgramUniform3(_ID, GetLocation(variableName), data.x, data.y, data.z);
		}
		public void SetValue(string variableName, int data)
		{
			GL.ProgramUniform1(_ID, GetLocation(variableName), data);
		}
		public void SetValue(string variableName, ivec2 data)
		{
			GL.ProgramUniform2(_ID, GetLocation(variableName), data.x, data.y);
		}
		public void SetValue(string variableName, ivec3 data)
		{
			GL.ProgramUniform3(_ID, GetLocation(variableName), data.x, data.y, data.z);
		}
		public void SetValue(string variableName, mat4 data)
		{
			GL.ProgramUniformMatrix4(_ID, GetLocation(variableName), 1, false, data.ToArray());
		}
		public void SetValue(string variableName, Texture data)
		{
			GL.Arb.ProgramUniformHandle(_ID, GetLocation(variableName), data.BindlessHandle);
		}
		public void SetValue(string variableName, Cubemap data)
		{
			GL.Arb.ProgramUniformHandle(_ID, GetLocation(variableName), data.BindlessHandle);
		}
	}
}