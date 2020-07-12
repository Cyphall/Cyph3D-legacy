using System;
using Cyph3D.UI.Gizmo;
using Cyph3D.UI.Impl;
using Cyph3D.UI.Window;
using ImGuiNET;

namespace Cyph3D.UI
{
	public static unsafe class UIHelper
	{
		private static IntPtr _context = IntPtr.Zero;
		
		public static void Init()
		{
			_context = ImGui.CreateContext();
			ImGui.SetCurrentContext(_context);
			
			ImplGlfw.Init(Engine.Window);
			ImplOpenGL.Init();
			
			ImGui.StyleColorsDark();
			
			TranslateGizmo.Init();
		}

		public static void Render()
		{
			if (!Engine.Window.GuiOpen) return;

			TranslateGizmo.Update();
			
			ImplOpenGL.NewFrame();
			ImplGlfw.NewFrame();
			ImGui.NewFrame();
			
			UIHierarchy.Show();
			UIMisc.Show();
			UIInspector.Show();
			UIResourceExplorer.Show();
			//UITest.Show();
			
			ImGui.Render();
			ImplOpenGL.RenderDrawData(*ImGuiNative.igGetDrawData());
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