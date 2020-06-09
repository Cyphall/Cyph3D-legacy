using System;
using System.Collections.Generic;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class Renderbuffer : IDisposable
	{
		private int _ID;
		
		private static HashSet<Renderbuffer> _renderbuffers = new HashSet<Renderbuffer>();

		public static implicit operator int(Renderbuffer renderbuffer) => renderbuffer._ID;

		public Renderbuffer(ivec2 size, RenderbufferStorage internalFormat)
		{
			GL.CreateRenderbuffers(1, out _ID);
			
			GL.NamedRenderbufferStorage(_ID, internalFormat, size.x, size.y);

			_renderbuffers.Add(this);
		}
		
		public void Bind()
		{
			Bind(this);
		}
		
		private static void Bind(int renderbuffer)
		{
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
		}

		public void Dispose()
		{
			GL.DeleteRenderbuffer(_ID);
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