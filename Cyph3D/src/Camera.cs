using System;
using GlmSharp;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using Renderer.GLObject;
using Renderer.Misc;

namespace Renderer
{
	public class Camera
	{
		private bool _orientationChanged = true;
		
		private vec3 _orientation = vec3.Zero;
		private vec3 Orientation
		{
			get
			{
				if (_orientationChanged) RecalculateOrientation();
				return _orientation;
			}
		}

		private vec3 _sideOrientation = vec3.Zero;
		private vec3 SideOrientation
		{
			get
			{
				if (_orientationChanged) RecalculateOrientation();
				return _sideOrientation;
			}
		}

		private ivec2 _winCenter;

		private mat4 Projection { get; }
		private mat4 View => mat4.LookAt(Position,  Position + Orientation, new vec3(0, 1, 0));

		// x: phi (horizontal) 0 to 360
		// y: theta (vertical) -89 to 89
		private vec2 _sphericalCoords = new vec2(0, 0);
		private vec2 SphericalCoords
		{
			get => _sphericalCoords;
			set
			{
				_sphericalCoords = value;
				_orientationChanged = true;
			}
		}

		private vec3 _position;
		private vec3 Position
		{
			get => _position;
			set
			{
				_position = value;
				_orientationChanged = true;
			}
		}

		private Renderer _renderer = new Renderer();

		public Camera(vec3 position = default, vec2 sphericalCoords = default)
		{
			Position = position;

			SphericalCoords = sphericalCoords;

			Projection = MathExt.Perspective(100, (float)Context.Window.Size.x / Context.Window.Size.y, 0.0001f, 1000f);

			_winCenter = Context.Window.Size / 2;
			Context.Window.CursorPos = _winCenter;
		}

		private void RecalculateOrientation()
		{
			_sphericalCoords = new vec2
			{
				x = MathExt.Modulo(_sphericalCoords.x, 360),
				y = Math.Clamp(_sphericalCoords.y, -89, 89)
			};

			vec2 sphericalCoordsRadians = vec2.Radians(_sphericalCoords);

			_orientation = new vec3
			{
				x = glm.Cos(sphericalCoordsRadians.y) * glm.Sin(sphericalCoordsRadians.x),
				y = glm.Sin(sphericalCoordsRadians.y),
				z = glm.Cos(sphericalCoordsRadians.y) * glm.Cos(sphericalCoordsRadians.x)
			};

			_sideOrientation = glm.Cross(new vec3(0, 1, 0), _orientation).Normalized;

			_orientationChanged = false;
		}

		public void Update(double deltaTime)
		{
			float ratio = (float)deltaTime * 2;

			if (Context.Window.GetKey(Keys.LeftControl) == InputAction.Press)
			{
				ratio /= 20;
			}
			if (Context.Window.GetKey(Keys.LeftShift) == InputAction.Press)
			{
				ratio *= 5;
			}

			if (Context.Window.GetKey(Keys.W) == InputAction.Press)
			{
				Position += Orientation * ratio;
			}
			if (Context.Window.GetKey(Keys.S) == InputAction.Press)
			{
				Position -= Orientation * ratio;
			}
			if (Context.Window.GetKey(Keys.A) == InputAction.Press)
			{
				Position += SideOrientation * ratio;
			}
			if (Context.Window.GetKey(Keys.D) == InputAction.Press)
			{
				Position -= SideOrientation * ratio;
			}

			// Context.Window.LeftClickCallback = LeftClick;
			// Context.Window.RightClickCallback = RightClick;
			
			vec2 mouseOffset = Context.Window.CursorPos;
			mouseOffset -= _winCenter;

			SphericalCoords += new vec2(-mouseOffset.x / 12, -mouseOffset.y / 12);
			
			Context.Window.CursorPos = _winCenter;
		}

		private void LeftClick(InputAction action, KeyModifiers mods)
		{
			if (action != InputAction.Press) return;
			
			Context.ObjectContainer.Add(
				new RenderObject(
					Material.GetOrLoad("Sci-Fi/SpaceCase1", true),
					Mesh.GetOrLoad("cube"),
					position: Position + Orientation,
					angularVelocity: new vec3(0, 5f, 0),
					scale: new vec3(0.5f)
				)
			);
		}

		private void RightClick(InputAction action, KeyModifiers mods)
		{
			if (action != InputAction.Press) return;
			
			Context.ObjectContainer.Add(
				new RenderObject(
					Material.GetOrLoad("Metals/OrnateBrass", true),
					Mesh.GetOrLoad("teapot"),
					position: Position + Orientation,
					angularVelocity: new vec3(0, 0, 0)
				)
			);
		}

		public void Render()
		{
			_renderer.Render(View, Projection, Position);
		}
	}
}