using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Cyph3D;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using Window = OpenToolkit.Windowing.GraphicsLibraryFramework.Window;

namespace ImGuiNET.Impl
{
	public static unsafe class ImplGlfw
	{
		// Data
        
        private static Window*   _window = null; // Main window
        private static double    _time;
        private static bool[]    _mouseJustPressed = { false, false, false, false, false };
        private static Cursor*[] _mouseCursors = new Cursor*[(int)ImGuiMouseCursor.COUNT];

        // Chain GLFW callbacks: our callbacks will call the user's previously installed callbacks, if any.
        private static GLFWCallbacks.ScrollCallback        _prevUserCallbackScroll;
        private static GLFWCallbacks.CharCallback          _prevUserCallbackChar;
        
        private static GLFWCallbacks.ScrollCallback        _callbackScroll;
        private static GLFWCallbacks.CharCallback          _callbackChar;
        
        public delegate string GetClipboardTextDelegate(IntPtr userData);
        public static string GetClipboardText(IntPtr userData)
        {
            return GLFW.GetClipboardString((Window*)userData);
        }

        public delegate void SetClipboardTextDelegate(IntPtr userData, IntPtr text);
        public static void SetClipboardText(IntPtr userData, IntPtr text)
        {
            GLFW.SetClipboardString((Window*)userData, Marshal.PtrToStringAnsi(text));
        }

        public static void MouseButtonCallback(MouseButton button, InputAction action, KeyModifiers mods)
        {
            if (action == InputAction.Press && button >= 0 && (int)button < _mouseJustPressed.Length)
                _mouseJustPressed[(int)button] = true;
        }

        public static void ScrollCallback(Window* window, double xoffset, double yoffset)
        {
            _prevUserCallbackScroll?.Invoke(window, xoffset, yoffset);

            ImGuiIOPtr io = ImGui.GetIO();
            io.MouseWheelH += (float)xoffset;
            io.MouseWheel += (float)yoffset;
        }

        public static void KeyCallback(Keys key, InputAction action, KeyModifiers mods)
        {
            if (key == Keys.Unknown) return;

            ImGuiIOPtr io = ImGui.GetIO();
            io.KeysDown[(int) key] = action switch
            {
                InputAction.Press => true,
                InputAction.Release => false,
                _ => io.KeysDown[(int) key]
            };

            // Modifiers are not reliable across systems
            io.KeyCtrl = io.KeysDown[(int)Keys.LeftControl] || io.KeysDown[(int)Keys.RightControl];
            io.KeyShift = io.KeysDown[(int)Keys.LeftShift] || io.KeysDown[(int)Keys.RightShift];
            io.KeyAlt = io.KeysDown[(int)Keys.LeftAlt] || io.KeysDown[(int)Keys.RightAlt];
            io.KeySuper = false;
        }

        public static void CharCallback(Window* window, uint c)
        {
            _prevUserCallbackChar?.Invoke(window, c);

            ImGuiIOPtr io = ImGui.GetIO();
            io.AddInputCharacter(c);
        }

