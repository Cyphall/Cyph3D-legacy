using System;
using System.Collections.Generic;
using System.Linq;
using Cyph3D.Extension;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class ShaderProgram : IDisposable
	{
		private int _ID;

		private Shader _vertex;
		private Shader _fragment;
		
		private Dictionary<string, int> _locations = new Dictionary<string, int>();
		
		public static implicit operator int(ShaderProgram shaderProgram) => shaderProgram._ID;
		
		private static Dictionary<string, ShaderProgram> _shaderPrograms = new Dictionary<string, ShaderProgram>();
		
		private ShaderProgram(string shadersName)
		{
			_vertex = Shader.Get($"{shadersName}.vert", ShaderType.VertexShader);
			_fragment = Shader.Get($"{shadersName}.frag", ShaderType.FragmentShader);
			
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
			
			_shaderPrograms.Add(shadersName, this);
		}
		
		public static ShaderProgram Get(string name)
		{
			if (!_shaderPrograms.ContainsKey(name))
			{
				Logger.Info($"Loading shader program \"{name}\"");
				ShaderProgram shaderProgram = new ShaderProgram(name);
				Logger.Info($"Shader program \"{name}\" loaded (id: {shaderProgram._ID})");
			}

			return _shaderPrograms[name];
		}

		private int GetLocation(string variableName)
		{
			if (!_locations.ContainsKey(variableName))
			{
				_locations.Add(variableName, GL.GetUniformLocation(_ID, variableName));
			}

			return _locations[variableName];
		}

		public void Dispose()
		{
			_vertex.Dispose();
			_fragment.Dispose();
			
			GL.DeleteProgram(_ID);
			_ID = 0;
		}
		
		public static void DisposeAll()
		{
			foreach (ShaderProgram program in _shaderPrograms.Values)
			{
				program.Dispose();
			}
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