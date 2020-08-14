using System;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public partial class Framebuffer
	{
		public Framebuffer SetTexture(FramebufferAttachment attachment, TextureSetting textureSetting, out Texture texture)
		{
			textureSetting.Size = _size;
			texture = textureSetting.CreateTexture();
			
			return SetTexture(attachment, texture);
		}
		
		public Framebuffer SetTexture(FramebufferAttachment attachment, Texture texture)
		{
			if (texture != null && texture.Size != _size)
			{
				throw new InvalidOperationException("The texture has a different size than the framebuffer size");
			}
			
			GL.NamedFramebufferTexture(_ID, attachment, texture ?? 0, 0);

			if (IsDrawBuffer(attachment))
			{
				if (texture != null)
				{
					_drawBuffers.Add((DrawBuffersEnum)attachment);
				}
				else
				{
					_drawBuffers.Remove((DrawBuffersEnum)attachment);
				}
			}
			
			CheckState();
			
			return this;
		}

		public Framebuffer SetCubemap(FramebufferAttachment attachment, CubemapSetting cubemapSetting, out Cubemap cubemap, int? face = null)
		{
			cubemapSetting.Size = _size;
			cubemap = cubemapSetting.CreateCubemap();

			return SetCubemap(attachment, cubemap, face);
		}
		
		public Framebuffer SetCubemap(FramebufferAttachment attachment, Cubemap cubemap, int? face = null)
		{
			if (cubemap != null && cubemap.Size != _size)
			{
				throw new InvalidOperationException("The texture has a different size than the framebuffer size");
			}

			if (face != null)
			{
				GL.NamedFramebufferTextureLayer(_ID, attachment, cubemap ?? 0, 0, face.Value);
			}
			else
			{
				GL.NamedFramebufferTexture(_ID, attachment, cubemap ?? 0, 0);
			}

			if (IsDrawBuffer(attachment))
			{
				if (cubemap != null)
				{
					_drawBuffers.Add((DrawBuffersEnum)attachment);
				}
				else
				{
					_drawBuffers.Remove((DrawBuffersEnum)attachment);
				}
			}
			
			CheckState();
			
			return this;
		}

		public Framebuffer SetRenderbuffer(FramebufferAttachment attachment, RenderbufferStorage internalFormat)
		{
			Renderbuffer renderbuffer = new Renderbuffer(_size, internalFormat);
			
			GL.NamedFramebufferRenderbuffer(_ID, attachment, RenderbufferTarget.Renderbuffer, renderbuffer);
			
			if (IsDrawBuffer(attachment))
				_drawBuffers.Add((DrawBuffersEnum)attachment);
			
			CheckState();
			
			return this;
		}
	}
}