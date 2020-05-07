using System;
using System.Collections.Generic;
using System.IO;
using GlmSharp;
using OpenGL;
using Renderer.Enum;
using Renderer.Misc;
using StbImageSharp;

namespace Renderer.GLObject
{
	public class Texture : IDisposable
	{
		private uint _ID;
		private ivec2 _size;
		
		private static HashSet<Texture> _textures = new HashSet<Texture>();
		
		private static uint CurrentlyBound
		{
			get
			{
				Gl.GetInteger(GetPName.TextureBinding2d, out uint value);
				return value;
			}
		}
		
		public static implicit operator uint(Texture texture) => texture._ID;

		public Texture(ivec2 size, InternalFormat internalFormat, TextureFiltering filtering = TextureFiltering.Nearest)
		{
			uint previousTexture = CurrentlyBound;
			
			_size = size;
			
			_ID = Gl.GenTexture();
			Bind();

			int finteringRaw = filtering switch
			{
				TextureFiltering.Linear => Gl.LINEAR,
				TextureFiltering.Nearest => Gl.NEAREST,
				_ => throw new ArgumentOutOfRangeException(nameof(filtering), filtering, null)
			};
			
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, finteringRaw);
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, finteringRaw);
			
			Gl.TexStorage2D(TextureTarget.Texture2d, 1, internalFormat, size.x, size.y);

			_textures.Add(this);
			
			Bind(previousTexture);
		}

		public void PutData(byte[] data, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			uint previousTexture = CurrentlyBound;
			Bind();
			Gl.TexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, _size.x, _size.y, format, type, data);
			Bind(previousTexture);
		}

		public void Dispose()
		{
			Gl.DeleteTextures(_ID);
			_ID = 0;
		}

		public static void DisposeAll()
		{
			foreach (Texture texture in _textures)
			{
				texture.Dispose();
			}
		}

		public void Bind()
		{
			Bind(this);
		}
		
		private static void Bind(uint texture)
		{
			Gl.BindTexture(TextureTarget.Texture2d, texture);
		}
		
		public static Texture FromFile(string name, bool sRGB = false, bool compressed = false)
		{
			Logger.Info($"Loading texture \"{name}\"");
			
			ImageResult image;

			try
			{
				using Stream stream = File.OpenRead($"resources/textures/{name}.png");
				image = ImageResult.FromStream(stream);
			}
			catch (IOException)
			{
				try
				{
					using Stream stream = File.OpenRead($"resources/textures/{name}.jpg");
					image = ImageResult.FromStream(stream);
				}
				catch (IOException)
				{
					Logger.Info($"Texture \"{name}\" doesn't exists, skipping...");
					return null;
				}
			}

			InternalFormat internalFormat;
			PixelFormat pixelFormat;
			switch (image.Comp)
			{
				case ColorComponents.Grey:
					pixelFormat = PixelFormat.Luminance;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbS3tcDxt1Ext : InternalFormat.CompressedRedRgtc1;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8 : InternalFormat.Red;
					break;
				case ColorComponents.GreyAlpha:
					pixelFormat = PixelFormat.LuminanceAlpha;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8Alpha8 : InternalFormat.Rgba8;
					break;
				case ColorComponents.RedGreenBlue:
					pixelFormat = PixelFormat.Rgb;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbS3tcDxt1Ext : InternalFormat.CompressedRgbS3tcDxt1Ext;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8 : InternalFormat.Rgb8;
					break;
				case ColorComponents.RedGreenBlueAlpha:
					pixelFormat = PixelFormat.Rgba;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8Alpha8 : InternalFormat.Rgba8;
					break;
				default:
					throw new NotSupportedException($"The colors format {image.Comp} is not supported");
			}

			ivec2 size = new ivec2(image.Width, image.Height);
			
			Texture texture = new Texture(size, internalFormat, TextureFiltering.Linear);
			texture.PutData(image.Data, pixelFormat);

			Logger.Info($"Texture \"{name}\" loaded");
			
			return texture;
		}

		static Texture()
		{
			StbImage.stbi_set_flip_vertically_on_load(Gl.TRUE);
		}
	}
}