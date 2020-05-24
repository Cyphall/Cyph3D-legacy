using System;
using Cyph3D.Extension;
using GlmSharp;
using OpenToolkit.Windowing.GraphicsLibraryFramework;

namespace Cyph3D
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

		private vec2 _previousMousePos;

		public mat4 Projection { get; }
		public mat4 View => mat4.LookAt(Position,  Position + Orientation, new vec3(0, 1, 0));

		// x: phi (horizontal) 0 to 360
		// y: theta (vertical) -89 to 89
		private vec2 _sphericalCoords = new vec2(0, 0);
		public vec2 SphericalCoords
		{
			get => _sphericalCoords;
			set
			{
				_sphericalCoords = value;
				_orientationChanged = true;
			}
		}

		private vec3 _position;
		public vec3 Position
		{
			get => _position;
			set
			{
				_position = value;
				_orientationChanged = true;
			}
		}

		public Camera(vec3 position = default, vec2 sphericalCoords = default)
		{
			Position = position;

			SphericalCoords = sphericalCoords;

			Projection = MathExt.Perspective(100, (float)Engine.Window.Size.x / Engine.Window.Size.y, 0.0001f, 1000f);

			_previousMousePos = Engine.Window.CursorPos;
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
			if (Engine.Window.GuiOpen)
			{
				_previousMousePos = Engine.Window.CursorPos;
				return;
			}
			
			float ratio = (float)deltaTime * 2;

			if (Engine.Window.GetKey(Keys.LeftControl) == InputAction.Press)
			{
				ratio /= 20;
			}
			if (Engine.Window.GetKey(Keys.LeftShift) == InputAction.Press)
			{
				ratio *= 5;
			}

			if (Engine.Window.GetKey(Keys.W) == InputAction.Press)
			{
				Position += Orientation * ratio;
			}
			if (Engine.Window.GetKey(Keys.S) == InputAction.Press)
			{
				Position -= Orientation * ratio;
			}
			if (Engine.Window.GetKey(Keys.A) == InputAction.Press)
			{
				Position += SideOrientation * ratio;
			}
			if (Engine.Window.GetKey(Keys.D) == InputAction.Press)
			{
				Position -= SideOrientation * ratio;
			}

			// Context.Window.LeftClickCallback = LeftClick;
			// Context.Window.RightClickCallback = RightClick;
			
			vec2 currentMousePos = Engine.Window.CursorPos;
			
			vec2 mouseOffset = currentMousePos - _previousMousePos;
			SphericalCoords -= mouseOffset / 12;
			
			_previousMousePos = currentMousePos;
		}
	}
}