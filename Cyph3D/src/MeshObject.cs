using Cyph3D.GLObject;
using Cyph3D.Misc;
using GlmSharp;

namespace Cyph3D
{
	public class MeshObject : SceneObject
	{
		private Material _material;
		private Mesh _mesh;
		
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
			_material = material;
			_mesh = mesh;
			Velocity = velocity ?? vec3.Zero;
			AngularVelocity = angularVelocity ?? vec3.Zero;
		}

		public void Render(mat4 view, mat4 projection, vec3 cameraPos)
		{
			_material.Bind(Transform.WorldMatrix, view, projection, cameraPos);
			_mesh.Render();
		}

		public override void Update(double deltaTime)
		{
			Transform.Position += Velocity * (float)deltaTime;
			Transform.Rotation += AngularVelocity * (float)deltaTime;
		}
	}
}