using System;
using System.Collections.Generic;
using System.IO;
using Cyph3D.Enumerable;
using Cyph3D.Extension;
using Cyph3D.Misc;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using StbImageSharp;

namespace Cyph3D.GLObject
{
	public class Cubemap : IDisposable
	{
		private int _ID;
		private ivec2 _size;
		
		private static HashSet<Cubemap> _cubemaps = new HashSet<Cubemap>();
		
		public Cubemap(ivec2 size, InternalFormat internalFormat, TextureFiltering filtering = TextureFiltering.Nearest, bool defaultReady = true)
		{
			_size = size;
			
			GL.CreateTextures(TextureTarget.TextureCubeMap, 1, out _ID);

			int finteringRaw = filtering switch
			{
				TextureFiltering.Linear => (int)All.Linear,
				TextureFiltering.Nearest => (int)All.Nearest,
				_ => throw new ArgumentOutOfRangeException(nameof(filtering), filtering, null)
			};
			
			GL.TextureParameter(_ID, TextureParameterName.TextureMinFilter, finteringRaw);
			GL.TextureParameter(_ID, TextureParameterName.TextureMagFilter, finteringRaw);
			GL.TextureParameter(_ID, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
			GL.TextureParameter(_ID, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
			GL.TextureParameter(_ID, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);


			GL.TextureStorage2D(_ID, 1, (SizedInternalFormat)internalFormat, size.x, size.y);
			_cubemaps.Add(this);
		}
		
		public void PutData(byte[] data, int face, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage3D(_ID, 0, 0, 0, face, _size.x, _size.y, 1, format, type, data);
		}
		
		public void PutData(IntPtr data, int face, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage3D(_ID, 0, 0, 0, face, _size.x, _size.y, 1, format, type, data);
		}
		
		public void Dispose()
		{
			GL.DeleteTexture(_ID);
			_ID = 0;
		}
		
		public static void DisposeAll()
		{
			foreach (Cubemap cubemap in _cubemaps)
			{
				cubemap.Dispose();
			}
		}
		
		public void Bind(int unit)
		{
			GL.BindTextureUnit(unit, _ID);
		}
		
		public static Cubemap FromFiles(string name, bool sRGB = false, bool compressed = false)
		{
			Logger.Info($"Loading texture \"{name}\"");
			
			ivec2 size;
			ColorComponents comp;
			
			try
			{
				using Stream stream = File.OpenRead(string.Format($"resources/materials/{name}", "_front"));
				StbImageExt.StbImageInfo(stream, out size, out comp);
			}
			catch (IOException)
			{
				throw new IOException($"Unable to load image {string.Format(name, "_front")} from disk");
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

			string[] suffixes =
			{
				"_right",
				"_left",
				"_down",
				"_up",
				"_front",
				"_back"
			};
			
			Cubemap cubemap = new Cubemap(size, internalFormat, TextureFiltering.Linear, false);

			// 0 = positive x face
			// 1 = negative x face
			// 2 = positive y face
			// 3 = negative y face
			// 4 = positive z face
			// 5 = negative z face
			
			for (int i = 0; i < 6; i++)
			{
				ImageResult image;
			
				try
				{
					using Stream stream = File.OpenRead(string.Format($"resources/materials/{name}", suffixes[i]));
					image = ImageResult.FromStream(stream);
				}
				catch (IOException)
				{
					throw new IOException($"Unable to load image {string.Format(name, suffixes[i])} from disk");
				}
				
				cubemap.PutData(image.Data, i, pixelFormat);
			}
			
			return cubemap;
		}
	}
}