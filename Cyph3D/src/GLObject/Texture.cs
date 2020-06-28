using System;
using Cyph3D.Enumerable;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using StbImageSharp;

namespace Cyph3D.GLObject
{
	public class Texture : IDisposable
	{
		private int _ID;
		private ivec2 _size;
		
		public static implicit operator int(Texture texture) => texture._ID;

		public Texture(ivec2 size, InternalFormat internalFormat, TextureFiltering filtering)
		{
			_size = size;
			
			GL.CreateTextures(TextureTarget.Texture2D, 1, out _ID);

			int finteringRaw = filtering switch
			{
				TextureFiltering.Linear => (int)All.Linear,
				TextureFiltering.Nearest => (int)All.Nearest,
				_ => throw new ArgumentOutOfRangeException(nameof(filtering), filtering, null)
			};
			
			GL.TextureParameter(_ID, TextureParameterName.TextureMinFilter, finteringRaw);
			GL.TextureParameter(_ID, TextureParameterName.TextureMagFilter, finteringRaw);
			
			GL.TextureStorage2D(_ID, 1, (SizedInternalFormat)internalFormat, size.x, size.y);
		}

		public void PutData(byte[] data, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage2D(_ID, 0, 0, 0, _size.x, _size.y, format, type, data);
		}
		
		public void PutData(IntPtr data, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage2D(_ID, 0, 0, 0, _size.x, _size.y, format, type, data);
		}

		public void Dispose()
		{
			GL.DeleteTexture(_ID);
			_ID = 0;
		}

		public void Bind(int unit)
		{
			GL.BindTextureUnit(unit, _ID);
		}

		static Texture()
		{
			StbImage.stbi_set_flip_vertically_on_load((int)All.True);
		}
	}
}