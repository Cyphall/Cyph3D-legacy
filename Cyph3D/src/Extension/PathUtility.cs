namespace Cyph3D.Extension
{
	public static class PathUtility
	{
		public static string GetPathWithoutExtension(string path)
		{
			int i = path.LastIndexOf(".");
			
			return i == -1 ? path : path.Remove(i);
		}
	}
}