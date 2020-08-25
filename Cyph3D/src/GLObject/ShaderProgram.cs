using System;
using System.Collections.Generic;
using System.IO;
using Cyph3D.Extension;
using Cyph3D.Helper;
using Cyph3D.Misc;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public partial class ShaderProgram : BufferBase
	{
		private Dictionary<string, int> _uniforms = new Dictionary<string, int>();
		
		public ShaderProgram(Dictionary<ShaderType, string[]> data)
		{
			_id = GL.CreateProgram();
			if (_id == 0)
			{
				throw new InvalidOperationException("Unable to create shader program instance");
			}
			
			List<int> shaders = new List<int>();

			foreach ((ShaderType type, string[] files) in data)
			{
				int shader = LoadShader(type, files);
				GL.AttachShader(_id, shader);
				shaders.Add(shader);
			}
			
			GL.LinkProgram(_id);

			for (int i = 0; i < shaders.Count; i++)
			{
				int shader = shaders[i];
				GL.DetachShader(_id, shader);
				GL.DeleteShader(shader);
			}
			
			GL.GetProgram(_id, GetProgramParameterName.LinkStatus, out int linkSuccess);
			
			if(linkSuccess == (int)All.False)
			{
				GL.GetProgram(_id, GetProgramParameterName.InfoLogLength, out int length);
				
				GL.GetProgramInfoLog(_id, length, out _, out string error);
		
				throw new InvalidOperationException($"Error while linking shaders to program: {error}");
			}
			
			GL.GetProgramInterface(_id, ProgramInterface.Uniform, ProgramInterfaceParameter.ActiveResources, out int uniformCount);

			for (int i = 0; i < uniformCount; i++)
			{
				int[] values = new int[2];
				GL.GetProgramResource(
					_id,
					ProgramInterface.Uniform,
					i,
					2,
					new [] {ProgramProperty.NameLength, ProgramProperty.ArraySize},
					2,
					out int _,
					values
				);
				
				GL.GetProgramResourceName(_id, ProgramInterface.Uniform, i, values[0], out int _, out string name);

				if (values[1] > 1)
				{
					string arrayName = name.Remove("[0]");
					_uniforms.Add(arrayName, i);
					for (int j = 0; j < values[1]; j++)
					{
						string fullName = $"{arrayName}[{j}]";
						_uniforms.Add(fullName, GL.GetUniformLocation(_id, fullName));
					}
				}
				else
				{
					_uniforms.Add(name, i);
				}
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

		protected override void DeleteBuffer()
		{
			GL.DeleteProgram(_id);
		}

		public void Bind()
		{
			GL.UseProgram(_id);
		}
	}
}