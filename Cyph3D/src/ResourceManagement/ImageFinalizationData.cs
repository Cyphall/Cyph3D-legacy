using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.ResourceManagement
{
	public class ImageFinalizationData
	{
		public ivec2 Size { get; }
		public InternalFormat InternalFormat { get; }
		public PixelFormat PixelFormat { get; }
		public byte[] TextureData { get; }

		public ImageFinalizationData(ivec2 size, InternalFormat internalFormat, PixelFormat pixelFormat, byte[] textureData)
		{
			Size = size;
			InternalFormat = internalFormat;
			PixelFormat = pixelFormat;
			TextureData = textureData;
		}
	}
}