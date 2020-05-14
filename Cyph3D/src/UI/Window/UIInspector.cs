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
					if (ImGui.DragFloat3("Position", ref imGuiPosition, 0.01f, 0, 0, "%.6g"))
					{
						UIHierarchy.Selected.Transform.Position = ConvertHelper.Convert(imGuiPosition);
					}
					
					Vector3 imGuiRotation = ConvertHelper.Convert(UIHierarchy.Selected.Transform.Rotation);
					if (ImGui.DragFloat3("Rotation", ref imGuiRotation, 0.01f, 0, 0, "%.6g"))
					{
						UIHierarchy.Selected.Transform.Rotation = ConvertHelper.Convert(imGuiRotation);
					}
					
					Vector3 imGuiScale = ConvertHelper.Convert(UIHierarchy.Selected.Transform.Scale);
					if (ImGui.DragFloat3("Scale", ref imGuiScale, 0.01f, 0, 0, "%.6g"))
					{
						UIHierarchy.Selected.Transform.Scale = ConvertHelper.Convert(imGuiScale);
					}

					switch (UIHierarchy.Selected)
					{
						case MeshObject meshObject:
						{
							Vector3 imGuiVelocity = ConvertHelper.Convert(meshObject.Velocity);
							if (ImGui.DragFloat3("Velocity", ref imGuiVelocity, 0.01f, 0, 0, "%.6g"))
							{
								meshObject.Velocity = ConvertHelper.Convert(imGuiVelocity);
							}
						
							Vector3 imGuiAngularVelocity = ConvertHelper.Convert(meshObject.AngularVelocity);
							if (ImGui.DragFloat3("Angular Velocity", ref imGuiAngularVelocity, 0.01f, 0, 0, "%.6g"))
							{
								meshObject.AngularVelocity = ConvertHelper.Convert(imGuiAngularVelocity);
							}
							break;
						}
						case PointLight pointLight:
							Vector3 imGuiColor = ConvertHelper.Convert(pointLight.Color);
							if (ImGui.ColorEdit3("Color", ref imGuiColor))
							{
								pointLight.Color = ConvertHelper.Convert(imGuiColor);
							}
							
							float intensity = pointLight.Intensity;
							if (ImGui.DragFloat("Intensity", ref intensity, 0.01f, 0, float.MaxValue, "%.6g"))
							{
								pointLight.Intensity = intensity;
							}
							break;
					}
				}
			}
		}
	}
}