namespace Cyph3D.Helper
{
	public static unsafe class Stride
	{
		public static int Get<T>(int count) where T: unmanaged
		{
			return sizeof(T) * count;
		}
	}
}