        public static void Init(Window* window)
        {
            _window = window;
            _time = 0.0;

            // Setup back-end capabilities flags
            ImGuiIOPtr io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;         // We can honor GetMouseCursor() values (optional)
            io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;    
            fixed(byte* p = &Encoding.ASCII.GetBytes("imgui_impl_glfw")[0])
            {
                io.NativePtr->BackendPlatformName = p;
            }

            // Keyboard mapping. ImGui will use those indices to peek into the io.KeysDown[] array.
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Insert] = (int)Keys.Insert;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Space] = (int)Keys.Space;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.KeyPadEnter] = (int)Keys.KeyPadEnter;
            io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;

            io.NativePtr->SetClipboardTextFn = Marshal.GetFunctionPointerForDelegate((SetClipboardTextDelegate)SetClipboardText);
            io.GetClipboardTextFn = Marshal.GetFunctionPointerForDelegate((GetClipboardTextDelegate)GetClipboardText);
            io.ClipboardUserData = (IntPtr)_window;
            io.ImeWindowHandle = (IntPtr)_window;

            // Create mouse cursors
            // (By design, on X11 cursors are user configurable and some cursors may be missing. When a cursor doesn't exist,
            // GLFW will emit an error which will often be printed by the app, so we temporarily disable error reporting.
            // Missing cursors will return NULL and our _UpdateMouseCursor() function will use the Arrow cursor instead.)
            IntPtr funcPointer = GLFW.SetErrorCallback((error, description) => {});
            GLFWCallbacks.ErrorCallback prevErrorCallback = funcPointer != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<GLFWCallbacks.ErrorCallback>(funcPointer) : null;
            _mouseCursors[(int)ImGuiMouseCursor.Arrow] = GLFW.CreateStandardCursor(CursorShape.Arrow);
            _mouseCursors[(int)ImGuiMouseCursor.TextInput] = GLFW.CreateStandardCursor(CursorShape.IBeam);
            _mouseCursors[(int)ImGuiMouseCursor.ResizeNS] = GLFW.CreateStandardCursor(CursorShape.VResize);
            _mouseCursors[(int)ImGuiMouseCursor.ResizeEW] = GLFW.CreateStandardCursor(CursorShape.HResize);
            _mouseCursors[(int)ImGuiMouseCursor.Hand] = GLFW.CreateStandardCursor(CursorShape.Hand);
            _mouseCursors[(int)ImGuiMouseCursor.ResizeAll] = GLFW.CreateStandardCursor(CursorShape.Arrow);
            _mouseCursors[(int)ImGuiMouseCursor.ResizeNESW] = GLFW.CreateStandardCursor(CursorShape.Arrow);
            _mouseCursors[(int)ImGuiMouseCursor.ResizeNWSE] = GLFW.CreateStandardCursor(CursorShape.Arrow);
            _mouseCursors[(int)ImGuiMouseCursor.NotAllowed] = GLFW.CreateStandardCursor(CursorShape.Arrow);
            GLFW.SetErrorCallback(prevErrorCallback);

            // Chain GLFW callbacks: our callbacks will call the user's previously installed callbacks, if any.
            _prevUserCallbackScroll = null;
            _prevUserCallbackChar = null;
            
            Context.Window.MouseButtonEvent += MouseButtonCallback;
            Context.Window.KeyEvent += KeyCallback;

            _callbackScroll = ScrollCallback;
            funcPointer = GLFW.SetScrollCallback(window, _callbackScroll);
            _prevUserCallbackScroll = funcPointer != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<GLFWCallbacks.ScrollCallback>(funcPointer) : null;

            _callbackChar = CharCallback;
            funcPointer = GLFW.SetCharCallback(window, _callbackChar);
            _prevUserCallbackChar = funcPointer != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<GLFWCallbacks.CharCallback>(funcPointer) : null;
        }

        public static void Shutdown()
        {
            for (ImGuiMouseCursor cursorN = 0; cursorN < ImGuiMouseCursor.COUNT; cursorN++)
            {
                GLFW.DestroyCursor(_mouseCursors[(int)cursorN]);
                _mouseCursors[(int)cursorN] = null;
            }
        }

        public static void UpdateMousePosAndButtons()
        {
            // Update buttons
            ImGuiIOPtr io = ImGui.GetIO();
            for (int i = 0; i < io.MouseDown.Count; i++)
            {
                // If a mouse press event came, always pass it as "mouse held this frame", so we don't miss click-release events that are shorter than 1 frame.
                io.MouseDown[i] = _mouseJustPressed[i] || GLFW.GetMouseButton(_window, (MouseButton)i) != 0;
                _mouseJustPressed[i] = false;
            }

            // Update mouse position
            Vector2 mousePosBackup = io.MousePos;
            io.MousePos = new Vector2(-float.MaxValue, -float.MaxValue);
            bool focused = GLFW.GetWindowAttrib(_window, WindowAttributeGetter.Focused);
            if (focused)
            {
                if (io.WantSetMousePos)
                {
                    GLFW.SetCursorPos(_window, mousePosBackup.X, mousePosBackup.Y);
                }
                else
                {
                    GLFW.GetCursorPos(_window, out double mouseX, out double mouseY);
                    io.MousePos = new Vector2((float)mouseX, (float)mouseY);
                }
            }
        }

        public static void UpdateMouseCursor()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) > 0 || GLFW.GetInputMode(_window, CursorStateAttribute.Cursor) == CursorModeValue.CursorDisabled)
                return;

            ImGuiMouseCursor imguiCursor = ImGui.GetMouseCursor();
            if (imguiCursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
            {
                // Hide OS mouse cursor if imgui is drawing it or if it wants no cursor
                GLFW.SetInputMode(_window, CursorStateAttribute.Cursor, CursorModeValue.CursorHidden);
            }
            else
            {
                // Show OS mouse cursor
                // FIXME-PLATFORM: Unfocused windows seems to fail changing the mouse cursor with GLFW 3.2, but 3.3 works here.
                GLFW.SetCursor(_window, _mouseCursors[(int)imguiCursor] != null ? _mouseCursors[(int)imguiCursor] : _mouseCursors[(int)ImGuiMouseCursor.Arrow]);
                GLFW.SetInputMode(_window, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);
            }
        }

        // private static void UpdateGamepads()
        // {
        //     ImGuiIO& io = ImGuiNET.ImGui::GetIO();
        //     memset(io.NavInputs, 0, sizeof(io.NavInputs));
        //     if ((io.ConfigFlags & ImGuiConfigFlags_NavEnableGamepad) == 0)
        //         return;
        //
        //     // Update gamepad inputs
        //     #define MAP_BUTTON(NAV_NO, BUTTON_NO)       { if (buttons_count > BUTTON_NO && buttons[BUTTON_NO] == GLFW_PRESS) io.NavInputs[NAV_NO] = 1.0f; }
        //     #define MAP_ANALOG(NAV_NO, AXIS_NO, V0, V1) { float v = (axes_count > AXIS_NO) ? axes[AXIS_NO] : V0; v = (v - V0) / (V1 - V0); if (v > 1.0f) v = 1.0f; if (io.NavInputs[NAV_NO] < v) io.NavInputs[NAV_NO] = v; }
        //     int axes_count = 0, buttons_count = 0;
        //     const float* axes = glfwGetJoystickAxes(GLFW_JOYSTICK_1, &axes_count);
        //     const unsigned char* buttons = glfwGetJoystickButtons(GLFW_JOYSTICK_1, &buttons_count);
        //     MAP_BUTTON(ImGuiNavInput_Activate,   0);     // Cross / A
        //     MAP_BUTTON(ImGuiNavInput_Cancel,     1);     // Circle / B
        //     MAP_BUTTON(ImGuiNavInput_Menu,       2);     // Square / X
        //     MAP_BUTTON(ImGuiNavInput_Input,      3);     // Triangle / Y
        //     MAP_BUTTON(ImGuiNavInput_DpadLeft,   13);    // D-Pad Left
        //     MAP_BUTTON(ImGuiNavInput_DpadRight,  11);    // D-Pad Right
        //     MAP_BUTTON(ImGuiNavInput_DpadUp,     10);    // D-Pad Up
        //     MAP_BUTTON(ImGuiNavInput_DpadDown,   12);    // D-Pad Down
        //     MAP_BUTTON(ImGuiNavInput_FocusPrev,  4);     // L1 / LB
        //     MAP_BUTTON(ImGuiNavInput_FocusNext,  5);     // R1 / RB
        //     MAP_BUTTON(ImGuiNavInput_TweakSlow,  4);     // L1 / LB
        //     MAP_BUTTON(ImGuiNavInput_TweakFast,  5);     // R1 / RB
        //     MAP_ANALOG(ImGuiNavInput_LStickLeft, 0,  -0.3f,  -0.9f);
        //     MAP_ANALOG(ImGuiNavInput_LStickRight,0,  +0.3f,  +0.9f);
        //     MAP_ANALOG(ImGuiNavInput_LStickUp,   1,  +0.3f,  +0.9f);
        //     MAP_ANALOG(ImGuiNavInput_LStickDown, 1,  -0.3f,  -0.9f);
        //     #undef MAP_BUTTON
        //     #undef MAP_ANALOG
        //     if (axes_count > 0 && buttons_count > 0)
        //         io.BackendFlags |= ImGuiBackendFlags_HasGamepad;
        //     else
        //         io.BackendFlags &= ~ImGuiBackendFlags_HasGamepad;
        // }

        public static void NewFrame()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            Debug.Assert(io.Fonts.IsBuilt(), "Font atlas not built! It is generally built by the renderer back-end. Missing call to renderer _NewFrame() function? e.g. ImGui_ImplOpenGL3_NewFrame().");

            // Setup display size (every frame to accommodate for window resizing)
            GLFW.GetWindowSize(_window, out int w, out int h);
            GLFW.GetFramebufferSize(_window, out int displayW, out int displayH);
            io.DisplaySize = new Vector2(w, h);
            if (w > 0 && h > 0)
                io.DisplayFramebufferScale = new Vector2((float)displayW / w, (float)displayH / h);

            // Setup time step
            double currentTime = GLFW.GetTime();
            io.DeltaTime = _time > 0.0 ? (float)(currentTime - _time) : 1.0f/60.0f;
            _time = currentTime;

            UpdateMousePosAndButtons();
            UpdateMouseCursor();
        }
	}
}