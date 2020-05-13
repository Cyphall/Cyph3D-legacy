using System.Numerics;
using Cyph3D.Misc;
using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public class UIHierarchy : IUIWindow
	{
		public void Show()
		{
			ImGui.SetNextWindowSize(new Vector2(200, 500));
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
			if (transform.Children.Count == 0)
				ImGui.BulletText(transform.Name);
			else
			{
				if (ImGui.TreeNode(transform.Name))
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