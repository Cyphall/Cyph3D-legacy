namespace Cyph3D.StateManagement
{
	public class GlViewport
	{
		public int X { get; }
		public int Y { get; }
		public int Width { get; }
		public int Height { get; }

		public GlViewport(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
	}
}