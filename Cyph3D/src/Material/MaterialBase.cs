using System;
using System.Collections.Generic;
using GlmSharp;
using Renderer.Misc;

namespace Renderer.Material
{
	public abstract class MaterialBase
	{
		private static Dictionary<string, MaterialBase> _materials = new Dictionary<string, MaterialBase>();
		
		public static MaterialBase Get(string name, Func<MaterialBase> creationMethod)
		{
			if (!_materials.ContainsKey(name))
			{
				Logger.Info($"Loading material \"{name}\"");
				_materials.Add(name, creationMethod.Invoke());
				Logger.Info($"Material \"{name}\" loaded");
			}

			return _materials[name];
		}
		
		public abstract void Bind(mat4 model, mat4 view, mat4 projection, vec3 cameraPos, PointLight light);
	}
}