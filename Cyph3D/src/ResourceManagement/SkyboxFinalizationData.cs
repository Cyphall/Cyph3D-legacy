using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.ResourceManagement
{
	public class SkyboxFinalizationData
	{
		public ivec2 Size { get; }
		public InternalFormat InternalFormat { get; }
		public PixelFormat PixelFormat { get; }
		public byte[][] CubemapData { get; }

		public SkyboxFinalizationData(ivec2 size, InternalFormat internalFormat, PixelFormat pixelFormat, byte[][] cubemapData)
		{
			Size = size;
			InternalFormat = internalFormat;
			PixelFormat = pixelFormat;
			CubemapData = cubemapData;
		}
	}
}