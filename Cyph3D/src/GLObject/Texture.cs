using System;
using System.Diagnostics;
using Cyph3D.Enumerable;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using StbImageNET;
using ExtTextureFilterAnisotropic = OpenToolkit.Graphics.OpenGL.ExtTextureFilterAnisotropic;

namespace Cyph3D.GLObject
{
	public class Texture : BufferBase
	{
		private ivec2 _size;
		public ivec2 Size => _size;
		private bool _useMipmaps;

		private long _bindlessHandle;
		public long BindlessHandle
		{
			get
			{
				if (!GL.Arb.IsTextureHandleResident(_bindlessHandle))
				{
					GL.Arb.MakeTextureHandleResident(_bindlessHandle);
				}

				return _bindlessHandle;
			}
		}

		public Texture(TextureSetting settings)
		{
			Debug.Assert(settings.Size != default);
			
			_size = settings.Size;
			_useMipmaps = settings.UseMipmaps;
			
			GL.CreateTextures(TextureTarget.Texture2D, 1, out _id);

			int minFintering = (int)(_useMipmaps ? All.LinearMipmapLinear : All.Linear);
			int magFintering = settings.Filtering switch
			{
				TextureFiltering.Linear => (int) All.Linear,
				TextureFiltering.Nearest => (int) All.Nearest,
				_ => throw new ArgumentOutOfRangeException(nameof(settings.Filtering), settings.Filtering, null)
			};
			
			GL.TextureParameter(_id, TextureParameterName.TextureMinFilter, minFintering);
			GL.TextureParameter(_id, TextureParameterName.TextureMagFilter, magFintering);
			if (_useMipmaps)
			{
				GL.GetFloat((GetPName) ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out float anisoCount);
				GL.TextureParameter(_id, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, anisoCount);
			}

			if (settings.IsShadowMap)
			{
				GL.TextureParameter(_id, TextureParameterName.TextureWrapS, (int)All.ClampToBorder);
				GL.TextureParameter(_id, TextureParameterName.TextureWrapT, (int)All.ClampToBorder);
				GL.TextureParameter(_id, TextureParameterName.TextureBorderColor, new []{1f, 1f, 1f, 1f});
			}

			GL.TextureStorage2D(_id, _useMipmaps ? CalculateMipmapCount(_size) : 1, (SizedInternalFormat)settings.InternalFormat, _size.x, _size.y);
			
			_bindlessHandle = GL.Arb.GetTextureHandle(_id);
		}

		private static int CalculateMipmapCount(ivec2 size)
		{
			return (int)Math.Floor(Math.Log2(Math.Max(size.x, size.y))) + 1;
		}

		public void PutData(byte[] data, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage2D(_id, 0, 0, 0, _size.x, _size.y, format, type, data);
			if (_useMipmaps)
				GL.GenerateTextureMipmap(_id);
		}
		
		public void PutData(IntPtr data, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage2D(_id, 0, 0, 0, _size.x, _size.y, format, type, data);
			if (_useMipmaps)
				GL.GenerateTextureMipmap(_id);
		}

		protected override void DeleteBuffer()
		{
			GL.DeleteTexture(_id);
		}

		public void Bind(int unit)
		{
			GL.BindTextureUnit(unit, _id);
		}

		static Texture()
		{
			StbImage.SetFlipVerticallyOnLoad(true);
		}
	}
}