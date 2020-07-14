using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public unsafe class VertexArray : IDisposable
	{
		private int _ID;
		
		private List<BufferBase> _mappedBuffers = new List<BufferBase>();

		public VertexArray()
		{
			GL.CreateVertexArrays(1, out _ID);
		}

		private int ResolveBufferIndex<T>(VertexBuffer<T> buffer) where T: unmanaged
		{
			if (!_mappedBuffers.Contains(buffer))
			{
				_mappedBuffers.Add(buffer);
				GL.VertexArrayVertexBuffer(_ID, _mappedBuffers.Count-1, buffer, IntPtr.Zero, buffer.Stride);
			}

			return _mappedBuffers.IndexOf(buffer);
		}

		public void RegisterAttrib<T>(VertexBuffer<T> buffer, int index, int dataCount, VertexAttribType dataType, int offset) where T: unmanaged
		{
			int bindingIndex = ResolveBufferIndex(buffer);
			
			GL.EnableVertexArrayAttrib(_ID, index);
			GL.VertexArrayAttribFormat(_ID, index, dataCount, dataType, false, offset);
			GL.VertexArrayAttribBinding(_ID, index, bindingIndex);
		}
		
		public void RegisterAttrib<T>(VertexBuffer<T> buffer, int index, int dataCount, VertexAttribType dataType, string structFieldName) where T: unmanaged
		{
			RegisterAttrib(buffer, index, dataCount, dataType, Marshal.OffsetOf<T>(structFieldName).ToInt32());
		}

		public void RegisterIndexBuffer(BufferBase buffer)
		{
			GL.VertexArrayElementBuffer(_ID, buffer);
		}

		public void Bind()
		{
			GL.BindVertexArray(_ID);
		}

		public void Dispose()
		{
			GL.DeleteVertexArray(_ID);
			_ID = 0;
		}
	}
}