using System;
using System.Collections.Generic;
using GlmSharp;
using OpenGL;

namespace Renderer.GLObject
{
	public class Renderbuffer : IDisposable
	{
		private uint _ID;
		
		private static HashSet<Renderbuffer> _renderbuffers = new HashSet<Renderbuffer>();
		
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

			_renderbuffers.Add(this);
			
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

		public void Dispose()
		{
			Gl.DeleteRenderbuffers(_ID);
			_ID = 0;
		}
		
		public static void DisposeAll()
		{
			foreach (Renderbuffer renderbuffer in _renderbuffers)
			{
				renderbuffer.Dispose();
			}
		}
	}
}