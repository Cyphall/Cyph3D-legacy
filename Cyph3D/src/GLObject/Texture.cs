using System;
using System.Collections.Generic;
using System.IO;
using Cyph3D.Enumerable;
using Cyph3D.Extension;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using StbImageSharp;

namespace Cyph3D.GLObject
{
	public class Texture : IDisposable
	{
		private int _ID;
		private ivec2 _size;
		
		public bool IsReady { get; private set; }
		
		private static HashSet<Texture> _textures = new HashSet<Texture>();
		
		private static int CurrentlyBound => GL.GetInteger(GetPName.TextureBinding2D);
		
		public static implicit operator int(Texture texture) => texture._ID;

		public Texture(ivec2 size, InternalFormat internalFormat, TextureFiltering filtering = TextureFiltering.Nearest, bool defaultReady = true)
		{
			int previousTexture = CurrentlyBound;
			
			_size = size;
			
			_ID = GL.GenTexture();
			Bind();

			int finteringRaw = filtering switch
			{
				TextureFiltering.Linear => (int)All.Linear,
				TextureFiltering.Nearest => (int)All.Nearest,
				_ => throw new ArgumentOutOfRangeException(nameof(filtering), filtering, null)
			};
			
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, finteringRaw);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, finteringRaw);
			
			GL.TexStorage2D(TextureTarget2d.Texture2D, 1, (SizedInternalFormat)internalFormat, size.x, size.y);

			_textures.Add(this);
			
			Bind(previousTexture);

			IsReady = defaultReady;
		}

		public void PutData(byte[] data, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			int previousTexture = CurrentlyBound;
			Bind();
			GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, _size.x, _size.y, format, type, data);
			Bind(previousTexture);
		}
		
		public void PutData(IntPtr data, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			int previousTexture = CurrentlyBound;
			Bind();
			GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, _size.x, _size.y, format, type, data);
			Bind(previousTexture);
		}

		public void Dispose()
		{
			GL.DeleteTexture(_ID);
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
		
		private static void Bind(int texture)
		{
			GL.BindTexture(TextureTarget.Texture2D, texture);
		}
		
		public static Texture FromFile(string name, bool sRGB = false, bool compressed = false)
		{
			Logger.Info($"Loading texture \"{name}\"");

			ivec2 size;
			ColorComponents comp;
			
			try
			{
				using Stream stream = File.OpenRead($"resources/textures/{name}.png");
				StbImageExt.StbImageInfo(stream, out size, out comp);
			}
			catch (IOException)
			{
				try
				{
					using Stream stream = File.OpenRead($"resources/textures/{name}.jpg");
					StbImageExt.StbImageInfo(stream, out size, out comp);
				}
				catch (IOException)
				{
					throw new IOException($"Unable to load image {name} from disk");
				}
			}

			InternalFormat internalFormat;
			PixelFormat pixelFormat;
			switch (comp)
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
					throw new NotSupportedException($"The colors format {comp} is not supported");
			}
			
			Texture texture = new Texture(size, internalFormat, TextureFiltering.Linear, false);
			
			GL.Finish();
			
			Engine.ThreadPool.Schedule(() => {
				ImageResult image;

				try
				{
					using Stream stream = File.OpenRead($"resources/textures/{name}.png");
					image = ImageResult.FromStream(stream, comp);
				}
				catch (IOException)
				{
					try
					{
						using Stream stream = File.OpenRead($"resources/textures/{name}.jpg");
						image = ImageResult.FromStream(stream, comp);
					}
					catch (IOException)
					{
						throw new IOException($"Unable to load image {name} from disk");
					}
				}
				
				texture.PutData(image.Data, pixelFormat);
				
				GL.Finish();
				
				Logger.Info($"Texture \"{name}\" loaded (id: {texture._ID})");
				
				texture.IsReady = true;
			});
			
			return texture;
		}

		public static bool ExistsOnDisk(string name)
		{
			return File.Exists($"resources/textures/{name}.png") || File.Exists($"resources/textures/{name}.jpg");
		}

		static Texture()
		{
			StbImage.stbi_set_flip_vertically_on_load((int)All.True);
		}
	}
}