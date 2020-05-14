using Cyph3D.Misc;
using GlmSharp;

namespace Cyph3D
{
	public class SceneObject
	{
		public Transform Transform { get; protected set; }
		public string Name { get; set; }

		public SceneObject(string name = null, SceneObject parent = null, vec3? position = null, vec3? rotation = null, vec3? scale = null)
		{
			Name = name ?? "Object";
			Transform = new Transform(this, parent?.Transform, position, rotation, scale);
		}
	}
}