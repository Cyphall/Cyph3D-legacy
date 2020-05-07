using System;
using System.Collections.Generic;
using OpenGL;
using Renderer.Misc;

namespace Renderer.GLObject
{
	public class ShaderStorageBuffer : IDisposable
	{
		private uint _ID;
		private uint _index;
		
		private static HashSet<uint> _usedIndexes = new HashSet<uint>();
		
		private static HashSet<ShaderStorageBuffer> _shaderStorageBuffers = new HashSet<ShaderStorageBuffer>();
		
		private static uint CurrentlyBound
		{
			get
			{
				Gl.GetInteger(GetPName.ShaderStorageBufferBinding, out uint value);
				return value;
			}
		}
		
		public static implicit operator uint(ShaderStorageBuffer shaderStorageBuffer) => shaderStorageBuffer._ID;

		public ShaderStorageBuffer(uint index)
		{
			uint previous = CurrentlyBound;
			
			_ID = Gl.GenBuffer();
			_index = index;
			_usedIndexes.Add(index);
			
			Bind();
			
			Gl.BindBufferBase(BufferTarget.ShaderStorageBuffer, _index, _ID);

			_shaderStorageBuffers.Add(this);
			
			Bind(previous);
		}

		public void PutData<T>(NativeArray<T> array) where T : unmanaged
		{
			uint previous = CurrentlyBound;
			Bind();
			
			Gl.BufferData(BufferTarget.ShaderStorageBuffer, array.ByteSize, array, BufferUsage.DynamicDraw);
			
			Bind(previous);
		}
		
		public void Bind()
		{
			Bind(this);
		}
		
		private static void Bind(uint shaderStorageBuffer)
		{
			Gl.BindBuffer(BufferTarget.ShaderStorageBuffer, shaderStorageBuffer);
		}

		public void Dispose()
		{
			Gl.DeleteBuffers(_ID);
			_ID = 0;
		}
		
		public static void DisposeAll()
		{
			foreach (ShaderStorageBuffer shaderStorageBuffer in _shaderStorageBuffers)
			{
				shaderStorageBuffer.Dispose();
			}
		}
	}
}