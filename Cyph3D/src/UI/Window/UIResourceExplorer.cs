using System;
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
		
		public static void Show()
		{
			ImGui.SetNextWindowSize(new Vector2(300, 300));
			ImGui.SetNextWindowPos(new Vector2(400, 1080-300));
			
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
			ImGui.BeginChild("list", Vector2.Zero, true);
			switch (_currentResourceType)
			{
				case ResourceType.Meshes:
					foreach (string meshPath in Directory.GetFiles("resources/meshes/", "*.obj", SearchOption.AllDirectories))
					{
						string meshName = PathUtility.GetPathWithoutExtension(meshPath.Remove("resources/meshes/"));

						ImGui.Selectable(meshName);
						
						if (ImGui.BeginDragDropSource())
						{
							GCHandle handle = GCHandle.Alloc(meshName);
							ImGui.SetDragDropPayload("MeshDragDrop", GCHandle.ToIntPtr(handle), (uint)sizeof(GCHandle));
							ImGui.EndDragDropSource();
						}
					}
					break;
				case ResourceType.Materials:
					break;
			}
			ImGui.EndChild();
			ImGui.EndGroup();
			
			ImGui.End();
		}

		private enum ResourceType
		{
			Meshes,
			Materials
		}
	}
}