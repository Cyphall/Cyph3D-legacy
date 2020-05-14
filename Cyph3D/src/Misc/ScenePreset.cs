using Cyph3D.GLObject;
using GlmSharp;

namespace Cyph3D.Misc
{
	public static class ScenePreset
	{
		public static Camera Spaceship()
		{
			Camera camera = new Camera(new vec3(0, 1.5f, -3));

			MeshObject corridor = new MeshObject(
				Material.GetOrLoad(
					"LinkedToMesh/corridor",
					true
				),
				Mesh.GetOrLoad("corridor"),
				"Corridor",
				rotation: new vec3(0, 270, 0)
			);
			Engine.ObjectContainer.Add(corridor);

			Engine.LightManager.AddPointLight(
				new PointLight(
					new vec3(10.19f, 0.6f, -0.29f),
					MathExt.FromRGB(245, 243, 255),
					1f,
					corridor
				)
			);

			Engine.LightManager.AddPointLight(
				new PointLight(
					new vec3(10.6f, 0.6f, -0.1f),
					MathExt.FromRGB(245, 243, 255),
					1f,
					corridor
				)
			);

			Engine.LightManager.AddPointLight(
				new PointLight(
					new vec3(10.83f, 0.6f, 0.21f),
					MathExt.FromRGB(245, 243, 255),
					1f,
					corridor
				)
			);

			for (int i = 0; i < 10; i++)
			{
				Engine.LightManager.AddPointLight(
					new PointLight(
						new vec3(-0.93f + i * 1.55f, 2.99f, 0),
						MathExt.FromRGB(222, 215, 188),
						0.2f,
						corridor
					)
				);

				Engine.LightManager.AddPointLight(
					new PointLight(
						new vec3(-0.62f + i * 1.55f, 2.99f, 0),
						MathExt.FromRGB(222, 215, 188),
						0.2f,
						corridor
					)
				);
			}

			return camera;
		}

		public static Camera Dungeon()
		{
			Camera camera = new Camera(new vec3(-12, 1.8f, 0), new vec2(90, 0));

			MeshObject dungeon = new MeshObject(
				Material.GetOrLoad(
					"Tiles/WallBrick",
					true
				),
				Mesh.GetOrLoad("dungeon"),
				"Dungeon"
			);
			Engine.ObjectContainer.Add(dungeon);

			Engine.LightManager.AddPointLight(
				new PointLight(
					new vec3(0, 1.5f, 0),
					MathExt.FromRGB(255, 141, 35),
					10f,
					dungeon
				)
			);

			return camera;
		}

		public static Camera TestQuat()
		{
			Camera camera = new Camera(new vec3(0, 0, -4));
			
			Engine.LightManager.AddPointLight(
				new PointLight(
					new vec3(0, 5, 8),
					MathExt.FromRGB(255, 255, 255),
					10f
				)
			);

			Engine.ObjectContainer.Add(
				new MeshObject(
					Material.GetOrLoad(
						"Tiles/ModernTiles",
						true
					),
					Mesh.GetOrLoad("Tiles32_cube"),
					position: new vec3(0, -18, 0),
					scale: new vec3(16)
				)
			);

			Engine.ObjectContainer.Add(
				new MeshObject(
					Material.GetOrLoad(
						"Sun",
						false
					),
					Mesh.GetOrLoad("simple_cube"),
					rotation: new vec3(0, 0, 45),
					angularVelocity: new vec3(30f, 0, 0)
				)
			);

			return camera;
		}
		
		public static Camera TextHierarchy()
		{
			Camera camera = new Camera(new vec3(0, 0, -6));

			MeshObject root = new MeshObject(
				Material.GetOrLoad(
					"Sun",
					false
				),
				Mesh.GetOrLoad("simple_cube")
			);

			MeshObject elem1 = new MeshObject(
				Material.GetOrLoad(
					"Sun",
					false
				),
				Mesh.GetOrLoad("simple_cube"),
				parent: root,
				position: new vec3(-2, 2, 0)
			);

			MeshObject elem2 = new MeshObject(
				Material.GetOrLoad(
					"Sun",
					false
				),
				Mesh.GetOrLoad("simple_cube"),
				parent: elem1,
				position: new vec3(-2, 2, 0),
				angularVelocity: new vec3(0, 20, 0)
			);
			
			Engine.ObjectContainer.Add(root);
			Engine.ObjectContainer.Add(elem1);
			Engine.ObjectContainer.Add(elem2);

			return camera;
		}

		public static Camera TestCube()
		{
			Camera camera = new Camera(new vec3(2, 0, -1), new vec2(-60, 0));

			Engine.LightManager.AddPointLight(
				new PointLight(
					new vec3(3, 2, 3),
					MathExt.FromRGB(255, 255, 255),
					10f
				)
			);

			Engine.ObjectContainer.Add(
				new MeshObject(
					Material.GetOrLoad(
						"Metals/RustedMetal",
						true
					),
					Mesh.GetOrLoad("simple_cube")
				)
			);

			return camera;
		}

		public static Camera TestSphere()
		{
			Camera camera = new Camera(new vec3(2, 0, -1), new vec2(-60, 0));

			Engine.LightManager.AddPointLight(
				new PointLight(
					new vec3(4, 2, 4),
					MathExt.FromRGB(255, 255, 255),
					10f
				)
			);

			Engine.ObjectContainer.Add(
				new MeshObject(
					Material.GetOrLoad(
						"Metals/OrnateBrass",
						true
					),
					Mesh.GetOrLoad("Tiles32_cube")
				)
			);

			return camera;
		}
	}
}