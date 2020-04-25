using GlmSharp;
using Renderer.GLObject;
using Renderer.Misc;

namespace Renderer
{
	public class RenderObject
	{
		private Material _material;
		private Mesh _mesh;
		
		public ActiveTransform Transform { get; }

		public RenderObject(
			Material material,
			Mesh mesh,
			vec3? position = null,
			vec3? rotation = null,
			vec3? scale = null,
			vec3? velocity = null,
			vec3? angularVelocity = null)
		{
			_material = material;
			_mesh = mesh;
			
			Transform = new ActiveTransform(position, rotation, scale, velocity, angularVelocity);
		}

		public void Render(mat4 view, mat4 projection, vec3 cameraPos)
		{
			_material.Bind(Transform.Matrix, view, projection, cameraPos);
			_mesh.Render();
		}

		public void Update(double deltaTime)
		{
			Transform.Update(deltaTime);
		}
	}
}