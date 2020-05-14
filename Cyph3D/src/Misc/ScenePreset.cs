using Cyph3D.GLObject;
using GlmSharp;

namespace Cyph3D.Misc
{
	public static class ScenePreset
	{
		public static Scene Spaceship()
		{
			Scene scene = new Scene(new Camera(new vec3(0, 1.5f, -3)));

			MeshObject corridor = new MeshObject(
				scene.Root,
				Material.GetOrLoad(
					"LinkedToMesh/corridor",
					true
				),
				Mesh.GetOrLoad("corridor"),
				"Corridor",
				rotation: new vec3(0, 270, 0)
			);
			scene.Add(corridor);

			scene.Add(
				new PointLight(
					corridor.Transform,
					new vec3(10.19f, 0.6f, -0.29f),
					MathExt.FromRGB(245, 243, 255),
					1f
				)
			);

			scene.Add(
				new PointLight(
					corridor.Transform,
					new vec3(10.6f, 0.6f, -0.1f),
					MathExt.FromRGB(245, 243, 255),
					1f
				)
			);

			scene.Add(
				new PointLight(
					corridor.Transform,
					new vec3(10.83f, 0.6f, 0.21f),
					MathExt.FromRGB(245, 243, 255),
					1f
				)
			);

			for (int i = 0; i < 10; i++)
			{
				scene.Add(
					new PointLight(
						corridor.Transform,
						new vec3(-0.93f + i * 1.55f, 2.99f, 0),
						MathExt.FromRGB(222, 215, 188),
						0.2f
					)
				);

				scene.Add(
					new PointLight(
						corridor.Transform,
						new vec3(-0.62f + i * 1.55f, 2.99f, 0),
						MathExt.FromRGB(222, 215, 188),
						0.2f
					)
				);
			}

			return scene;
		}

		public static Scene Dungeon()
		{
			Scene scene = new Scene(new Camera(new vec3(-12, 1.8f, 0), new vec2(90, 0)));

			MeshObject dungeon = new MeshObject(
				scene.Root,
				Material.GetOrLoad(
					"Tiles/WallBrick",
					true
				),
				Mesh.GetOrLoad("dungeon"),
				"Dungeon"
			);
			scene.Add(dungeon);

			scene.Add(
				new PointLight(
					dungeon.Transform,
					new vec3(0, 1.5f, 0),
					MathExt.FromRGB(255, 141, 35),
					10f
				)
			);

			return scene;
		}

		public static Scene TestQuat()
		{
			Scene scene = new Scene(new Camera(new vec3(0, 0, -4)));
			scene.Add(
				new PointLight(
					scene.Root,
					new vec3(0, 5, 8),
					MathExt.FromRGB(255, 255, 255),
					10f
				)
			);

			scene.Add(
				new MeshObject(
					scene.Root,
					Material.GetOrLoad(
						"Tiles/ModernTiles",
						true
					),
					Mesh.GetOrLoad("Tiles32_cube"),
					position: new vec3(0, -18, 0),
					scale: new vec3(16)
				)
			);

			scene.Add(
				new MeshObject(
					scene.Root,
					Material.GetOrLoad(
						"Sun",
						false
					),
					Mesh.GetOrLoad("simple_cube"),
					rotation: new vec3(0, 0, 45),
					angularVelocity: new vec3(30f, 0, 0)
				)
			);

			return scene;
		}

		public static Scene TestCube()
		{
			Scene scene = new Scene(new Camera(new vec3(2, 0, -1), new vec2(-60, 0)));
			
			scene.Add(
				new PointLight(
					scene.Root,
					new vec3(3, 2, 3),
					MathExt.FromRGB(255, 255, 255),
					10f
				)
			);

			scene.Add(
				new MeshObject(
					scene.Root,
					Material.GetOrLoad(
						"Metals/RustedMetal",
						true
					),
					Mesh.GetOrLoad("simple_cube")
				)
			);

			return scene;
		}

		public static Scene TestSphere()
		{
			Scene scene = new Scene(new Camera(new vec3(2, 0, -1), new vec2(-60, 0)));
			
			scene.Add(
				new PointLight(
					scene.Root,
					new vec3(4, 2, 4),
					MathExt.FromRGB(255, 255, 255),
					10f
				)
			);

			scene.Add(
				new MeshObject(
					scene.Root,
					Material.GetOrLoad(
						"Metals/OrnateBrass",
						true
					),
					Mesh.GetOrLoad("Tiles32_cube")
				)
			);

			return scene;
		}
	}
}