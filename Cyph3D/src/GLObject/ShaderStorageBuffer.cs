using System;
using OpenGL;
using Renderer.Misc;

namespace Renderer.GLObject
{
	public class ShaderStorageBuffer
	{
		private uint _ID;
		private uint _index;

		public ShaderStorageBuffer(uint index)
		{
			_ID = Gl.GenBuffer();
			_index = index;
			Bind();
			Gl.BindBufferBase(BufferTarget.ShaderStorageBuffer, _index, _ID);
			Gl.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
		}

		public void PutData<T>(NativeArray<T> array) where T : unmanaged
		{
			Bind();
			Gl.BufferData(BufferTarget.ShaderStorageBuffer, array.ByteSize, array, BufferUsage.DynamicDraw);
			Gl.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
		}

		public void Bind()
		{
			Gl.BindBuffer(BufferTarget.ShaderStorageBuffer, _ID);
		}
	}
}