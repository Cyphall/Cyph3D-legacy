using Cyph3D.Misc;
using GlmSharp;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using GLFWWindow = OpenToolkit.Windowing.GraphicsLibraryFramework.Window;

namespace Cyph3D
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

		private bool _guiOpen = true;
		public bool GuiOpen
		{
			get => _guiOpen;
			private set
			{
				_guiOpen = value;

				if (value)
				{
					GLFW.SetInputMode(_glfwWindow, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);
					CursorPos = Center;
				}
				else
				{
					GLFW.SetInputMode(_glfwWindow, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);
				}
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

		public delegate void AnyMouseButtonDelegate(MouseButton button, InputAction action, KeyModifiers mods);
		public delegate void MouseButtonDelegate(InputAction action, KeyModifiers mods);

		public event AnyMouseButtonDelegate MouseButtonEvent;
		public event MouseButtonDelegate RightClickEvent;
		public event MouseButtonDelegate LeftClickEvent;
		
		public delegate void KeyDelegate(Keys key, InputAction action, KeyModifiers mods);
		
		public event KeyDelegate KeyEvent;

		private GLFWCallbacks.MouseButtonCallback _mouseButtonCallback;
		private GLFWCallbacks.KeyCallback _keyCallback;
		private GLFWCallbacks.CursorPosCallback _cursorPosCallback;
		
		public Window(ivec2? size = null)
		{
			GLFW.Init();

			GLFW.SetErrorCallback((code, message) => Logger.Error(message, "GLFW"));

			GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
			GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 6);
			GLFW.WindowHint(WindowHintInt.DepthBits, 0);
			GLFW.WindowHint(WindowHintInt.StencilBits, 0);
			GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
#if DEBUG
			GLFW.WindowHint(WindowHintBool.OpenGLDebugContext, true);
#endif

			if (size != null)
			{
				_glfwWindow = GLFW.CreateWindow(size.Value.x, size.Value.y, "Cyph3D", null, null);
			}
			else
			{
				Monitor* monitor = GLFW.GetPrimaryMonitor();
				VideoMode* mode = GLFW.GetVideoMode(monitor);
				_glfwWindow = GLFW.CreateWindow(mode->Width, mode->Height, "Cyph3D", monitor, null);
			}

			GLFW.MakeContextCurrent(_glfwWindow);
			GuiOpen = false;
			
			GLFW.SetInputMode(_glfwWindow, (StickyAttributes)0x00033005, true); // Enable raw mouse

			if (size != null)
			{
				VideoMode* vidmode = GLFW.GetVideoMode(GLFW.GetPrimaryMonitor());
				if (vidmode != null)
				{
					GLFW.SetWindowPos(_glfwWindow, (vidmode->Width - size.Value.x) / 2, (vidmode->Height- size.Value.y) / 2);
				}
			}

			SetCallbacks();
		}

		private void SetCallbacks()
		{
			_mouseButtonCallback = MouseCallback;
			GLFW.SetMouseButtonCallback(_glfwWindow, _mouseButtonCallback);
			
			_keyCallback = KeyCallback;
			GLFW.SetKeyCallback(_glfwWindow, _keyCallback);

			_cursorPosCallback = CursorPosCallback;
			GLFW.SetCursorPosCallback(_glfwWindow, _cursorPosCallback);

			KeyEvent += (key, action, mods) => {
				if (action == InputAction.Press)
				{
					switch (key)
					{
						case Keys.Escape:
							GuiOpen = !GuiOpen;
							break;
						case Keys.Q:
							if (mods == KeyModifiers.Control)
								ShouldClose = true;
							break;
					}
				}
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
		
		private void CursorPosCallback(GLFWWindow* window, double x, double y)
		{
			
		}
	}
}