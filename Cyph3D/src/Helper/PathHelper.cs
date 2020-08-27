namespace Cyph3D.Helper
{
	public static class PathHelper
	{
		public static string GetPathWithoutExtension(string path)
		{
			int i = path.LastIndexOf(".");
			
			return i == -1 ? path : path.Remove(i);
		}
	}
}