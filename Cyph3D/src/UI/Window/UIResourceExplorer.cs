using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Cyph3D.Extension;
using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public static unsafe class UIResourceExplorer
	{
		private static ResourceType _currentResourceType;
		
		private static List<string> _meshes = new List<string>();
		private static List<string> _materials = new List<string>();
		private static List<string> _skyboxes = new List<string>();

		static UIResourceExplorer()
		{
			RescanFiles();
		}
		
		public static void Show()
		{
			ImGui.SetNextWindowSize(new Vector2(500, 300));
			ImGui.SetNextWindowPos(new Vector2(400, Engine.Window.Size.y - 300));
			
			if (!ImGui.Begin("Resources", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize)) return;

			ImGui.BeginChild("type", new Vector2(100, 0));
			foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
			{
				if (ImGui.Selectable(Enum.GetName(typeof(ResourceType), resourceType), resourceType == _currentResourceType))
				{
					_currentResourceType = resourceType;
				}
			}
			ImGui.EndChild();
			
			ImGui.SameLine();
			
			ImGui.BeginGroup();

			Vector2 buttonSize = ImGui.CalcTextSize("Refresh") + ImGui.GetStyle().FramePadding*2;
			ImGui.Spacing();
			ImGui.SameLine(ImGui.GetContentRegionAvail().X - buttonSize.X);
			if (ImGui.Button("Refresh"))
			{
				RescanFiles();
			}
			
			ImGui.BeginChild("list", Vector2.Zero, true);
			switch (_currentResourceType)
			{
				case ResourceType.Meshes:
					foreach (string mesh in _meshes)
					{
						ImGui.Selectable(mesh);
						
						if (ImGui.BeginDragDropSource())
						{
							GCHandle handle = GCHandle.Alloc(mesh);
							ImGui.SetDragDropPayload("MeshDragDrop", GCHandle.ToIntPtr(handle), (uint)sizeof(GCHandle));
							ImGui.EndDragDropSource();
						}
					}
					break;
				case ResourceType.Materials:
					foreach (string material in _materials)
					{
						ImGui.Selectable(material);
						
						if (ImGui.BeginDragDropSource())
						{
							GCHandle handle = GCHandle.Alloc(material);
							ImGui.SetDragDropPayload("MaterialDragDrop", GCHandle.ToIntPtr(handle), (uint)sizeof(GCHandle));
							ImGui.EndDragDropSource();
						}
					}
					break;
				case ResourceType.Skyboxes:
					foreach (string skybox in _skyboxes)
					{
						ImGui.Selectable(skybox);
						
						if (ImGui.BeginDragDropSource())
						{
							GCHandle handle = GCHandle.Alloc(skybox);
							ImGui.SetDragDropPayload("SkyboxDragDrop", GCHandle.ToIntPtr(handle), (uint)sizeof(GCHandle));
							ImGui.EndDragDropSource();
						}
					}
					break;
			}
			ImGui.EndChild();
			
			ImGui.EndGroup();
			
			ImGui.End();
		}

		private static void RescanFiles()
		{
			_meshes.Clear();
			foreach (string meshPath in Directory.GetFiles("resources/meshes/", "*.obj", SearchOption.AllDirectories))
			{
				_meshes.Add(PathUtility.GetPathWithoutExtension(meshPath.Remove("resources/meshes/")));
			}
			_meshes.Sort();
			
			_materials.Clear();
			FindMaterial("resources/materials/");
			_materials.Sort();
			
			_skyboxes.Clear();
			FindSkyboxes("resources/skyboxes/");
			_skyboxes.Sort();
		}

		private static void FindMaterial(string path)
		{
			if (File.Exists($"{path}/material.json"))
			{
				_materials.Add(path.Remove("resources/materials/"));
			}

			foreach (string folder in Directory.GetDirectories(path))
			{
				FindMaterial(folder.Replace(@"\", "/"));
			}
		}
		
		private static void FindSkyboxes(string path)
		{
			if (File.Exists($"{path}/skybox.json"))
			{
				_skyboxes.Add(path.Remove("resources/skyboxes/"));
			}

			foreach (string folder in Directory.GetDirectories(path))
			{
				FindSkyboxes(folder.Replace(@"\", "/"));
			}
		}

		private enum ResourceType
		{
			Meshes,
			Materials,
			Skyboxes
		}
	}
}