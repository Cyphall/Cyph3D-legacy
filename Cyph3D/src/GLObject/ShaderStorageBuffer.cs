using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class ShaderStorageBuffer<T> : Buffer<T> where T: unmanaged
	{
		public void Bind(int bindingPoint)
		{
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, _id);
		}

		public ShaderStorageBuffer() : base(true)
		{
		}
	}
}