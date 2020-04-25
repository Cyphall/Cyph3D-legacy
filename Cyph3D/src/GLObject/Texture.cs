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
		private InternalFormat _internalFormat;
		private PixelFormat _pixelFormat;
		private PixelType _pixelType;
		
		private static Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
		
		private static uint CurrentlyBound
		{
			get
			{
				Gl.GetInteger(GetPName.TextureBinding2d, out uint value);
				return value;
			}
		}

		private static (PixelFormat, PixelType) ResolveFormat(InternalFormat format)
		{
			return format switch
			{
				InternalFormat.Rgba16f => (PixelFormat.Rgba, PixelType.HalfFloat),
				InternalFormat.Rgb16f => (PixelFormat.Rgb, PixelType.HalfFloat),
				InternalFormat.Rgba => (PixelFormat.Rgba, PixelType.UnsignedByte),
				InternalFormat.Rgb => (PixelFormat.Rgb, PixelType.UnsignedByte),
				InternalFormat.SrgbAlpha => (PixelFormat.Rgba, PixelType.UnsignedByte),
				InternalFormat.Srgb => (PixelFormat.Rgb, PixelType.UnsignedByte),
				(InternalFormat)34837 => (PixelFormat.Rgb, PixelType.Float),
				_ => throw new NotImplementedException($"{format} format resolution is not yet implemented.")
			};
		}

		static Texture()
		{
			StbImage.stbi_set_flip_vertically_on_load(Gl.TRUE);
		}
		
		private Texture(string name, InternalFormat internalFormat)
		{
			uint previousTexture = CurrentlyBound;
			
			ImageResult image;

			try
			{
				using Stream stream = File.OpenRead($"resources/textures/{name}.png");
				image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
			}
			catch (IOException)
			{
				try
				{
					using Stream stream = File.OpenRead($"resources/textures/{name}.jpg");
					image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
				}
				catch (IOException)
				{
					Logger.Error($"Unable to open jpg or png for texture \"{name}\"");
					throw;
				}
			}

			_size = new ivec2(image.Width, image.Height);
			_internalFormat = internalFormat;
			_pixelFormat = PixelFormat.Rgb;
			_pixelType = PixelType.UnsignedByte;
			
			_ID = Gl.GenTexture();

			if (_ID == 0)
			{
				throw new InvalidOperationException($"Unable to create texture \"{name}\"");
			}

			Bind();

			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.LINEAR);
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.LINEAR);

			Gl.TexImage2D(TextureTarget.Texture2d, 0, _internalFormat, image.Width, image.Height, 0, _pixelFormat, _pixelType, image.Data);
			
			Gl.BindTexture(TextureTarget.Texture2d, previousTexture);
		}

		public Texture(ivec2 size, InternalFormat internalFormat)
		{
			_size = size;
			_internalFormat = internalFormat;
			(_pixelFormat, _pixelType) = ResolveFormat(internalFormat);
			
			_ID = Gl.GenTexture();
			Gl.BindTexture(TextureTarget.Texture2d, _ID);
			
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);
			Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
			
			Gl.TexImage2D(TextureTarget.Texture2d, 0, _internalFormat, size.x, size.y, 0, _pixelFormat, _pixelType, IntPtr.Zero);
		}

		public static implicit operator uint(Texture t)
		{
			return t._ID;
		}
		
		public static Texture Get(string name, InternalFormat format)
		{
			if (!_textures.ContainsKey(name))
			{
				Logger.Info($"Loading texture \"{name}\"");
				_textures.Add(name, new Texture(name, format));
				Logger.Info($"Texture \"{name}\" loaded");
			}

			return _textures[name];
		}

		public void PutData(byte[] data)
		{
			Gl.TexSubImage2D(TextureTarget.Texture2d, 0, 0, 0, _size.x, _size.y, _pixelFormat, _pixelType, data);
		}

		public void Dispose()
		{
			Gl.DeleteTextures(_ID);
		}

		public static void DisposeAll()
		{
			foreach (Texture texture in _textures.Values)
			{
				texture.Dispose();
			}
		}

		public void Bind()
		{
			Gl.BindTexture(TextureTarget.Texture2d, _ID);
		}
	}
}