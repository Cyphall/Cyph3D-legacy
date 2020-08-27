using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class Renderbuffer : BufferBase
	{
		public Renderbuffer(ivec2 size, RenderbufferStorage internalFormat)
		{
			GL.CreateRenderbuffers(1, out _id);
			
			GL.NamedRenderbufferStorage(_id, internalFormat, size.x, size.y);
		}
		
		public void Bind()
		{
			Bind(_id);
		}
		
		private static void Bind(int renderbuffer)
		{
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
		}

		protected override void DeleteBuffer()
		{
			GL.DeleteRenderbuffer(_id);
		}
	}
}