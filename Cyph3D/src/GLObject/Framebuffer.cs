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
		private bool _completed;

		private List<DrawBuffersEnum> _drawBuffers = new List<DrawBuffersEnum>();
		private List<FramebufferAttachment> _usedAttachments = new List<FramebufferAttachment>();
		
		private static HashSet<Framebuffer> _framebuffers = new HashSet<Framebuffer>();

		public static implicit operator int(Framebuffer framebuffer) => framebuffer._ID;

		public Framebuffer(ivec2 size)
		{
			_size = size;
			GL.CreateFramebuffers(1, out _ID);

			_framebuffers.Add(this);
		}

		public Framebuffer WithTexture(FramebufferAttachment attachment, InternalFormat internalFormat, out Texture texture, TextureFiltering filtering = TextureFiltering.Nearest)
		{
			if (_usedAttachments.Contains(attachment))
			{
				throw new InvalidOperationException("This framebuffer attachment is alreay used");
			}
				
			texture = new Texture(_size, internalFormat, filtering);
			
			GL.NamedFramebufferTexture(_ID, attachment, texture, 0);
			
			_usedAttachments.Add(attachment);
			
			if (IsDrawBuffer(attachment))
				_drawBuffers.Add((DrawBuffersEnum)attachment);
			
			return this;
		}
		
		public Framebuffer WithTexture(FramebufferAttachment attachment, InternalFormat internalFormat, TextureFiltering filtering = TextureFiltering.Nearest)
		{
			return WithTexture(attachment, internalFormat, out _, filtering);
		}

		public Framebuffer WithRenderbuffer(FramebufferAttachment attachment, RenderbufferStorage internalFormat, out Renderbuffer renderbuffer)
		{
			if (_usedAttachments.Contains(attachment))
			{
				throw new InvalidOperationException("This framebuffer attachment is alreay used");
			}
			
			renderbuffer = new Renderbuffer(_size, internalFormat);
			
			GL.NamedFramebufferRenderbuffer(_ID, attachment, RenderbufferTarget.Renderbuffer, renderbuffer);
			
			if (IsDrawBuffer(attachment))
				_drawBuffers.Add((DrawBuffersEnum)attachment);
			
			return this;
		}
		
		public Framebuffer WithRenderbuffer(FramebufferAttachment attachment, RenderbufferStorage internalFormat)
		{
			return WithRenderbuffer(attachment, internalFormat, out _);
		}

		public Framebuffer Complete()
		{
			GL.NamedFramebufferDrawBuffers(_ID, _drawBuffers.Count, _drawBuffers.ToArray());
			
			FramebufferErrorCode state = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
			if (state != FramebufferErrorCode.FramebufferComplete)
			{
				throw new InvalidOperationException($"Error while creating framebuffer: {state}");
			}

			_completed = true;

			return this;
		}

		private static bool IsDrawBuffer(FramebufferAttachment attachment)
		{
			return attachment >= FramebufferAttachment.ColorAttachment0 && attachment <= FramebufferAttachment.ColorAttachment31;
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
			if (!_completed)
				throw new InvalidOperationException("An incomplete framebuffer has been bound.");
			
			BindUnsafe(this);
		}

		private static void BindUnsafe(int framebuffer)
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
		}
	}
}