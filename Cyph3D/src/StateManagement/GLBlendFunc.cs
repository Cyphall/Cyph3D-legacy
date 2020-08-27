using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.StateManagement
{
	public class GLBlendFunc
	{
		public BlendingFactor SFactor { get; }
		public BlendingFactor DFactor { get; }

		public GLBlendFunc(BlendingFactor sFactor, BlendingFactor dFactor)
		{
			SFactor = sFactor;
			DFactor = dFactor;
		}
	}
}