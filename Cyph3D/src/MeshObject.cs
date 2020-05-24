using Cyph3D.Extension;
using Cyph3D.GLObject;
using GlmSharp;

namespace Cyph3D
{
	public class MeshObject : SceneObject
	{
		public Material Material { get; }
		public Mesh Mesh { get; }

		public vec3 Velocity { get; set; }
		public vec3 AngularVelocity { get; set; }
		
		public MeshObject(
			Transform parent,
			Material material,
			Mesh mesh,
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
			if (Material.Bind(Transform.WorldMatrix, view, projection, cameraPos))
				Mesh.Render();
		}

		public override void Update(double deltaTime)
		{
			Transform.Position += Velocity * (float)deltaTime;
			Transform.Rotation += AngularVelocity * (float)deltaTime;
		}
	}
}