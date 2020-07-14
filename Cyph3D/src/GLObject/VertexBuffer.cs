namespace Cyph3D.GLObject
{
	public class VertexBuffer<T> : Buffer<T> where T: unmanaged
	{
		public int Stride { get; }
		
		public VertexBuffer(bool mutable, int stride) : base(mutable)
		{
			Stride = stride;
		}
	}
}