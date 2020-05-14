using System;
using System.Collections.Generic;
using Cyph3D.Misc;
using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public static class UIDebug
	{
		private static bool _gbufferDebug;
		private static bool _showDemoWindow;

		private static Dictionary<string, Func<Scene>> _scenes = new Dictionary<string, Func<Scene>>
		{
			{"Dungeon", ScenePreset.Dungeon},
			{"Spaceship", ScenePreset.Spaceship},
			{"Test Quaternion", ScenePreset.TestQuat},
			{"Test Cube", ScenePreset.TestCube},
			{"Test Sphere", ScenePreset.TestSphere},
		};
		private static string _selectedScene = "Dungeon";
		
		public static void Show()
		{
			if (ImGui.Checkbox("GBuffer Debug View", ref _gbufferDebug))
			{
				Engine.Renderer.Debug = _gbufferDebug;
			}

			ImGui.Checkbox("Show Demo Window", ref _showDemoWindow);
			
			if (_showDemoWindow)
				ImGui.ShowDemoWindow();
			
			ImGui.Separator();

			if (ImGui.BeginCombo("Scene", _selectedScene))
			{
				foreach (string scene in _scenes.Keys)
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

			if (ImGui.Button("Load scene"))
			{
				Engine.Scene.Dispose();
				Engine.Scene = _scenes[_selectedScene]();
			}
		}
	}
}