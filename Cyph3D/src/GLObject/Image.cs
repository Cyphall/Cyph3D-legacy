using System;
using System.IO;
using Cyph3D.Enumerable;
using Cyph3D.Helper;
using Cyph3D.ResourceManagement;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using StbImageNET;

namespace Cyph3D.GLObject
{
	public class Image : IDisposable
	{
		public string Name { get; set; }
		public Texture ResourceData { get; private set; }
		public bool IsResourceReady { get; private set; }
		
		public Image(string name)
		{
			Name = name;
		}
		
		public void FinalizeLoading(ImageFinalizationData data)
		{
			TextureCreateInfo createInfo = new TextureCreateInfo
			{
				Size = data.Size,
				InternalFormat = data.InternalFormat,
				Filtering = TextureFiltering.Linear,
				UseMipmaps = true
			};
			
			ResourceData = new Texture(createInfo);
			
			ResourceData.PutData(data.TextureData, data.PixelFormat);

			IsResourceReady = true;
		}

		public void Dispose()
		{
			ResourceData?.Dispose();
			ResourceData = null;
			IsResourceReady = false;
		}

		public static ImageFinalizationData LoadFromFile(string path, bool sRGB, bool compressed)
		{
			byte[] textureData;

			ivec2 size = new ivec2();
			Components comp;
				
			try
			{
				textureData = StbImage.Load(path, out size.x, out size.y, out comp);
			}
			catch (IOException)
			{
				throw new IOException($"Unable to load image {path} from disk");
			}
				
			(InternalFormat internalFormat, PixelFormat pixelFormat) = TextureHelper.GetTextureSetting((int) comp, compressed, sRGB);

			return new ImageFinalizationData(
				size,
				internalFormat,
				pixelFormat,
				textureData
			);
		}
	}
}