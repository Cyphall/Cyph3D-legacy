using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cyph3D.GLObject;
using Cyph3D.Misc;
using Cyph3D.UI;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.GraphicsLibraryFramework;

namespace Cyph3D
{
	public static class Engine
	{
		public static Window Window { get; private set; }
		public static List<MeshObject> ObjectContainer { get; } = new List<MeshObject>();
		public static SceneObject SceneRoot { get; } = new SceneObject("Root");
		public static LightManager LightManager { get; } = new LightManager();
		public static Camera Camera { get; private set; }
		public static Renderer Renderer { get; private set; }

		public static void Init()
		{
			Window = new Window();

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
			
			Renderer = new Renderer();

			Camera = ScenePreset.Spaceship();
			
			ImGuiHelper.Init();
		}

		public static void Run()
		{
			while (!Window.ShouldClose)
			{
				GLFW.PollEvents();

				double deltaTime = Logger.Time.DeltaTime;

				Camera.Update(deltaTime);
				ObjectContainer.ForEach(o => o.Update(deltaTime));
				
				Renderer.Render(Camera);
				
				ImGuiHelper.Update();
				ImGuiHelper.Render();

				Window.SwapBuffers();
			}
		}

		public static void Shutdown()
		{
			LightManager.Dispose();
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
	}
}