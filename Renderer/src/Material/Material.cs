using System;
using System.Collections.Generic;
using GlmSharp;

namespace Renderer
{
	public abstract class Material
	{
		private static Dictionary<string, Material> _materials = new Dictionary<string, Material>();
		
		public static Material Get(string name, Func<Material> creationMethod)
		{
			if (!_materials.ContainsKey(name))
			{
				Logger.Info($"Loading material \"{name}\"");
				_materials.Add(name, creationMethod.Invoke());
				Logger.Info($"Material \"{name}\" loaded");
			}

			return _materials[name];
		}
		public abstract void Bind(mat4 model, mat4 view, mat4 projection, vec3 cameraPos, vec3 lightPos);
	}
}