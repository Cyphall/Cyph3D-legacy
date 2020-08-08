using System.Collections.Generic;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.ResourceManagement
{
	public class ShaderProgramEqualityComparer : IEqualityComparer<Dictionary<ShaderType, string[]>>
	{
		public bool Equals(Dictionary<ShaderType, string[]> first, Dictionary<ShaderType, string[]> second)
		{
			if (ReferenceEquals(first, second))
				return true;
			if (ReferenceEquals(first, null))
				return false;
			if (ReferenceEquals(second, null))
				return false;
			
			foreach (ShaderType type in first.Keys)
			{
				if (!second.ContainsKey(type))
					return false;

				if (!Equals(first[type], second[type]))
					return false;
			}

			return true;
		}

		private bool Equals(string[] first, string[] second)
		{
			if (ReferenceEquals(first, second))
				return true;
			if (ReferenceEquals(first, null))
				return false;
			if (ReferenceEquals(second, null))
				return false;
				
			if (first.Length != second.Length)
				return false;

			for (int i = 0; i < first.Length; i++)
			{
				if (first[i] != second[i])
					return false;
			}

			return true;
		}

		public int GetHashCode(Dictionary<ShaderType, string[]> obj)
		{
			int result = 17;

			foreach ((ShaderType type, string[] files) in obj)
			{
				result = result * 23 + (int)type;

				for (int i = 0; i < files.Length; i++)
				{
					result = result * 23 + files[i].GetHashCode();
				}
			}
			
			return result;
		}
	}
}