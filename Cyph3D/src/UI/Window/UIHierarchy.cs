using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Cyph3D.Extension;
using Cyph3D.Lighting;
using Cyph3D.Misc;
using GlmSharp;
using ImGuiNET;

// ReSharper disable PossibleLossOfFraction
namespace Cyph3D.UI.Window
{
	public static class UIHierarchy
	{
		private const ImGuiTreeNodeFlags BASE_FLAGS = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;

		private static GCHandle? _currentlyDraggedHandle;
		
		private static Queue<Tuple<Transform, Transform>> _hierarchyOrderChangeQueue = new Queue<Tuple<Transform, Transform>>();
		private static Queue<Transform> _hierarchyDeleteQueue = new Queue<Transform>();
		private static Queue<Type> _hierarchyAddQueue = new Queue<Type>();

		public static void Show()
		{
			ImGui.SetNextWindowSize(new Vector2(400, Engine.Window.Size.y / 2));
			ImGui.SetNextWindowPos(new Vector2(0));

			if (ImGui.Begin("Hierarchy", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
			{
				// Main context menu to add elements to the scene
				if (ImGui.BeginPopupContextWindow("HierarchyAction"))
				{
					if (ImGui.BeginMenu("Create"))
					{
						if (ImGui.MenuItem("PointLight"))
						{
							_hierarchyAddQueue.Enqueue(typeof(PointLight));
						}
						if (ImGui.MenuItem("DirectionalLight"))
						{
							_hierarchyAddQueue.Enqueue(typeof(DirectionalLight));
						}
						if (ImGui.MenuItem("MeshObject"))
						{
							_hierarchyAddQueue.Enqueue(typeof(MeshObject));
						}
						ImGui.EndMenu();
					}

					bool canDelete = UIInspector.Selected != null && UIInspector.Selected is SceneObject;
					if (ImGui.MenuItem("Delete", canDelete))
					{
						_hierarchyDeleteQueue.Enqueue(((SceneObject)UIInspector.Selected)?.Transform);
					}
					
					ImGui.EndPopup();
				}
				
				//Hierarchy tree creation
				if (!AddRootToTree() && _currentlyDraggedHandle != null)
				{
					_currentlyDraggedHandle.Value.Free();
					_currentlyDraggedHandle = null;
				}

				ProcessHierarchyChanges();
			
				ImGui.End();
			}
		}

		private static void ProcessHierarchyChanges()
		{
			while (_hierarchyOrderChangeQueue.Count > 0)
			{
				(Transform dragged, Transform newParent) = _hierarchyOrderChangeQueue.Dequeue();

				if (newParent.Parent == dragged) return;
				if (newParent == dragged.Parent) return;

				dragged.Parent = newParent;
			}
			
			while (_hierarchyDeleteQueue.Count > 0)
			{
				SceneObject sceneObject = _hierarchyDeleteQueue.Dequeue().Owner;
				if (UIInspector.Selected == sceneObject)
				{
					UIInspector.Selected = null;
				}
				Engine.Scene.Remove(sceneObject);
			}
			
			while (_hierarchyAddQueue.Count > 0)
			{
				Type type = _hierarchyAddQueue.Dequeue();
				
				if (type == typeof(PointLight))
				{
					Engine.Scene.Add(new PointLight(Engine.Scene.Root, vec3.Ones, 1));
				}
				else if (type == typeof(DirectionalLight))
				{
					Engine.Scene.Add(new DirectionalLight(Engine.Scene.Root, vec3.Ones, 1));
				}
				else if (type == typeof(MeshObject))
				{
					Engine.Scene.Add(new MeshObject(Engine.Scene.Root, null, null));
				}
			}
		}
		
		private static bool AddRootToTree()
		{
			bool open = ImGui.TreeNodeEx(Engine.Scene.Name, BASE_FLAGS | ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader);
			
			//Make root a dragdrop target for hierarchy change
			ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("HierarchyOrderChange");
			if (payload.IsValid())
			{
				Transform dropped = (Transform) GCHandle.FromIntPtr(payload.Data).Target;
				_hierarchyOrderChangeQueue.Enqueue(new Tuple<Transform, Transform>(dropped, Engine.Scene.Root));
			}
			ImGui.EndDragDropTarget();
			
			bool currentlyDragging = false;
			
			if (open)
			{
				//Add root children
				int childrenCount = Engine.Scene.Root.Children.Count;
				for (int i = 0; i < childrenCount; i++)
				{
					currentlyDragging |= AddObjectToTree(Engine.Scene.Root.Children[i]);
				}

				ImGui.TreePop();
			}

			return currentlyDragging;
		}
		
		private static unsafe bool AddObjectToTree(Transform transform)
		{
			ImGui.PushID(transform.Owner.GUID);
			ImGuiTreeNodeFlags flags = BASE_FLAGS;
			
			if (UIInspector.Selected == transform.Owner)
				flags |= ImGuiTreeNodeFlags.Selected;
			
			if (transform.Children.Count == 0)
				flags |= ImGuiTreeNodeFlags.Leaf;
			
			bool open = ImGui.TreeNodeEx(transform.Owner.Name, flags);
			
			//Select the item on click
			if (ImGui.IsItemClicked())
			{
				UIInspector.Selected = transform.Owner;
			}
			
			//Make the item a drag drop source and target for hierarchy change
			bool currentlyDragging = false;
			if (ImGui.BeginDragDropSource())
			{
				currentlyDragging = true;
				ImGui.Text(transform.Owner.Name);
							
				_currentlyDraggedHandle ??= GCHandle.Alloc(transform);
				ImGui.SetDragDropPayload("HierarchyOrderChange", GCHandle.ToIntPtr(_currentlyDraggedHandle.Value), (uint)sizeof(IntPtr));
				ImGui.EndDragDropSource();
			}
			if (ImGui.BeginDragDropTarget())
			{
				ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("HierarchyOrderChange");
				if (payload.IsValid())
				{
					Transform dropped = (Transform) GCHandle.FromIntPtr(payload.Data).Target;
					_hierarchyOrderChangeQueue.Enqueue(new Tuple<Transform, Transform>(dropped, transform));
				}
				ImGui.EndDragDropTarget();
			}
			
			//Draw item children if the item is opened
			if (open)
			{
				int childrenCount = transform.Children.Count;
				for (int i = 0; i < childrenCount; i++)
				{
					currentlyDragging |= AddObjectToTree(transform.Children[i]);
				}

				ImGui.TreePop();
			}
			
			ImGui.PopID();

			return currentlyDragging;
		}
	}
}