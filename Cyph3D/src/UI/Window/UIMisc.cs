using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ImGuiNET;

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
			ImGui.SetNextWindowSize(new Vector2(350, 200));
			ImGui.SetNextWindowPos(new Vector2(1570, 0));
			
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