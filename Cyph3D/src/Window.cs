using System;
using GlmSharp;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using GLFWWindow = OpenToolkit.Windowing.GraphicsLibraryFramework.Window;

namespace Renderer
{
	public unsafe class Window
	{
		private GLFWWindow* _glfwWindow;

		private ivec2 _size = ivec2.MinValue;
		public ivec2 Size
		{
			get
			{
				if (_size == ivec2.MinValue)
				{
					GLFW.GetWindowSize(_glfwWindow, out _size.x, out _size.y);
				}

				return _size;
			}
		}

		public vec2 CursorPos
		{
			get
			{
				GLFW.GetCursorPos(_glfwWindow, out double x, out double y);
				return new vec2((float)x, (float)y);
			}
			set => GLFW.SetCursorPos(_glfwWindow, value.x, value.y);
		}

		public bool ShouldClose
		{
			get => GLFW.WindowShouldClose(_glfwWindow);
			set => GLFW.SetWindowShouldClose(_glfwWindow, value);
		}

		public delegate void LeftClickCallbackDelegate(InputAction action, KeyModifiers mods);
		public LeftClickCallbackDelegate LeftClickCallback { get; set; }
		
		public delegate void RightClickCallbackDelegate(InputAction action, KeyModifiers mods);
		public RightClickCallbackDelegate RightClickCallback { get; set; }

		private GLFWCallbacks.MouseButtonCallback _mouseButtonCallback;
		
		public Window()
		{
			Monitor* monitor = GLFW.GetPrimaryMonitor();
			VideoMode* mode = GLFW.GetVideoMode(monitor);
			_glfwWindow = GLFW.CreateWindow(mode->Width, mode->Height, "Cyph3D", monitor, null);
			
			GLFW.MakeContextCurrent(_glfwWindow);
			GLFW.SetInputMode(_glfwWindow, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);

			_mouseButtonCallback = MouseCallback;
			GLFW.SetMouseButtonCallback(_glfwWindow, _mouseButtonCallback);
		}
		
		public Window(ivec2 size)
		{
			_glfwWindow = GLFW.CreateWindow(size.x, size.y, "Cyph3D", null, null);
			
			GLFW.MakeContextCurrent(_glfwWindow);
			GLFW.SetInputMode(_glfwWindow, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);
		}

		public InputAction GetKey(Keys key)
		{
			return GLFW.GetKey(_glfwWindow, key);
		}
		
		public InputAction GetMouseButton(MouseButton button)
		{
			return GLFW.GetMouseButton(_glfwWindow, button);
		}

		public void SwapBuffers()
		{
			GLFW.SwapBuffers(_glfwWindow);
		}

		private void MouseCallback(GLFWWindow* window, MouseButton button, InputAction action, KeyModifiers mods)
		{
			switch (button)
			{
				case MouseButton.Left:
					LeftClickCallback?.Invoke(action, mods);
					break;
				case MouseButton.Right:
					RightClickCallback?.Invoke(action, mods);
					break;
			}
		}
	}
}