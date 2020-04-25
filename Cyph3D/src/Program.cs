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

			VideoMode mode = Glfw.GetVideoMode(Glfw.PrimaryMonitor);
			Window window = Glfw.CreateWindow(mode.Width, mode.Height, "Renderer", Glfw.PrimaryMonitor, Window.None);
			// Window window = Glfw.CreateWindow(mode.Width, mode.Height, "Renderer", Monitor.None, Window.None);

			Context.Window = window;

			Glfw.MakeContextCurrent(window);
			Glfw.SetInputMode(window, InputMode.Cursor, (int) CursorMode.Disabled);

			ivec2 winSize = new ivec2();
			Glfw.GetWindowSize(Context.Window, out winSize.x, out winSize.y);
			Context.WindowSize = winSize;

			Camera camera = new Camera(new vec3(1.5f, 0, -2.5f));

			Context.ObjectContainer.Add(
				new RenderObject(
					Material.Get(
						"sun",
						() => new Material(
							false,
							Texture.Get("sun", InternalFormat.Srgb)
						)
					),
					"planet",
					new vec3(4, 2, 4)
				)
			);

			Context.LightContainer.Add(
				new PointLight(
					new vec3(4, 2, 4),
					new vec3(1f, 0.8f, 0.6f),
					1f
				)
			);


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

			ShaderProgram.DisposeAll();
			Shader.DisposeAll();
			Texture.DisposeAll();
			Mesh.DisposeAll();
			Framebuffer.DisposeAll();

			Glfw.Terminate();
		}
	}
}