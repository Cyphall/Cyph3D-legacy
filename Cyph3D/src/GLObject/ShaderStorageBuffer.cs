using System;
using System.Collections.Generic;
using Cyph3D.Misc;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class ShaderStorageBuffer : IDisposable
	{
		private int _ID;
		private int _index;
		
		private static HashSet<int> _usedIndexes = new HashSet<int>();
		
		private static HashSet<ShaderStorageBuffer> _shaderStorageBuffers = new HashSet<ShaderStorageBuffer>();
		
		public static implicit operator int(ShaderStorageBuffer shaderStorageBuffer) => shaderStorageBuffer._ID;

		public ShaderStorageBuffer(int index)
		{
			GL.CreateBuffers(1, out _ID);
			_index = index;
			_usedIndexes.Add(index);
			
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, _index, _ID);

			_shaderStorageBuffers.Add(this);
		}

		public void PutData<T>(NativeArray<T> array) where T : unmanaged
		{
			GL.NamedBufferData(_ID, array.ByteSize, array, BufferUsageHint.DynamicDraw);
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