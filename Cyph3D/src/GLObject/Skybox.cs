using Cyph3D.Enumerable;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class Skybox : Cubemap
	{
		public string Name { get; set; }
		public float Rotation { get; set; }
		
		public Skybox(ivec2 size, string name, InternalFormat internalFormat, TextureFiltering filtering = TextureFiltering.Nearest) : base(size, internalFormat, filtering)
		{
			Name = name;
		}
	}
}