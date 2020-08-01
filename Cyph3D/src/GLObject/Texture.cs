using System;
using System.Diagnostics;
using Cyph3D.Enumerable;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using StbImageNET;
using ExtTextureFilterAnisotropic = OpenToolkit.Graphics.OpenGL.ExtTextureFilterAnisotropic;

namespace Cyph3D.GLObject
{
	public class Texture : IDisposable
	{
		private int _ID;
		private ivec2 _size;
		private bool _useMipmaps;
		
		public static implicit operator int(Texture texture) => texture._ID;

		public Texture(TextureSetting settings)
		{
			Debug.Assert(settings.Size != default);
			
			_size = settings.Size;
			_useMipmaps = settings.UseMipmaps;
			
			GL.CreateTextures(TextureTarget.Texture2D, 1, out _ID);

			int minFintering = (int)(_useMipmaps ? All.LinearMipmapLinear : All.Linear);
			int magFintering = settings.Filtering switch
			{
				TextureFiltering.Linear => (int) All.Linear,
				TextureFiltering.Nearest => (int) All.Nearest,
				_ => throw new ArgumentOutOfRangeException(nameof(settings.Filtering), settings.Filtering, null)
			};
			
			GL.TextureParameter(_ID, TextureParameterName.TextureMinFilter, minFintering);
			GL.TextureParameter(_ID, TextureParameterName.TextureMagFilter, magFintering);
			if (_useMipmaps)
			{
				GL.GetFloat((GetPName) ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out float anisoCount);
				GL.TextureParameter(_ID, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, anisoCount);
			}

			GL.TextureStorage2D(_ID, _useMipmaps ? CalculateMipmapCount(_size) : 1, (SizedInternalFormat)settings.InternalFormat, _size.x, _size.y);
		}

		private static int CalculateMipmapCount(ivec2 size)
		{
			return (int)Math.Floor(Math.Log2(Math.Max(size.x, size.y))) + 1;
		}

		public void PutData(byte[] data, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage2D(_ID, 0, 0, 0, _size.x, _size.y, format, type, data);
			if (_useMipmaps)
				GL.GenerateTextureMipmap(_ID);
		}
		
		public void PutData(IntPtr data, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage2D(_ID, 0, 0, 0, _size.x, _size.y, format, type, data);
			if (_useMipmaps)
				GL.GenerateTextureMipmap(_ID);
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
			StbImage.SetFlipVerticallyOnLoad(true);
		}
	}
}