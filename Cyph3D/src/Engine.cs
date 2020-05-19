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

		private static Scene _scene;
		public static Scene Scene
		{
			get => _scene;
			set
			{
				_scene = value;
				SceneChanged?.Invoke();
			}
		}

		public static Renderer Renderer { get; private set; }

		public static event Action SceneChanged;

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
			
			Scene = new Scene();
			
			ImGuiHelper.Init();
		}

		public static void Run()
		{
			while (!Window.ShouldClose)
			{
				GLFW.PollEvents();

				double deltaTime = Logger.Time.DeltaTime;
				
				Scene.Camera.Update(deltaTime);
				Scene.Objects.ForEach(o => o.Update(deltaTime));
			
				Renderer.Render(Scene.Camera);

				ImGuiHelper.Update();
				ImGuiHelper.Render();

				Window.SwapBuffers();
			}
		}

		public static void Shutdown()
		{
			Scene.Dispose();
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