using System;
using System.Collections.Generic;
using System.Numerics;
using Cyph3D.Extension;
using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public static class UIHierarchy
	{
		private const ImGuiTreeNodeFlags BASE_FLAGS = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;

		private static Transform _currentlyDragged;
		
		private static Queue<Tuple<Transform, Transform>> _hierarchyChangeQueue = new Queue<Tuple<Transform, Transform>>();

		public static void Show()
		{
			ImGui.SetNextWindowSize(new Vector2(300, 500));
			ImGui.SetNextWindowPos(new Vector2(0));

			if (ImGui.Begin("Hierarchy", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
			{
				if (ImGui.TreeNodeEx(Engine.Scene.Name, BASE_FLAGS | ImGuiTreeNodeFlags.DefaultOpen))
				{
					if (ImGui.BeginDragDropTarget())
					{
						if (ImGui.AcceptDragDropPayload("HIERARCHY_CHANGE").IsValid())
						{
							_hierarchyChangeQueue.Enqueue(new Tuple<Transform, Transform>(_currentlyDragged, Engine.Scene.Root));
						}
						ImGui.EndDragDropTarget();
					}
					
					int childrenCount = Engine.Scene.Root.Children.Count;
					for (int i = 0; i < childrenCount; i++)
					{
						AddToTree(Engine.Scene.Root.Children[i]);
					}

					ImGui.TreePop();
				}

				ProcessHierarchyChanges();
			
				ImGui.End();
			}
		}

		private static void ProcessHierarchyChanges()
		{
			while (_hierarchyChangeQueue.Count > 0)
			{
				(Transform dragged, Transform newParent) = _hierarchyChangeQueue.Dequeue();

				if (newParent.Parent == dragged) return;
				if (newParent == dragged.Parent) return;

				dragged.Parent = newParent;
			}
		}

		private static void AddToTree(Transform transform)
		{
			ImGui.PushID(transform.Owner.GUID);
			ImGuiTreeNodeFlags flags = BASE_FLAGS;
			
			if (UIInspector.Selected == transform.Owner)
				flags |= ImGuiTreeNodeFlags.Selected;
			
			if (transform.Children.Count == 0)
				flags |= ImGuiTreeNodeFlags.Leaf;
			
			bool open = ImGui.TreeNodeEx(transform.Owner.Name, flags);
			
			if (ImGui.IsItemClicked())
			{
				UIInspector.Selected = transform.Owner;
			}
			
			if (ImGui.BeginDragDropSource())
			{
				ImGui.SetDragDropPayload("HIERARCHY_CHANGE", IntPtr.Zero, 0);
				_currentlyDragged = transform;
				ImGui.Text(transform.Owner.Name);
				ImGui.EndDragDropSource();
			}

			if (ImGui.BeginDragDropTarget())
			{
				if (ImGui.AcceptDragDropPayload("HIERARCHY_CHANGE").IsValid())
				{
					_hierarchyChangeQueue.Enqueue(new Tuple<Transform, Transform>(_currentlyDragged, transform));
				}
				ImGui.EndDragDropTarget();
			}
			
			if (open)
			{
				int childrenCount = transform.Children.Count;
				for (int i = 0; i < childrenCount; i++)
				{
					AddToTree(transform.Children[i]);
				}

				ImGui.TreePop();
			}
			
			ImGui.PopID();
		}

		private static unsafe bool IsValid(this ImGuiPayloadPtr payload)
		{
			return payload.NativePtr != null;
		}
	}
}