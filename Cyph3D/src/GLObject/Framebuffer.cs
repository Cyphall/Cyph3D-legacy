using System;
using System.Collections.Generic;
using Cyph3D.Enumerable;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class Framebuffer : IDisposable
	{
		private int _ID;
		private ivec2 _size;
		
		private static HashSet<Framebuffer> _framebuffers = new HashSet<Framebuffer>();
		
		private static int CurrentlyBound => GL.GetInteger(GetPName.FramebufferBinding);

		public static implicit operator int(Framebuffer framebuffer) => framebuffer._ID;

		public Framebuffer(out Texture texture, ivec2 size, InternalFormat internalFormat)
		{
			int previousFramebuffer = CurrentlyBound;
			
			_size = size;
			_ID = GL.GenFramebuffer();
			
			Bind();
			
			texture = AddTexture(FramebufferAttachment.ColorAttachment0, internalFormat);

			CheckStatus();
			
			Bind(previousFramebuffer);

			_framebuffers.Add(this);
		}
		
		public Framebuffer(out Renderbuffer renderbuffer, ivec2 size, RenderbufferStorage internalFormat)
		{
			int previousFramebuffer = CurrentlyBound;
			
			_size = size;
			_ID = GL.GenFramebuffer();
			
			Bind();
			
			renderbuffer = AddRenderbuffer(FramebufferAttachment.ColorAttachment0, internalFormat);

			CheckStatus();
			
			Bind(previousFramebuffer);

			_framebuffers.Add(this);
		}

		public Texture AddTexture(FramebufferAttachment attachment, InternalFormat internalFormat, TextureFiltering filtering = TextureFiltering.Nearest)
		{
			int previousFramebuffer = CurrentlyBound;
			Bind();

			Texture texture = new Texture(_size, internalFormat, filtering);
			
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2D, texture, 0);
			
			CheckStatus();
			
			Bind(previousFramebuffer);

			return texture;
		}

		public Renderbuffer AddRenderbuffer(FramebufferAttachment attachment, RenderbufferStorage internalFormat)
		{
			int previousFramebuffer = CurrentlyBound;
			Bind();
			
			Renderbuffer renderbuffer = new Renderbuffer(_size, internalFormat);
			
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachment, RenderbufferTarget.Renderbuffer, renderbuffer);
			
			CheckStatus();
			
			Bind(previousFramebuffer);

			return renderbuffer;
		}

		public void SetDrawBuffers(params DrawBuffersEnum[] buffers)
		{
			int previousFramebuffer = CurrentlyBound;
			Bind();
			
			GL.DrawBuffers(buffers.Length, buffers);
			
			Bind(previousFramebuffer);
		}

		private void CheckStatus()
		{
			int previousFramebuffer = CurrentlyBound;
			Bind();
			
			FramebufferErrorCode state = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

			if (state != FramebufferErrorCode.FramebufferComplete)
			{
				throw new InvalidOperationException($"Error while creating framebuffer: {state}");
			}
			
			Bind(previousFramebuffer);
		}

		public void Dispose()
		{
			GL.DeleteFramebuffer(_ID);
			_ID = 0;
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
			Bind(this);
		}

		private static void Bind(int framebuffer)
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
		}
	}
}