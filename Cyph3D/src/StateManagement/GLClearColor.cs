namespace Cyph3D.StateManagement
{
	public class GLClearColor
	{
		public float R { get; }
		public float G { get; }
		public float B { get; }
		public float A { get; }

		public GLClearColor(float r, float g, float b, float a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}
	}
}