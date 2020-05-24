using System;
using System.Collections.Generic;
using Cyph3D.Extension;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class ShaderStorageBuffer : IDisposable
	{
		private int _ID;
		private int _index;
		
		private static HashSet<int> _usedIndexes = new HashSet<int>();
		
		private static HashSet<ShaderStorageBuffer> _shaderStorageBuffers = new HashSet<ShaderStorageBuffer>();
		
		private static int CurrentlyBound => GL.GetInteger((GetPName)All.ShaderStorageBufferBinding);
		
		public static implicit operator int(ShaderStorageBuffer shaderStorageBuffer) => shaderStorageBuffer._ID;

		public ShaderStorageBuffer(int index)
		{
			int previous = CurrentlyBound;
			
			_ID = GL.GenBuffer();
			_index = index;
			_usedIndexes.Add(index);
			
			Bind();
			
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, _index, _ID);

			_shaderStorageBuffers.Add(this);
			
			Bind(previous);
		}

		public void PutData<T>(NativeArray<T> array) where T : unmanaged
		{
			int previous = CurrentlyBound;
			Bind();
			
			GL.BufferData(BufferTarget.ShaderStorageBuffer, array.ByteSize, array, BufferUsageHint.DynamicDraw);
			
			Bind(previous);
		}
		
		public void Bind()
		{
			Bind(this);
		}
		
		private static void Bind(int shaderStorageBuffer)
		{
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, shaderStorageBuffer);
		}

		public void Dispose()
		{
			GL.DeleteBuffer(_ID);
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