using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public partial class ShaderProgram
	{
		public void SetValue(string variableName, params float[] data)
		{
			GL.ProgramUniform1(_id, GetLocation(variableName), data.Length, data);
		}
		
		public unsafe void SetValue(string variableName, params vec2[] data)
		{
			fixed (vec2* ptr = data)
			{
				GL.ProgramUniform2(_id, GetLocation(variableName), data.Length, (float*)ptr);
			}
		}
		
		public unsafe void SetValue(string variableName, params vec3[] data)
		{
			fixed (vec3* ptr = data)
			{
				GL.ProgramUniform3(_id, GetLocation(variableName), data.Length, (float*)ptr);
			}
		}
		
		public void SetValue(string variableName, params int[] data)
		{
			GL.ProgramUniform1(_id, GetLocation(variableName), data.Length, data);
		}
		
		public unsafe void SetValue(string variableName, params ivec2[] data)
		{
			fixed (ivec2* ptr = data)
			{
				GL.ProgramUniform2(_id, GetLocation(variableName), data.Length, (int*)ptr);
			}
		}
		
		public unsafe void SetValue(string variableName, params ivec3[] data)
		{
			fixed (ivec3* ptr = data)
			{
				GL.ProgramUniform3(_id, GetLocation(variableName), data.Length, (int*)ptr);
			}
		}

		public unsafe void SetValue(string variableName, params mat3[] data)
		{
			fixed (mat3* ptr = data)
			{
				GL.ProgramUniformMatrix3(_id, GetLocation(variableName), data.Length, false, (float*)ptr);
			}
		}
		
		public unsafe void SetValue(string variableName, params mat4[] data)
		{
			fixed (mat4* ptr = data)
			{
				GL.ProgramUniformMatrix4(_id, GetLocation(variableName), data.Length, false, (float*)ptr);
			}
		}
		
		public void SetValue(string variableName, params Texture[] data)
		{
			long[] handles = new long[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				handles[i] = data[i].BindlessHandle;
			}
			
			GL.Arb.ProgramUniformHandle(_id, GetLocation(variableName), data.Length, handles);
		}
		
		public void SetValue(string variableName, params Cubemap[] data)
		{
			long[] handles = new long[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				handles[i] = data[i].BindlessHandle;
			}
			
			GL.Arb.ProgramUniformHandle(_id, GetLocation(variableName), data.Length, handles);
		}
	}
}