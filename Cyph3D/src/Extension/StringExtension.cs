namespace Cyph3D.Extension
{
	public static class StringExtension
	{
		public static string Remove(this string s, string stringToRemove)
		{
			int index = s.IndexOf(stringToRemove);
			return index == -1 ? s : s.Remove(index, stringToRemove.Length);
		}
	}
}