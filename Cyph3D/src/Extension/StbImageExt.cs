using System.IO;
using GlmSharp;
using StbImageSharp;

namespace Cyph3D.Extension
{
	public static unsafe class StbImageExt
	{
		public static void StbImageInfo(Stream stream, out ivec2 size, out ColorComponents colorComponents)
		{
			int width;
			int height;
			int comp;

			StbImage.stbi__info_main(new StbImage.stbi__context(stream), &width, &height, &comp);

			size = new ivec2(width, height);
			colorComponents = (ColorComponents) comp;
		}
	}
}