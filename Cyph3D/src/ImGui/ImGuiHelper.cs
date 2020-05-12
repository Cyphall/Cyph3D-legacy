using System;
using System.Numerics;
using Renderer;
using Renderer.Misc;

namespace ImGuiNET.Impl
{
	public static unsafe class ImGuiHelper
	{
		private static IntPtr _context = IntPtr.Zero;
		
		public static void Init()
		{
			_context = ImGui.CreateContext();
			ImGui.SetCurrentContext(_context);
			
			ImplGlfw.Init(Context.Window);
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
			
			HierarchyWindow();
		}

		public static void Shutdown()
		{
			ImplOpenGL.Shutdown();
			ImplGlfw.Shutdown();
			
			ImGui.DestroyContext(_context);
			_context = IntPtr.Zero;
		}

		private static void HierarchyWindow()
		{
			if (!Context.Window.GuiMode) return;
			
			ImGui.SetNextWindowSize(new Vector2(200, 500));
			ImGui.SetNextWindowPos(new Vector2(0));

			if (ImGui.Begin("Hierarchy", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
			{
				ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
				
				AddToTree(Context.SceneRoot);
			
				ImGui.End();
			}

			static void AddToTree(Transform transform)
			{
				if (transform.Children.Count == 0)
					ImGui.BulletText(transform.Name);
				else
				{
					if (ImGui.TreeNode(transform.Name))
					{
						transform.Children.ForEach( c => AddToTree(c));
						ImGui.TreePop();
					}
				}
			}
		}
	}
}