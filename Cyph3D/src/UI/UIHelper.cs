﻿using System;
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

			UIHierarchy.Show();
			UIMisc.Show();
			UIInspector.Show();
			UIResourceExplorer.Show();
			//UITest.Show();
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