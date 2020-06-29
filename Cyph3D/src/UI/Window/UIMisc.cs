using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Cyph3D.Extension;
using ImGuiNET;

// ReSharper disable PossibleLossOfFraction
namespace Cyph3D.UI.Window
{
	public static class UIMisc
	{
		private static bool _gbufferDebug;
		private static bool _showDemoWindow;
		private static bool _showRawQuaternion;

		private static List<string> _scenes;
		private static string _selectedScene;

		static UIMisc()
		{
			RescanFiles();
		}
		
		public static bool ShowRawQuaternion => _showRawQuaternion;

		public static void Show()
		{
			ImGui.SetNextWindowSize(new Vector2(400, 300));
			ImGui.SetNextWindowPos(new Vector2(Engine.Window.Size.x - 400, 0));
			
			if (ImGui.Begin("Misc", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
			{
				if (ImGui.Checkbox("GBuffer Debug View", ref _gbufferDebug))
				{
					Engine.Renderer.Debug = _gbufferDebug;
				}

				ImGui.Checkbox("Show Demo Window", ref _showDemoWindow);
			
				if (_showDemoWindow)
					ImGui.ShowDemoWindow();

				ImGui.Checkbox("Show Raw Quaternion", ref _showRawQuaternion);

				float cameraSpeed = Engine.Scene.Camera.Speed;
				if (ImGui.SliderFloat("Camera speed", ref cameraSpeed, 0, 10))
				{
					Engine.Scene.Camera.Speed = cameraSpeed;
				}
			
				ImGui.Separator();

				if (ImGui.BeginCombo("Scene", _selectedScene))
				{
					foreach (string scene in _scenes)
					{
						bool selected = scene == _selectedScene;
						if (ImGui.Selectable(scene, selected))
						{
							_selectedScene = scene;
						}

						if (selected)
						{
							ImGui.SetItemDefaultFocus();
						}
					}

					ImGui.EndCombo();
				}
				
				ImGui.SameLine();
				
				if (ImGui.Button("Refresh"))
				{
					RescanFiles();
				}

				if (ImGui.Button("Load scene"))
				{
					Engine.Scene.Dispose();
					Engine.Scene = Scene.Load(_selectedScene);
				}
			
				ImGui.SameLine();

				if (ImGui.Button("Save scene"))
				{
					Engine.Scene.Save();
				}
				
				ImGui.Separator();
				
				string skyboxName = Engine.Scene.Skybox?.Name ?? "None";
				ImGui.InputText("Skybox", ref skyboxName, 0, ImGuiInputTextFlags.ReadOnly);
				if (ImGui.BeginDragDropTarget())
				{
					ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("SkyboxDragDrop");
					if (payload.IsValid())
					{
						string name = (string) GCHandle.FromIntPtr(payload.Data).Target;
						Engine.Scene.ResourceManager.RequestSkybox(name, skybox => Engine.Scene.Skybox = skybox);
					}
					ImGui.EndDragDropTarget();
				}

				if (Engine.Scene.Skybox != null)
				{
					float skyboxRotation = Engine.Scene.Skybox.Rotation;
					if (ImGui.SliderFloat("Skybox rotation", ref skyboxRotation, 0, 360))
					{
						Engine.Scene.Skybox.Rotation = skyboxRotation;
					}
				}
			}
		}
		
		private static void RescanFiles()
		{
			_scenes = new List<string>();

			foreach (string file in Directory.GetFiles("resources/scenes"))
			{
				_scenes.Add(Path.GetFileNameWithoutExtension(file));
			}

			_selectedScene = _scenes.FirstOrDefault();
		}
	}
}