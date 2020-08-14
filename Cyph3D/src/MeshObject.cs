using Cyph3D.GLObject;
using Cyph3D.Misc;
using GlmSharp;

namespace Cyph3D
{
	public class MeshObject : SceneObject
	{
		public Material Material { get; set; }
		public Mesh Mesh { get; set; }

		public vec3 Velocity { get; set; }
		public vec3 AngularVelocity { get; set; }
		public bool ContributeShadows { get; set; }
		
		public MeshObject(
			Transform parent,
			Material material,
			Mesh mesh = null,
			string name = null,
			vec3? position = null,
			vec3? rotation = null,
			vec3? scale = null,
			vec3? velocity = null,
			vec3? angularVelocity = null):
			base(parent, name ?? "Object", position, rotation, scale)
		{
			Material = material;
			Mesh = mesh;
			Velocity = velocity ?? vec3.Zero;
			AngularVelocity = angularVelocity ?? vec3.Zero;
		}

		public void Render(mat4 view, mat4 projection, vec3 cameraPos)
		{
			if (Material != null)
				Material.Bind(Transform.WorldMatrix, view, projection, cameraPos);
			else
				Material.Default.Bind(Transform.WorldMatrix, view, projection, cameraPos);
			
			Mesh?.Render();
		}

		public override void Update(double deltaTime)
		{
			Transform.Position += Velocity * (float)deltaTime;
			
			vec3 rotationOffset = AngularVelocity * (float)deltaTime;
			Transform.Rotation = Transform.Rotation
				.Rotate(glm.Radians(rotationOffset.x), new vec3(1, 0, 0))
				.Rotate(glm.Radians(rotationOffset.y), new vec3(0, 1, 0))
				.Rotate(glm.Radians(rotationOffset.z), new vec3(0, 0, 1));
		}
	}
}