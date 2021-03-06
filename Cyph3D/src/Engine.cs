using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using Cyph3D.GLObject;
using Cyph3D.Helper;
using Cyph3D.Misc;
using Cyph3D.Rendering;
using Cyph3D.ResourceManagement;
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
		
		public static ResourceManager GlobalResourceManager { get; } = new ResourceManager();

		public static Renderer Renderer { get; private set; }
		
		public static ThreadPool ThreadPool { get; private set; }

		public static event Action SceneChanged;

		public static void Init()
		{
			GlfwHelper.Init();
			
			Window = new Window();
			
			GLFW.GetVersion(out int major, out int minor, out int revision);
			Logger.Info($"GLFW Version: {major}.{minor}.{revision}", "GLFW");
			Logger.Info($"OpenGL Version: {GL.GetString(StringName.Version)}", "OPGL");
			Logger.Info($"GPU: {GL.GetString(StringName.Renderer)}", "OPGL");
			
			int coreCount = 0;
			foreach (ManagementBaseObject item in new ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get())
			{
				coreCount += int.Parse(item["NumberOfCores"].ToString());
			}
			
			ThreadPool = new ThreadPool(Math.Max(coreCount-1, 1));

			GL.Enable(EnableCap.DebugOutputSynchronous);
			GL.Enable(EnableCap.DebugOutput);
			GL.DebugMessageCallback(
				(source, type, id, severity, length, message, param) => {
					string logMessage = $"{Marshal.PtrToStringAnsi(message, length)} \n id: {id} \n source: {source}";

					switch (severity)
					{
						case DebugSeverity.DebugSeverityHigh:
							Logger.Error(logMessage, "OPGL");
							Debugger.Break();
							break;
						case DebugSeverity.DebugSeverityMedium:
							Logger.Warning(logMessage, "OPGL");
							break;
						case DebugSeverity.DebugSeverityLow:
							Logger.Info(logMessage, "OPGL");
							break;
					}
				}, IntPtr.Zero
			);
			
			Material.InitializeDefault();
			Framebuffer.InitDrawToDefault();
			
			Renderer = new Renderer();
			
			Scene = new Scene();
			
			UIHelper.Init();
		}

		public static void Run()
		{
			while (!Window.ShouldClose)
			{
				GLFW.PollEvents();
				
				GlobalResourceManager.Update();
				Scene.ResourceManager.Update();

				double deltaTime = Logger.Time.DeltaTime;
				
				Scene.Camera.Update(deltaTime);
				Scene.Objects.ForEach(o => o.Update(deltaTime));
			
				Renderer.Render(Scene.Camera);

				UIHelper.Update();
				UIHelper.Render();

				Window.SwapBuffers();
			}
		}

		public static void Shutdown()
		{
			Scene.Dispose();
			GlobalResourceManager.Dispose();
			// ShaderProgram.DisposeAll();
			// Shader.DisposeAll();
			// Texture.DisposeAll();
			// Renderbuffer.DisposeAll();
			// ShaderStorageBuffer.DisposeAll();
			// Mesh.DisposeAll();
			// Framebuffer.DisposeAll();

			UIHelper.Shutdown();
			
			Renderer.Dispose();
			
			GLFW.Terminate();
		}

		
	}
}