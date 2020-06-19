using System.Numerics;
using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public static class UITest
	{
		public static void Show()
		{
			ImGui.SetNextWindowSize(new Vector2(200, 200));

			if (ImGui.Begin("Test", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
			{
				if (ImGui.BeginPopupContextWindow("HierarchyAction"))
				{
					if (ImGui.BeginMenu("Create"))
					{
						if (ImGui.MenuItem("PointLight"))
						{
							
						}
						if (ImGui.MenuItem("DirectionalLight"))
						{
							
						}
						ImGui.EndMenu();
					}
					ImGui.EndPopup();
				}
			}
		}
	}
}