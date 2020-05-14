using System.Numerics;
using Cyph3D.Misc;
using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public static class UIInspector
	{
		public static void Show()
		{
			ImGui.SetNextWindowSize(new Vector2(300, 580));
			ImGui.SetNextWindowPos(new Vector2(0, 500));
			
			if (ImGui.Begin("Inspector", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
			{
				if (UIHierarchy.Selected != null)
				{
					Vector3 imGuiPosition = ConvertHelper.Convert(UIHierarchy.Selected.Transform.Position);
					if (ImGui.InputFloat3("Position", ref imGuiPosition, "%.6g", ImGuiInputTextFlags.EnterReturnsTrue))
					{
						UIHierarchy.Selected.Transform.Position = ConvertHelper.Convert(imGuiPosition);
					}
					
					Vector3 imGuiRotation = ConvertHelper.Convert(UIHierarchy.Selected.Transform.Rotation);
					if (ImGui.InputFloat3("Rotation", ref imGuiRotation, "%.6g", ImGuiInputTextFlags.EnterReturnsTrue))
					{
						UIHierarchy.Selected.Transform.Rotation = ConvertHelper.Convert(imGuiRotation);
					}
					
					Vector3 imGuiScale = ConvertHelper.Convert(UIHierarchy.Selected.Transform.Scale);
					if (ImGui.InputFloat3("Scale", ref imGuiScale, "%.6g", ImGuiInputTextFlags.EnterReturnsTrue))
					{
						UIHierarchy.Selected.Transform.Scale = ConvertHelper.Convert(imGuiScale);
					}
				}
			}
		}
	}
}