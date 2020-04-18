using System;
using System.Collections.Generic;
using GlmSharp;
using OpenGL;

namespace Renderer
{
	public class Framebuffer : IDisposable
	{
		private uint _ID;
		private ivec2 _size;
		
		private static HashSet<Framebuffer> _framebuffers = new HashSet<Framebuffer>();
		
		private static uint CurrentlyBound
		{
			get
			{
				Gl.GetInteger((GetPName)Gl.FRAMEBUFFER_BINDING, out uint value);
				return value;
			}
		}

		public Framebuffer(out Texture texture, ivec2 size, InternalFormat internalFormat, PixelFormat pixelFormat, PixelType type = PixelType.UnsignedByte)
		{
			uint previousFramebuffer = CurrentlyBound;
			
			_size = size;
			_ID = Gl.GenFramebuffer();
			
			Bind();
			
			texture = AddTexture(FramebufferAttachment.ColorAttachment0, internalFormat, pixelFormat, type);

			FramebufferStatus state = Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

			if (state != FramebufferStatus.FramebufferComplete)
			{
				throw new InvalidOperationException($"Error while creating framebuffer: {state}");
			}
			
			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, previousFramebuffer);

			_framebuffers.Add(this);
		}
		
		public Framebuffer(out Renderbuffer renderbuffer, ivec2 size, InternalFormat internalFormat)
		{
			uint previousFramebuffer = CurrentlyBound;
			
			_size = size;
			_ID = Gl.GenFramebuffer();
			
			Bind();
			
			renderbuffer = AddRenderbuffer(FramebufferAttachment.ColorAttachment0, internalFormat);

			FramebufferStatus state = Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

			if (state != FramebufferStatus.FramebufferComplete)
			{
				throw new InvalidOperationException($"Error while creating framebuffer: {state}");
			}
			
			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, previousFramebuffer);

			_framebuffers.Add(this);
		}

		public Texture AddTexture(FramebufferAttachment attachment, InternalFormat internalFormat, PixelFormat pixelFormat, PixelType type = PixelType.UnsignedByte)
		{
			uint previousFramebuffer = CurrentlyBound;
			
			Bind();

			Texture texture = new Texture(_size, internalFormat, pixelFormat, type);
			
			Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2d, texture, 0);
			
			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, previousFramebuffer);

			return texture;
		}

		public Renderbuffer AddRenderbuffer(FramebufferAttachment attachment, InternalFormat internalFormat)
		{
			uint previousFramebuffer = CurrentlyBound;
			
			Bind();
			
			Renderbuffer renderbuffer = new Renderbuffer(_size, internalFormat);
			
			Gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachment, RenderbufferTarget.Renderbuffer, renderbuffer);
			
			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, previousFramebuffer);

			return renderbuffer;
		}

		public void Dispose()
		{
			Gl.DeleteFramebuffers(_ID);
		}
		
		public static void DisposeAll()
		{
			foreach (Framebuffer framebuffer in _framebuffers)
			{
				framebuffer.Dispose();
			}
		}

		public void Bind()
		{
			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, _ID);
		}
	}
}