using Cyph3D.Misc;
using GlmSharp;

namespace Cyph3D
{
	public class SceneObject
	{
		public Transform Transform { get; protected set; }
		public string Name { get; set; }
		public bool IsValid { get; private set; } = true;

		public SceneObject(Transform parent, string name, vec3? position = null, vec3? rotation = null, vec3? scale = null)
		{
			Name = name;
			Transform = new Transform(this, parent, position, rotation, scale);
		}

		public virtual void Update(double deltaTime)
		{
			
		}

		public void Invalidate()
		{
			IsValid = false;
		}
	}
}