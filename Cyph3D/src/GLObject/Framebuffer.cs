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

		public static implicit operator int(Framebuffer framebuffer) => framebuffer._ID;

		public Framebuffer(ivec2 size)
		{
			_size = size;
			GL.CreateFramebuffers(1, out _ID);
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
			return Enum.IsDefined(typeof(DrawBuffersEnum), (int)attachment);
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