using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public abstract class BufferBase
	{
		protected int _ID;
		
		public static implicit operator int(BufferBase buffer) => buffer._ID;

		protected BufferBase()
		{
			GL.CreateBuffers(1, out _ID);
		}
	}
}