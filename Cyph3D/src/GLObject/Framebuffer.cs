﻿using System;
using System.Collections.Generic;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public partial class Framebuffer : BufferBase
	{
		private ivec2 _size;

		private List<DrawBuffersEnum> _drawBuffers = new List<DrawBuffersEnum>();

		public Framebuffer(ivec2 size)
		{
			GL.CreateFramebuffers(1, out _id);
			_size = size;
		}

		private void CheckState()
		{
			GL.NamedFramebufferDrawBuffers(_id, _drawBuffers.Count, _drawBuffers.ToArray());
			GL.NamedFramebufferReadBuffer(_id, ReadBufferMode.None);
			
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

		protected override void DeleteBuffer()
		{
			GL.DeleteFramebuffer(_id);
		}

		public void Bind()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, _id);
		}
	}
}