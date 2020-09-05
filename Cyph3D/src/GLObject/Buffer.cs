using System;
using System.Diagnostics;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public unsafe class Buffer<T> : BufferBase where T: unmanaged
	{
		public bool IsMutable { get; }
		public int Size { get; private set; } = -1;

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
				Size = data.Length;
			}
			else
			{
				if (_isAlloccated)
				{
					Debug.Assert(data.Length == Size);
					GL.NamedBufferSubData(_id, IntPtr.Zero, sizeof(T) * data.Length, data);
				}
				else
				{
					GL.NamedBufferStorage(_id, sizeof(T) * data.Length, data, BufferStorageFlags.None);
					Size = data.Length;
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