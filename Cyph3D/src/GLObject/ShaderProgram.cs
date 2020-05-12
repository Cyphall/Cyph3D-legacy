using System;
using System.Collections.Generic;
using System.Linq;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using Renderer.Misc;

namespace Renderer.GLObject
{
	public class ShaderProgram : IDisposable
	{
		private int _ID;

		private Shader _vertex;
		private Shader _fragment;
		
		private Dictionary<string, int> _locations = new Dictionary<string, int>();

		private static int CurrentlyBound => GL.GetInteger(GetPName.CurrentProgram);
		
		public static implicit operator int(ShaderProgram shaderProgram) => shaderProgram._ID;
		
		private static Dictionary<string, ShaderProgram> _shaderPrograms = new Dictionary<string, ShaderProgram>();
		
		private ShaderProgram(string shadersName)
		{
			int previousProgram = CurrentlyBound;
			
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
			
			
			GL.GetProgram(_ID, GetProgramParameterName.LinkStatus, out int linkSuccess);
			
			if(linkSuccess == (int)All.False)
			{
				GL.GetProgram(_ID, GetProgramParameterName.InfoLogLength, out int length);
				
				GL.GetProgramInfoLog(_ID, length, out _, out string error);
		
				throw new InvalidOperationException($"Error while linking shaders ({_vertex.FileName}, {_fragment.FileName}) to program: {error}");
			}
			
			_shaderPrograms.Add(shadersName, this);
			
			Bind(previousProgram);
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
			Bind(this);
		}

		public void Unbind()
		{
			Bind(0);
		}
		
		private static void Bind(int shaderProgram)
		{
			GL.UseProgram(shaderProgram);
		}
		
		public void SetValue(string variableName, float data)
		{
			Bind();
			GL.Uniform1(GetLocation(variableName), data);
		}
		public void SetValue(string variableName, vec2 data)
		{
			Bind();
			GL.Uniform2(GetLocation(variableName), data.x, data.y);
		}
		public void SetValue(string variableName, vec3 data)
		{
			Bind();
			GL.Uniform3(GetLocation(variableName), data.x, data.y, data.z);
		}
		public void SetValue(string variableName, int data)
		{
			Bind();
			GL.Uniform1(GetLocation(variableName), data);
		}
		public void SetValue(string variableName, ivec2 data)
		{
			Bind();
			GL.Uniform2(GetLocation(variableName), data.x, data.y);
		}
		public void SetValue(string variableName, ivec3 data)
		{
			Bind();
			GL.Uniform3(GetLocation(variableName), data.x, data.y, data.z);
		}
		public void SetValue(string variableName, mat4 data)
		{
			Bind();
			GL.UniformMatrix4(GetLocation(variableName), 1, false, data.ToArray());
		}
	}
}