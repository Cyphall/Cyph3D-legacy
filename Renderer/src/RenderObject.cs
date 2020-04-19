using GlmSharp;
using Renderer.GLObject;
using Renderer.Material;
using Renderer.Misc;

namespace Renderer
{
	public class RenderObject
	{
		private MaterialBase _material;
		private Mesh _mesh;
		
		public ActiveTransform Transform { get; }
		public bool IsLight { get; }

		public RenderObject(
			MaterialBase material,
			string meshName,
			bool isLight,
			vec3? position = null,
			vec3? rotation = null,
			vec3? scale = null,
			vec3? velocity = null,
			vec3? angularVelocity = null)
		{
			_material = material;
			_mesh = Mesh.Get(meshName);
			IsLight = isLight;
			
			Transform = new ActiveTransform(position, rotation, scale, velocity, angularVelocity);
		}

		public void Render(mat4 view, mat4 projection, vec3 cameraPos, vec3 lightPos)
		{
			_material.Bind(Transform.Matrix, view, projection, cameraPos, lightPos);
			_mesh.Render();
		}

		public void Update(double deltaTime)
		{
			Transform.Update(deltaTime);
		}
	}
}