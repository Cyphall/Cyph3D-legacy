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

		public Renderbuffer(ivec2 size, InternalFormat internalFormat)
		{
			uint previousBuffer = CurrentlyBound;
			
			_ID = Gl.GenRenderbuffer();
			
			Bind();
			
			Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFormat, size.x, size.y);
			
			Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, previousBuffer);
		}
		
		public static implicit operator uint(Renderbuffer t)
		{
			return t._ID;
		}

		public void Bind()
		{
			Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _ID);
		}
	}
}