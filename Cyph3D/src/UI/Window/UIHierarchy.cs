using System.Numerics;
using Cyph3D.Misc;
using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public static class UIHierarchy
	{
		private const ImGuiTreeNodeFlags BASE_FLAGS = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;

		public static void Show()
		{
			ImGui.SetNextWindowSize(new Vector2(300, 500));
			ImGui.SetNextWindowPos(new Vector2(0));

			if (ImGui.Begin("Hierarchy", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
			{
				if (ImGui.TreeNodeEx(Engine.Scene.Name, BASE_FLAGS | ImGuiTreeNodeFlags.DefaultOpen))
				{
					int childrenCount = Engine.Scene.Root.Children.Count;
					for (int i = 0; i < childrenCount; i++)
					{
						AddToTree(Engine.Scene.Root.Children[i]);
					}

					ImGui.TreePop();
				}
			
				ImGui.End();
			}
		}
		
		private static void AddToTree(Transform transform)
		{
			ImGuiTreeNodeFlags flags = BASE_FLAGS;
			
			if (transform.Children.Count == 0)
			{
				flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
				if (UIInspector.Selected == transform.Owner)
					flags |= ImGuiTreeNodeFlags.Selected;
				
				ImGui.TreeNodeEx(transform.Owner.Name, flags);

				if (ImGui.IsItemClicked())
				{
					UIInspector.Selected = transform.Owner;
				}
			}
			else
			{
				if (UIInspector.Selected == transform.Owner)
					flags |= ImGuiTreeNodeFlags.Selected;

				bool open = ImGui.TreeNodeEx(transform.Owner.Name, flags);
				
				if (ImGui.IsItemClicked())
				{
					UIInspector.Selected = transform.Owner;
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