using System;
using System.Collections.Generic;
using Cyph3D.Misc;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class ShaderStorageBuffer<T> : Buffer<T> where T: unmanaged
	{
		public void Bind(int bindingPoint)
		{
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, _ID);
		}

		public ShaderStorageBuffer() : base(true)
		{
		}
	}
}