using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlmSharp;
using OpenGL;
using Renderer.Misc;

namespace Renderer.GLObject
{
	public class ShaderProgram : IDisposable
	{
		private uint _ID;

		private Shader _vertex;
		private Shader _fragment;
		
		private Dictionary<string, int> _locations = new Dictionary<string, int>();

		private static uint CurrentlyBound
		{
			get
			{
				Gl.GetInteger(GetPName.CurrentProgram, out uint value);
				return value;
			}
		}
		
		public static implicit operator uint(ShaderProgram shaderProgram) => shaderProgram._ID;
		
		private static Dictionary<string, ShaderProgram> _shaderPrograms = new Dictionary<string, ShaderProgram>();
		
		private ShaderProgram(string shadersName)
		{
			uint previousProgram = CurrentlyBound;
			
			_vertex = Shader.Get($"{shadersName}.vert", ShaderType.VertexShader);
			_fragment = Shader.Get($"{shadersName}.frag", ShaderType.FragmentShader);
			
			_ID = Gl.CreateProgram();
			if (_ID == 0)
			{
				throw new InvalidOperationException($"Unable to create shader program instance for shader {shadersName}");
			}
			
			Gl.AttachShader(_ID, _vertex);
			Gl.AttachShader(_ID, _fragment);
	
			Gl.LinkProgram(_ID);
			
			
			Gl.GetProgram(_ID, ProgramProperty.LinkStatus, out int linkSuccess);
			
			if(linkSuccess == Gl.FALSE)
			{
				Gl.GetProgram(_ID, ProgramProperty.InfoLogLength, out int length);
				
				StringBuilder error = new StringBuilder(length);
				Gl.GetProgramInfoLog(_ID, length, out _, error);
		
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
				// ReSharper disable once ObjectCreationAsStatement
				new ShaderProgram(name);
				Logger.Info($"Shader program \"{name}\" loaded");
			}

			return _shaderPrograms[name];
		}

		private int GetLocation(string variableName)
		{
			if (!_locations.ContainsKey(variableName))
			{
				_locations.Add(variableName, Gl.GetUniformLocation(_ID, variableName));
			}

			return _locations[variableName];
		}

		public void Dispose()
		{
			_vertex.Dispose();
			_fragment.Dispose();
			
			Gl.DeleteProgram(_ID);
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
		
		private static void Bind(uint shaderProgram)
		{
			Gl.UseProgram(shaderProgram);
		}
		
		public void SetValue(string variableName, float data)
		{
			Bind();
			Gl.Uniform1(GetLocation(variableName), data);
		}
		public void SetValue(string variableName, vec2 data)
		{
			Bind();
			Gl.Uniform2(GetLocation(variableName), data.x, data.y);
		}
		public void SetValue(string variableName, vec3 data)
		{
			Bind();
			Gl.Uniform3(GetLocation(variableName), data.x, data.y, data.z);
		}
		public void SetValue(string variableName, int data)
		{
			Bind();
			Gl.Uniform1(GetLocation(variableName), data);
		}
		public void SetValue(string variableName, ivec2 data)
		{
			Bind();
			Gl.Uniform2(GetLocation(variableName), data.x, data.y);
		}
		public void SetValue(string variableName, ivec3 data)
		{
			Bind();
			Gl.Uniform3(GetLocation(variableName), data.x, data.y, data.z);
		}
		public void SetValue(string variableName, mat4 data)
		{
			Bind();
			Gl.UniformMatrix4(GetLocation(variableName), false, data.ToArray());
		}
	}
}