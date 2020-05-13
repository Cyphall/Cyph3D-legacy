﻿using System.Globalization;
using System.Numerics;
using Cyph3D.Misc;
using GlmSharp;
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
					Vector3 imGuiPosition = ConvertHelper.Convert(UIHierarchy.Selected.Position);
					if (ImGui.InputFloat3("Position", ref imGuiPosition, "%.6g", ImGuiInputTextFlags.EnterReturnsTrue))
					{
						UIHierarchy.Selected.Position = ConvertHelper.Convert(imGuiPosition);
					}
					
					Vector3 imGuiRotation = ConvertHelper.Convert(UIHierarchy.Selected.Rotation);
					if (ImGui.InputFloat3("Rotation", ref imGuiRotation, "%.6g", ImGuiInputTextFlags.EnterReturnsTrue))
					{
						UIHierarchy.Selected.Rotation = ConvertHelper.Convert(imGuiRotation);
					}
					
					Vector3 imGuiScale = ConvertHelper.Convert(UIHierarchy.Selected.Scale);
					if (ImGui.InputFloat3("Scale", ref imGuiScale, "%.6g", ImGuiInputTextFlags.EnterReturnsTrue))
					{
						UIHierarchy.Selected.Scale = ConvertHelper.Convert(imGuiScale);
					}
				}
			}
		}
	}
}