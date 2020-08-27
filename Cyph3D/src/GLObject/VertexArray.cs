using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class VertexArray : BufferBase
	{
		private List<BufferBase> _mappedBuffers = new List<BufferBase>();

		public VertexArray()
		{
			GL.CreateVertexArrays(1, out _id);
		}

		private int ResolveBufferIndex<T>(VertexBuffer<T> buffer) where T: unmanaged
		{
			if (!_mappedBuffers.Contains(buffer))
			{
				_mappedBuffers.Add(buffer);
				GL.VertexArrayVertexBuffer(_id, _mappedBuffers.Count-1, buffer, IntPtr.Zero, buffer.Stride);
			}

			return _mappedBuffers.IndexOf(buffer);
		}

		public void RegisterAttrib<T>(VertexBuffer<T> buffer, int location, int elementsPerVertex, VertexAttribType dataType, int offset) where T: unmanaged
		{
			int bindingIndex = ResolveBufferIndex(buffer);
			
			GL.EnableVertexArrayAttrib(_id, location);
			GL.VertexArrayAttribFormat(_id, location, elementsPerVertex, dataType, false, offset);
			GL.VertexArrayAttribBinding(_id, location, bindingIndex);
		}
		
		public void RegisterAttrib<T>(VertexBuffer<T> buffer, int location, int elementsPerVertex, VertexAttribType dataType, string structFieldName) where T: unmanaged
		{
			RegisterAttrib(buffer, location, elementsPerVertex, dataType, Marshal.OffsetOf<T>(structFieldName).ToInt32());
		}

		public void RegisterIndexBuffer(BufferBase buffer)
		{
			GL.VertexArrayElementBuffer(_id, buffer);
		}

		public void Bind()
		{
			GL.BindVertexArray(_id);
		}

		protected override void DeleteBuffer()
		{
			GL.DeleteVertexArray(_id);
		}
	}
}