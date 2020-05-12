using System;
using System.Runtime.InteropServices;
using Cyph3D.GLObject;
using Cyph3D.Misc;
using GlmSharp;
using ImGuiNET.Impl;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.GraphicsLibraryFramework;

namespace Cyph3D
{
	internal static class Program
	{
		private static void Main()
		{
			Context.Window = new Window();

			GL.LoadBindings(new GLFWBindingsContext());

			GL.DebugMessageCallback(
				(source, type, id, severity, length, message, param) => {
					string logMessage = $"{Marshal.PtrToStringAnsi(message, length)} \n id: {id} \n source: {source}";

					switch (severity)
					{
						case DebugSeverity.DebugSeverityHigh:
							Logger.Error(logMessage, "OPENGL");
							break;
						case DebugSeverity.DebugSeverityMedium:
							Logger.Warning(logMessage, "OPENGL");
							break;
						case DebugSeverity.DebugSeverityLow:
							Logger.Info(logMessage, "OPENGL");
							break;
					}
				}, IntPtr.Zero
			);

			Camera camera = Dungeon();
			
			ImGuiHelper.Init();

			while (!Context.Window.ShouldClose)
			{
				GLFW.PollEvents();

				if (Context.Window.GetKey(Keys.Escape) == InputAction.Press) Context.Window.ShouldClose = true;

				double deltaTime = Logger.Time.DeltaTime;

				camera.Update(deltaTime);
				Context.ObjectContainer.ForEach(o => o.Update(deltaTime));

				camera.Render();
				
				ImGuiHelper.Update();

				ImGuiHelper.Render();

				Context.Window.SwapBuffers();
			}

			Context.LightManager.Dispose();
			ShaderProgram.DisposeAll();
			Shader.DisposeAll();
			Texture.DisposeAll();
			Renderbuffer.DisposeAll();
			ShaderStorageBuffer.DisposeAll();
			Mesh.DisposeAll();
			Framebuffer.DisposeAll();

			ImGuiHelper.Shutdown();
			
			GLFW.Terminate();
		}

		private static Camera Spaceship()
		{
			Camera camera = new Camera(new vec3(0, 1.5f, -3));

			Context.ObjectContainer.Add(
				new RenderObject(
					Material.GetOrLoad(
						"LinkedToMesh/corridor",
						true
					),
					Mesh.GetOrLoad("corridor"),
					rotation: new vec3(0, 270, 0)
				)
			);

			Context.LightManager.AddPointLight(
				new PointLight(
					new vec3(-0.29f, 0.6f, 10.19f),
					MathExt.FromRGB(245, 243, 255),
					1f
				)
			);

			Context.LightManager.AddPointLight(
				new PointLight(
					new vec3(-0.1f, 0.6f, 10.6f),
					MathExt.FromRGB(245, 243, 255),
					1f
				)
			);

			Context.LightManager.AddPointLight(
				new PointLight(
					new vec3(0.21f, 0.6f, 10.83f),
					MathExt.FromRGB(245, 243, 255),
					1f
				)
			);

			for (int i = 0; i < 10; i++)
			{
				Context.LightManager.AddPointLight(
					new PointLight(
						new vec3(0, 2.99f, -0.93f + i * 1.55f),
						MathExt.FromRGB(222, 215, 188),
						0.2f
					)
				);

				Context.LightManager.AddPointLight(
					new PointLight(
						new vec3(0, 2.99f, -0.62f + i * 1.55f),
						MathExt.FromRGB(222, 215, 188),
						0.2f
					)
				);
			}

			return camera;
		}

		private static Camera Dungeon()
		{
			Camera camera = new Camera(new vec3(-12, 1.8f, 0), new vec2(90, 0));

			Context.LightManager.AddPointLight(
				new PointLight(
					new vec3(0, 1.5f, 0),
					MathExt.FromRGB(255, 141, 35),
					10f
				)
			);

			Context.ObjectContainer.Add(
				new RenderObject(
					Material.GetOrLoad(
						"Tiles/WallBrick",
						true
					),
					Mesh.GetOrLoad("dungeon"),
					"Dungeon"
				)
			);

			return camera;
		}

		private static Camera TestQuat()
		{
			Camera camera = new Camera(new vec3(0, 0, -4));
			
			Context.LightManager.AddPointLight(
				new PointLight(
					new vec3(0, 5, 8),
					MathExt.FromRGB(255, 255, 255),
					10f
				)
			);

			Context.ObjectContainer.Add(
				new RenderObject(
					Material.GetOrLoad(
						"Tiles/ModernTiles",
						true
					),
					Mesh.GetOrLoad("Tiles32_cube"),
					position: new vec3(0, -18, 0),
					scale: new vec3(16)
				)
			);

			Context.ObjectContainer.Add(
				new RenderObject(
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
		
		private static Camera TextHierarchy()
		{
			Camera camera = new Camera(new vec3(0, 0, -6));

			RenderObject root = new RenderObject(
				Material.GetOrLoad(
					"Sun",
					false
				),
				Mesh.GetOrLoad("simple_cube")
			);

			RenderObject elem1 = new RenderObject(
				Material.GetOrLoad(
					"Sun",
					false
				),
				Mesh.GetOrLoad("simple_cube"),
				parent: root,
				position: new vec3(-2, 2, 0)
			);

			RenderObject elem2 = new RenderObject(
				Material.GetOrLoad(
					"Sun",
					false
				),
				Mesh.GetOrLoad("simple_cube"),
				parent: elem1,
				position: new vec3(-2, 2, 0),
				angularVelocity: new vec3(0, 20, 0)
			);
			
			Context.ObjectContainer.Add(root);
			Context.ObjectContainer.Add(elem1);
			Context.ObjectContainer.Add(elem2);

			return camera;
		}

		private static Camera TestCube()
		{
			Camera camera = new Camera(new vec3(2, 0, -1), new vec2(-60, 0));

			Context.LightManager.AddPointLight(
				new PointLight(
					new vec3(3, 2, 3),
					MathExt.FromRGB(255, 255, 255),
					10f
				)
			);

			Context.ObjectContainer.Add(
				new RenderObject(
					Material.GetOrLoad(
						"Metals/RustedMetal",
						true
					),
					Mesh.GetOrLoad("simple_cube")
				)
			);

			return camera;
		}

		private static Camera TestSphere()
		{
			Camera camera = new Camera(new vec3(2, 0, -1), new vec2(-60, 0));

			Context.LightManager.AddPointLight(
				new PointLight(
					new vec3(4, 2, 4),
					MathExt.FromRGB(255, 255, 255),
					10f
				)
			);

			Context.ObjectContainer.Add(
				new RenderObject(
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