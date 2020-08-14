using System;
using System.Collections.Generic;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public partial class Framebuffer : IDisposable
	{
		private int _ID;
		private ivec2 _size;

		private List<DrawBuffersEnum> _drawBuffers = new List<DrawBuffersEnum>();
		private List<FramebufferAttachment> _usedAttachments = new List<FramebufferAttachment>();

		public static implicit operator int(Framebuffer framebuffer) => framebuffer._ID;

		public Framebuffer(ivec2 size)
		{
			_size = size;
			GL.CreateFramebuffers(1, out _ID);
		}

		public Framebuffer SetTexture(FramebufferAttachment attachment, TextureSetting textureSetting, out Texture texture)
		{
			if (_usedAttachments.Contains(attachment))
			{
				throw new InvalidOperationException("This framebuffer attachment is alreay used");
			}

			textureSetting.Size = _size;
			texture = textureSetting.CreateTexture();
			
			GL.NamedFramebufferTexture(_ID, attachment, texture, 0);
			
			_usedAttachments.Add(attachment);
			
			if (IsDrawBuffer(attachment))
				_drawBuffers.Add((DrawBuffersEnum)attachment);
			
			CheckState();
			
			return this;
		}

		public Framebuffer SetRenderbuffer(FramebufferAttachment attachment, RenderbufferStorage internalFormat)
		{
			if (_usedAttachments.Contains(attachment))
			{
				throw new InvalidOperationException("This framebuffer attachment is alreay used");
			}
			
			Renderbuffer renderbuffer = new Renderbuffer(_size, internalFormat);
			
			GL.NamedFramebufferRenderbuffer(_ID, attachment, RenderbufferTarget.Renderbuffer, renderbuffer);
			
			if (IsDrawBuffer(attachment))
				_drawBuffers.Add((DrawBuffersEnum)attachment);
			
			CheckState();
			
			return this;
		}

		private void CheckState()
		{
			GL.NamedFramebufferDrawBuffers(_ID, _drawBuffers.Count, _drawBuffers.ToArray());
			GL.NamedFramebufferReadBuffer(_ID, ReadBufferMode.None);
			
			FramebufferErrorCode state = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
			if (state != FramebufferErrorCode.FramebufferComplete)
			{
				throw new InvalidOperationException($"Error while creating framebuffer: {state}");
			}
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

		public void Bind()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, this);
		}
	}
}