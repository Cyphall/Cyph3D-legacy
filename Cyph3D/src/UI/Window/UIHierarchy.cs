﻿using System.Numerics;
using Cyph3D.Misc;
using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public static class UIHierarchy
	{
		private const ImGuiTreeNodeFlags BASE_FLAGS = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;
		public static SceneObject Selected { get; private set; }

		public static void Show()
		{
			if (Selected != null && !Selected.IsValid) Selected = null;
			
			ImGui.SetNextWindowSize(new Vector2(300, 500));
			ImGui.SetNextWindowPos(new Vector2(0));

			if (ImGui.Begin("Hierarchy", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
			{
				int childrenCount = Engine.Scene.Root.Children.Count;
				for (int i = 0; i < childrenCount; i++)
				{
					AddToTree(Engine.Scene.Root.Children[i]);
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
				if (Selected == transform.Owner)
					flags |= ImGuiTreeNodeFlags.Selected;
				
				ImGui.TreeNodeEx(transform.Owner.Name, flags);

				if (ImGui.IsItemClicked())
				{
					Selected = transform.Owner;
				}
			}
			else
			{
				if (Selected == transform.Owner)
					flags |= ImGuiTreeNodeFlags.Selected;

				bool open = ImGui.TreeNodeEx(transform.Owner.Name, flags);
				
				if (ImGui.IsItemClicked())
				{
					Selected = transform.Owner;
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