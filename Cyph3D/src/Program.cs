using System;
using System.Runtime.InteropServices;
using GLFW;
using GlmSharp;
using OpenGL;
using Renderer.GLObject;
using Renderer.Misc;

namespace Renderer
{
	internal static class Program
	{
		private static void Main()
		{
			Gl.Initialize();
			Glfw.Init();

			Gl.DebugMessageCallback(
				(source, type, id, severity, length, message, param) => { Logger.Error($"message: {Marshal.PtrToStringAnsi(message, length)} | id: {id} | source: {source} | type: {type}", "OPENGL"); }, IntPtr.Zero);
			Glfw.SetErrorCallback((code, message) => Logger.Error(Marshal.PtrToStringAnsi(message), "GLFW"));


			Glfw.WindowHint(Hint.ContextVersionMajor, 4);
			Glfw.WindowHint(Hint.ContextVersionMinor, 6);
			Glfw.WindowHint(Hint.OpenglDebugContext, true);
			Glfw.WindowHint(Hint.DepthBits, 0);
			Glfw.WindowHint(Hint.StencilBits, 0);
			Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);

			VideoMode mode = Glfw.GetVideoMode(Glfw.PrimaryMonitor);
			Window window = Glfw.CreateWindow(mode.Width, mode.Height, "Renderer", Glfw.PrimaryMonitor, Window.None);
			// Window window = Glfw.CreateWindow(mode.Width, mode.Height, "Renderer", Monitor.None, Window.None);

			Context.Window = window;

			Glfw.MakeContextCurrent(window);
			Glfw.SetInputMode(window, InputMode.Cursor, (int) CursorMode.Disabled);

			ivec2 winSize = new ivec2();
			Glfw.GetWindowSize(Context.Window, out winSize.x, out winSize.y);
			Context.WindowSize = winSize;

			Camera camera = Dungeon();

			while (!Glfw.WindowShouldClose(window))
			{
				Glfw.PollEvents();

				if (Glfw.GetKey(window, Keys.Escape) == InputState.Press) Glfw.SetWindowShouldClose(window, true);

				double deltaTime = Logger.Time.DeltaTime;

				camera.Update(deltaTime);
				Context.ObjectContainer.ForEach(o => o.Update(deltaTime));

				camera.Render();

				Glfw.SwapBuffers(window);
			}

			Context.LightManager.Dispose();
			ShaderProgram.DisposeAll();
			Shader.DisposeAll();
			Texture.DisposeAll();
			Mesh.DisposeAll();
			Framebuffer.DisposeAll();

			Glfw.Terminate();
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
			Camera camera = new Camera(new vec3(-8, 1.8f, 0), new vec2(90, 0));
			
			Context.LightManager.AddPointLight(
				new PointLight(
					new vec3(0, 2, 0),
					MathExt.FromRGB(255, 141, 35),
					5f
				)
			);
			
			Context.ObjectContainer.Add(
				new RenderObject(
					Material.GetOrLoad(
						"Tiles/WallBrick",
						true
					),
					Mesh.GetOrLoad("dungeon")
				)
			);

			return camera;
		}

		private static void TestQuat()
		{
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