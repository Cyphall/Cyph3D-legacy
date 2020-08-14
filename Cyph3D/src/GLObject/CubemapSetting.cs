using Cyph3D.Enumerable;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class CubemapSetting
	{
		public ivec2 Size { get; set; }
		public InternalFormat InternalFormat { get; set; } = InternalFormat.Rgb8;
		public TextureFiltering Filtering { get; set; } = TextureFiltering.Nearest;

		public Cubemap CreateCubemap()
		{
			return new Cubemap(this);
		}
	}
}