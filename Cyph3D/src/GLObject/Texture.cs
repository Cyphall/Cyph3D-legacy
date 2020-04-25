using System;
using System.Collections.Generic;
using System.IO;
using GlmSharp;
using OpenGL;
using Renderer.Misc;
using StbImageSharp;

namespace Renderer.GLObject
{
	public class Texture : IDisposable
	{
		private uint _ID;
		private ivec2 _size;
		
		private static HashSet<Texture> _textures = new HashSet<Texture>();

		public Texture(ivec2 size, InternalFormat internalFormat, Filtering filtering = Filtering.Nearest)
		{
			_size = size;
			
			_ID = Gl.GenTexture();
			Bind();

			int finteringRaw = filtering switch
			{
				Filtering.Linear => Gl.LINEAR,
				Filtering.Nearest => Gl.NEAREST,
				_ => throw new ArgumentOutOfRangeException(nameof(filtering), filtering, null)
			};
			
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, finteringRaw);
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, finteringRaw);
			
			Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, size.x, size.y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

			_textures.Add(this);
		}

		public static implicit operator uint(Texture t)
		{
			return t._ID;
		}

		public void PutData(byte[] data, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			Gl.TexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, _size.x, _size.y, format, type, data);
		}

		public void Dispose()
		{
			Gl.DeleteTextures(_ID);
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
			Gl.BindTexture(TextureTarget.Texture2d, _ID);
		}
		
		public static Texture FromFile(string name, bool sRGB = false)
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
					internalFormat = sRGB ? InternalFormat.Srgb8 : InternalFormat.Rgb;
					break;
				case ColorComponents.GreyAlpha:
					pixelFormat = PixelFormat.LuminanceAlpha;
					internalFormat = sRGB ? InternalFormat.Srgb8Alpha8 : InternalFormat.Rgba;
					break;
				case ColorComponents.RedGreenBlue:
					pixelFormat = PixelFormat.Rgb;
					internalFormat = sRGB ? InternalFormat.Srgb8 : InternalFormat.Rgb;
					break;
				case ColorComponents.RedGreenBlueAlpha:
					pixelFormat = PixelFormat.Rgba;
					internalFormat = sRGB ? InternalFormat.Srgb8Alpha8 : InternalFormat.Rgba;
					break;
				default:
					throw new NotSupportedException($"The colors format {image.Comp} is not supported");
			}

			ivec2 size = new ivec2(image.Width, image.Height);
			
			Texture texture = new Texture(size, internalFormat, Filtering.Linear);
			texture.PutData(image.Data, pixelFormat);

			Logger.Info($"Texture \"{name}\" loaded");
			
			return texture;
		}

		static Texture()
		{
			StbImage.stbi_set_flip_vertically_on_load(Gl.TRUE);
		}

		public enum Filtering
		{
			Linear,
			Nearest
		}
	}
}