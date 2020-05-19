using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public static class UIDebug
	{
		private static bool _gbufferDebug;
		private static bool _showDemoWindow;

		private static List<string> _scenes;
		private static string _selectedScene;

		static UIDebug()
		{
			RefreshList();
		}
		
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

			if (ImGui.Button("Load scene"))
			{
				Engine.Scene.Dispose();
				Engine.Scene = Scene.Load(_selectedScene);
			}
			
			ImGui.SameLine();

			if (ImGui.Button("Refresh list"))
			{
				RefreshList();
			}

			ImGui.Separator();

			if (ImGui.Button("Save current scene"))
			{
				Engine.Scene.Save();
			}
		}

		private static void RefreshList()
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