using GlmSharp;
using OpenGL;

namespace Renderer.GLObject
{
	public class Renderbuffer
	{
		private uint _ID;
		
		private static uint CurrentlyBound
		{
			get
			{
				Gl.GetInteger(GetPName.RenderbufferBinding, out uint value);
				return value;
			}
		}
		
		public static implicit operator uint(Renderbuffer renderbuffer) => renderbuffer._ID;

		public Renderbuffer(ivec2 size, InternalFormat internalFormat)
		{
			uint previousBuffer = CurrentlyBound;
			
			_ID = Gl.GenRenderbuffer();
			
			Bind();
			
			Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFormat, size.x, size.y);
			
			Bind(previousBuffer);
		}
		
		public void Bind()
		{
			Bind(this);
		}
		
		private static void Bind(uint renderbuffer)
		{
			Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
		}
	}
}