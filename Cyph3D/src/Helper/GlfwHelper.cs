using System;
using System.Runtime.InteropServices;
using Cyph3D.Misc;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using GLFWWindow = OpenToolkit.Windowing.GraphicsLibraryFramework.Window;

namespace Cyph3D.Helper
{
	public static class GlfwHelper
	{
		//TODO: Delete me when we can finally retrieve ints with GLFW.GetWindowAttrib
		static GlfwHelper()
		{
			NativeLibrary.SetDllImportResolver(typeof (GlfwHelper).Assembly, (name, assembly, path) =>
			{
				if (name != "glfw3.dll")
					return IntPtr.Zero;
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					return NativeLibrary.Load("libglfw.so.3", assembly, path);
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					return NativeLibrary.Load("libglfw.3.dylib", assembly, path);
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					return IntPtr.Zero;
				return IntPtr.Size == 8 ? NativeLibrary.Load("glfw3-x64.dll", assembly, path) : NativeLibrary.Load("glfw3-x86.dll", assembly, path);
			});
		}
		
		[DllImport("glfw3.dll")]
		private static extern unsafe int glfwGetWindowAttrib(
			GLFWWindow* window,
			int attribute);

		public static void Init()
		{
			GLFW.Init();

			GLFW.SetErrorCallback((code, message) => Logger.Error(message, "GLFW"));

			EnsureGpuIsCompatible();
			
			GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
			GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 6);
			GLFW.WindowHint(WindowHintInt.DepthBits, 0);
			GLFW.WindowHint(WindowHintInt.StencilBits, 0);
			GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
#if DEBUG
			GLFW.WindowHint(WindowHintBool.OpenGLDebugContext, true);
#endif
		}
		
		private static unsafe void EnsureGpuIsCompatible()
		{
			GLFW.WindowHint(WindowHintBool.Visible, false);
			
			GLFWWindow* window = GLFW.CreateWindow(1, 1, "CompatibilityQuery", null, null);
			GLFW.MakeContextCurrent(window);
			
			GL.LoadBindings(new GLFWBindingsContext());

			int major = glfwGetWindowAttrib(window, (int)WindowHintInt.ContextVersionMajor);
			int minor = glfwGetWindowAttrib(window, (int)WindowHintInt.ContextVersionMinor);
			int revision = glfwGetWindowAttrib(window, (int)WindowHintInt.ContextRevision);
			Version maxSupportedOpenGLVersion = new Version(major, minor, revision);
			Version requestedOpenGLVersion = new Version(4, 6, 0);

			bool error = false;
			if (maxSupportedOpenGLVersion < requestedOpenGLVersion)
			{
				Logger.Error(
					$"OpenGL {requestedOpenGLVersion} is not supported by this driver.\n" +
					"Please make sure your GPU is compatible and your driver is up to date.\n\n" + 
					$"Driver: {GL.GetString(StringName.Version)}\n" +
					$"GPU: {GL.GetString(StringName.Renderer)}\n", "OPGL");
				error = true;
			}

			string[] requiredExtensions = {"GL_ARB_bindless_texture"};

			foreach (string extension in requiredExtensions)
			{
				if (error)
					break;
				
				if (!GLFW.ExtensionSupported(extension))
				{
					Logger.Error(
						$"OpenGL extension '{extension}' is not supported by this driver.\n" +
						"Please make sure your GPU is compatible and your driver is up to date.\n\n" + 
						$"Driver: {GL.GetString(StringName.Version)}\n" +
						$"GPU: {GL.GetString(StringName.Renderer)}\n", "OPGL");
					error = true;
				}
			}
			
			GLFW.MakeContextCurrent(null);
			GLFW.DestroyWindow(window);
			
			GLFW.WindowHint(WindowHintBool.Visible, true);

			if (error)
			{
				Console.WriteLine("Press any key to continue...");
				Console.ReadKey();
				Environment.Exit(1);
			}
		}
	}
}