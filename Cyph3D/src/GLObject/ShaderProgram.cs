using System;
using System.Collections.Generic;
using System.Linq;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class ShaderProgram : IDisposable
	{
		private int _ID;

		private Shader _vertex;
		private Shader _fragment;
		
		private Dictionary<string, int> _uniforms = new Dictionary<string, int>();
		
		public static implicit operator int(ShaderProgram shaderProgram) => shaderProgram._ID;
		
		public ShaderProgram(string shadersName)
		{
			_vertex = new Shader($"{shadersName}.vert", ShaderType.VertexShader);
			_fragment = new Shader($"{shadersName}.frag", ShaderType.FragmentShader);
			
			_ID = GL.CreateProgram();
			if (_ID == 0)
			{
				throw new InvalidOperationException($"Unable to create shader program instance for shader {shadersName}");
			}
			
			GL.AttachShader(_ID, _vertex);
			GL.AttachShader(_ID, _fragment);
	
			GL.LinkProgram(_ID);
			
			GL.DetachShader(_ID, _vertex);
			GL.DetachShader(_ID, _fragment);
			
			GL.GetProgram(_ID, GetProgramParameterName.LinkStatus, out int linkSuccess);
			
			if(linkSuccess == (int)All.False)
			{
				GL.GetProgram(_ID, GetProgramParameterName.InfoLogLength, out int length);
				
				GL.GetProgramInfoLog(_ID, length, out _, out string error);
		
				throw new InvalidOperationException($"Error while linking shaders ({_vertex.FileName}, {_fragment.FileName}) to program: {error}");
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

		private int GetLocation(string variableName)
		{
			return _uniforms.GetValueOrDefault(variableName, -1);
		}

		public void Dispose()
		{
			_vertex.Dispose();
			_fragment.Dispose();
			
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
	}
}