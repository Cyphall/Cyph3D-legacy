using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Cyph3D.Extension;
using Cyph3D.GLObject;
using Cyph3D.Lighting;
using Cyph3D.Misc;
using ImGuiNET;

// ReSharper disable PossibleLossOfFraction
namespace Cyph3D.UI.Window
{
	public static class UIInspector
	{
		public static object Selected { get; set; }

		static UIInspector()
		{
			Engine.SceneChanged += () => Selected = null;
		}
		
		public static void Show()
		{
			ImGui.SetNextWindowSize(new Vector2(400, Engine.Window.Size.y - Engine.Window.Size.y / 2));
			ImGui.SetNextWindowPos(new Vector2(0, Engine.Window.Size.y / 2));
			
			if (ImGui.Begin("Inspector", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
			{
				if (Selected != null)
				{
					switch (Selected)
					{
						case SceneObject sceneObject:
							ShowSceneObject(sceneObject);
							break;
						case Material material:
							ShowMaterial(material);
							break;
					}
				}
			}
		}

		private static void ShowSceneObject(SceneObject selected)
		{
			byte[] imGuiName = new byte[50];
			Encoding.UTF8.GetBytes(selected.Name.ToCharArray()).CopyTo(imGuiName, 0);
			
			if (ImGui.InputText("Name", imGuiName, 50))
			{
				string temp = Encoding.UTF8.GetString(imGuiName);
				int end = temp.IndexOf('\0');
				selected.Name = temp.Substring(0, end);
			}
			
			ImGui.Text("Transform");
            
            Vector3 imGuiPosition = ConvertHelper.Convert(selected.Transform.Position);
            if (ImGui.DragFloat3("Position", ref imGuiPosition, 0.01f, 0, 0, "%.3g"))
            {
	            selected.Transform.Position = ConvertHelper.Convert(imGuiPosition);
            }

            if (UIMisc.ShowRawQuaternion)
            {
	            Vector4 imGuiQuaternion = ConvertHelper.QuatConvert(selected.Transform.Rotation);
	            if (ImGui.DragFloat4("Quaternion", ref imGuiQuaternion, 0.01f, 0, 0, "%.3g"))
	            {
		            selected.Transform.Rotation = ConvertHelper.QuatConvert(imGuiQuaternion);
	            }
            }
            else
            {
	            Vector3 imGuiRotation = ConvertHelper.Convert(selected.Transform.EulerRotation);
	            if (ImGui.DragFloat3("Rotation", ref imGuiRotation, 0.01f, 0, 0, "%.3g"))
	            {
		            selected.Transform.EulerRotation = ConvertHelper.Convert(imGuiRotation);
	            }
            }
            
            Vector3 imGuiScale = ConvertHelper.Convert(selected.Transform.Scale);
            if (ImGui.DragFloat3("Scale", ref imGuiScale, 0.01f, 0, 0, "%.3g"))
            {
	            selected.Transform.Scale = ConvertHelper.Convert(imGuiScale);
            }
            
            ImGui.Separator();
            
            ImGui.Text("Properties");

            switch (Selected)
            {
            	case MeshObject meshObject:
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

                    string meshName = meshObject.Mesh?.Name ?? "None";
                    ImGui.InputText("Mesh", ref meshName, 0, ImGuiInputTextFlags.ReadOnly);
                    if (ImGui.BeginDragDropTarget())
                    {
	                    ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("MeshDragDrop");
	                    if (payload.IsValid())
	                    {
		                    string newMesh = (string) GCHandle.FromIntPtr(payload.Data).Target;
		                    meshObject.Mesh = Mesh.GetOrLoad(newMesh);
	                    }
	                    ImGui.EndDragDropTarget();
                    }
                    
                    string materialName = meshObject.Material?.Name ?? "None";
                    ImGui.InputText("Material", ref materialName, 0, ImGuiInputTextFlags.ReadOnly);
                    if (ImGui.BeginDragDropTarget())
                    {
	                    ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("MaterialDragDrop");
	                    if (payload.IsValid())
	                    {
		                    string newMaterial = (string) GCHandle.FromIntPtr(payload.Data).Target;
		                    meshObject.Material = Material.GetOrLoad(newMaterial);
	                    }
	                    ImGui.EndDragDropTarget();
                    }
            		break;
            	case Light pointLight:
            		Vector3 imGuiSrgbColor = ConvertHelper.Convert(pointLight.SrgbColor);
            		if (ImGui.ColorEdit3("Color", ref imGuiSrgbColor))
            		{
            			pointLight.SrgbColor = ConvertHelper.Convert(imGuiSrgbColor);
            		}
            		
            		float intensity = pointLight.Intensity;
            		if (ImGui.DragFloat("Intensity", ref intensity, 0.01f, 0, float.MaxValue, "%.6g"))
            		{
            			pointLight.Intensity = intensity;
            		}
            		break;
            }
		}

		private static void ShowMaterial(Material material)
		{
			
		}
	}
}