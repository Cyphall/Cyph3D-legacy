using System.Numerics;
using Cyph3D.Misc;
using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public static class UIHierarchy
	{
		private const ImGuiTreeNodeFlags BASE_FLAGS = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;
		public static Transform Selected { get; private set; }

		public static void Show()
		{
			ImGui.SetNextWindowSize(new Vector2(300, 500));
			ImGui.SetNextWindowPos(new Vector2(0));

			if (ImGui.Begin("Hierarchy", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
			{
				ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
				
				AddToTree(Engine.SceneRoot);
			
				ImGui.End();
			}
		}
		
		private static void AddToTree(Transform transform)
		{
			ImGuiTreeNodeFlags flags = BASE_FLAGS;
			
			if (transform.Children.Count == 0)
			{
				flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
				if (Selected == transform)
					flags |= ImGuiTreeNodeFlags.Selected;
				
				ImGui.TreeNodeEx(transform.Name, flags);

				if (ImGui.IsItemClicked())
				{
					Selected = transform;
				}
			}
			else
			{
				if (Selected == transform)
					flags |= ImGuiTreeNodeFlags.Selected;

				bool open = ImGui.TreeNodeEx(transform.Name, flags);
				
				if (ImGui.IsItemClicked())
				{
					Selected = transform;
				}
				
				if (open)
				{
					int childrenCount = transform.Children.Count;
					for (int i = 0; i < childrenCount; i++)
					{
						AddToTree(transform.Children[i]);
					}

					ImGui.TreePop();
				}
			}
		}
	}
}