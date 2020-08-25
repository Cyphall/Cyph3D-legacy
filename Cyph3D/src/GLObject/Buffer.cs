using System;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public unsafe class Buffer<T> : BufferBase where T: unmanaged
	{
		public bool IsMutable { get; }

		private bool _isAlloccated;

		public Buffer(bool mutable)
		{
			GL.CreateBuffers(1, out _id);
			IsMutable = mutable;
		}

		public void PutData(T[] data)
		{
			if (IsMutable)
			{
				GL.NamedBufferData(_id, sizeof(T) * data.Length, data, BufferUsageHint.DynamicDraw);
			}
			else
			{
				if (_isAlloccated)
				{
					GL.NamedBufferSubData(_id, IntPtr.Zero, sizeof(T) * data.Length, data);
				}
				else
				{
					GL.NamedBufferStorage(_id, sizeof(T) * data.Length, data, BufferStorageFlags.None);
					_isAlloccated = true;
				}
			}
		}

		protected override void DeleteBuffer()
		{
			GL.DeleteBuffer(_id);
		}
	}
}