using System;
using System.Collections.Generic;
using Cyph3D.UI.Impl;
using Cyph3D.UI.Window;
using ImGuiNET;

namespace Cyph3D.UI
{
	public static unsafe class ImGuiHelper
	{
		private static IntPtr _context = IntPtr.Zero;
		private static List<IUIWindow> _windows = new List<IUIWindow>();
		
		public static void Init()
		{
			_context = ImGui.CreateContext();
			ImGui.SetCurrentContext(_context);
			
			ImplGlfw.Init(Engine.Window);
			ImplOpenGL.Init();
			
			ImGui.StyleColorsDark();
			
			_windows.Add(new UIHierarchy());
			_windows.Add(new UIDebug());
		}

		public static void Render()
		{
			ImGui.Render();
			ImplOpenGL.RenderDrawData(*ImGuiNative.igGetDrawData());
		}

		public static void Update()
		{
			ImplOpenGL.NewFrame();
			ImplGlfw.NewFrame();
			ImGui.NewFrame();
			
			if (!Engine.Window.GuiOpen) return;

			for (int i = 0; i < _windows.Count; i++)
			{
				_windows[i].Show();
			}
		}

		public static void Shutdown()
		{
			ImplOpenGL.Shutdown();
			ImplGlfw.Shutdown();
			
			ImGui.DestroyContext(_context);
			_context = IntPtr.Zero;
		}
	}
}