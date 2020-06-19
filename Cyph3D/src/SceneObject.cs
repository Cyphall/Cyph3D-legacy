using System;
using Cyph3D.Extension;
using GlmSharp;

namespace Cyph3D
{
	public abstract class SceneObject
	{
		public Transform Transform { get; protected set; }
		public string Name { get; set; }
		public string GUID { get; } = Guid.NewGuid().ToString();

		protected SceneObject(Transform parent, string name, vec3? position = null, vec3? rotation = null, vec3? scale = null)
		{
			Name = name;
			Transform = new Transform(this, parent, position, rotation, scale);
		}

		public virtual void Update(double deltaTime)
		{
			
		}
	}
}