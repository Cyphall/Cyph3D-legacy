using System;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public unsafe class Buffer<T> : BufferBase, IDisposable where T: unmanaged
	{
		public bool IsMutable { get; }

		private bool _isAlloccated;

		public Buffer(bool mutable)
		{
			IsMutable = mutable;
		}

		public void PutData(T[] data)
		{
			if (IsMutable)
			{
				GL.NamedBufferData(_ID, sizeof(T) * data.Length, data, BufferUsageHint.DynamicDraw);
			}
			else
			{
				if (_isAlloccated)
				{
					GL.NamedBufferSubData(_ID, IntPtr.Zero, sizeof(T) * data.Length, data);
				}
				else
				{
					GL.NamedBufferStorage(_ID, sizeof(T) * data.Length, data, BufferStorageFlags.None);
					_isAlloccated = true;
				}
			}
		}

		public void Dispose()
		{
			GL.DeleteBuffer(_ID);
			_ID = 0;
		}
	}
}