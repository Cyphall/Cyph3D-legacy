using System;
using GlmSharp;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using GLFWWindow = OpenToolkit.Windowing.GraphicsLibraryFramework.Window;

namespace Renderer
{
	public unsafe class Window
	{
		private GLFWWindow* _glfwWindow;
		public static implicit operator GLFWWindow*(Window window) => window._glfwWindow; 

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

		public ivec2 Center => Size / 2;

		private bool _guiMode = true;
		public bool GuiMode
		{
			get => _guiMode;
			private set
			{
				_guiMode = value;
				switch (value)
				{
					case true:
						GLFW.SetInputMode(_glfwWindow, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);
						break;
					case false:
						CursorPos = Center;
						GLFW.SetInputMode(_glfwWindow, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);
						break;
				}
			}
		}

		public vec2 CursorPos
		{
			get
			{
				if (GuiMode)
					return Center;
				
				GLFW.GetCursorPos(_glfwWindow, out double x, out double y);
				return new vec2((float)x, (float)y);
			}
			set
			{
				if (!GuiMode)
					GLFW.SetCursorPos(_glfwWindow, value.x, value.y);
			}
		}

		public bool ShouldClose
		{
			get => GLFW.WindowShouldClose(_glfwWindow);
			set => GLFW.SetWindowShouldClose(_glfwWindow, value);
		}

		public delegate void AnyMouseButtonDelegate(MouseButton button, InputAction action, KeyModifiers mods);
		public delegate void MouseButtonDelegate(InputAction action, KeyModifiers mods);

		public event AnyMouseButtonDelegate MouseButtonEvent;
		public event MouseButtonDelegate RightClickEvent;
		public event MouseButtonDelegate LeftClickEvent;
		
		public delegate void KeyDelegate(Keys key, InputAction action, KeyModifiers mods);
		
		public event KeyDelegate KeyEvent;

		private GLFWCallbacks.MouseButtonCallback _mouseButtonCallback;
		private GLFWCallbacks.KeyCallback _keyCallback;
		
		public Window()
		{
			Monitor* monitor = GLFW.GetPrimaryMonitor();
			VideoMode* mode = GLFW.GetVideoMode(monitor);
			_glfwWindow = GLFW.CreateWindow(mode->Width, mode->Height, "Cyph3D", monitor, null);
			
			GLFW.MakeContextCurrent(_glfwWindow);
			GuiMode = false;
			
			SetCallbacks();
		}
		
		public Window(ivec2 size)
		{
			_glfwWindow = GLFW.CreateWindow(size.x, size.y, "Cyph3D", null, null);
			
			GLFW.MakeContextCurrent(_glfwWindow);
			GuiMode = false;
			
			VideoMode* vidmode = GLFW.GetVideoMode(GLFW.GetPrimaryMonitor());
			if (vidmode != null)
			{
				GLFW.SetWindowPos(_glfwWindow, (vidmode->Width - size.x) / 2, (vidmode->Height- size.y) / 2);
			}
			
			SetCallbacks();
		}

		private void SetCallbacks()
		{
			_mouseButtonCallback = MouseCallback;
			GLFW.SetMouseButtonCallback(_glfwWindow, _mouseButtonCallback);
			
			_keyCallback = KeyCallback;
			GLFW.SetKeyCallback(_glfwWindow, _keyCallback);

			KeyEvent += (key, action, mods) => {
				if (key == Keys.LeftAlt && action == InputAction.Press)
					GuiMode = !GuiMode;
			};
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
			MouseButtonEvent?.Invoke(button, action, mods);
			switch (button)
			{
				case MouseButton.Left:
					LeftClickEvent?.Invoke(action, mods);
					break;
				case MouseButton.Right:
					RightClickEvent?.Invoke(action, mods);
					break;
			}
		}
		
		private void KeyCallback(GLFWWindow* window, Keys key, int scancode, InputAction action, KeyModifiers mods)
		{
			KeyEvent?.Invoke(key, action, mods);
		}
	}
}