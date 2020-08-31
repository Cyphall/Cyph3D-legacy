using Cyph3D.GLObject;

namespace Cyph3D.Rendering
{
	public interface IPostProcessPass
	{
		public Texture Render(Texture currentRenderResult, Texture renderRaw, Texture depth);
	}
}