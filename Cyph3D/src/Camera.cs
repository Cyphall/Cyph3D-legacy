using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GLFW;
using GlmSharp;
using OpenGL;
using Renderer.GLObject;

namespace Renderer
{
	public class Camera
	{
		private bool _orientationChanged = true;
		private bool _leftClickPreviousState;
		private bool _rightClickPreviousState;
		
		private vec3 _orientation = vec3.Zero;
		private vec3 Orientation
		{
			get
			{
				if (_orientationChanged) RecalculateOrientation();
				return _orientation;
			}
			set => _orientation = value;
		}

		private vec3 _sideOrientation = vec3.Zero;
		private vec3 SideOrientation
		{
			get
			{
				if (_orientationChanged) RecalculateOrientation();
				return _sideOrientation;
			}
			set => _sideOrientation = value;
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

		public Camera(vec3 position = default)
		{
			Position = position;
			
			SphericalCoords = new vec2(340.16702f, -0.58333683f);

			Projection = mat4.Perspective(glm.Radians(80f), (float)Context.WindowSize.x / Context.WindowSize.y, 0.0001f, 1000f);

			_winCenter = Context.WindowSize / 2;
			Glfw.SetCursorPosition(Context.Window, _winCenter.x, _winCenter.y);
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
			float ratio = (float)deltaTime;

			if (Glfw.GetKey(Context.Window, Keys.LeftControl) == InputState.Press)
			{
				ratio /= 20;
			}
			if (Glfw.GetKey(Context.Window, Keys.LeftShift) == InputState.Press)
			{
				ratio *= 5;
			}

			if (Glfw.GetKey(Context.Window, Keys.W) == InputState.Press)
			{
				Position += Orientation * ratio;
			}
			if (Glfw.GetKey(Context.Window, Keys.S) == InputState.Press)
			{
				Position -= Orientation * ratio;
			}
			if (Glfw.GetKey(Context.Window, Keys.A) == InputState.Press)
			{
				Position += SideOrientation * ratio;
			}
			if (Glfw.GetKey(Context.Window, Keys.D) == InputState.Press)
			{
				Position -= SideOrientation * ratio;
			}

			bool leftClickCurrentState = Glfw.GetMouseButton(Context.Window, MouseButton.Left) == InputState.Press;
			if (!_leftClickPreviousState && leftClickCurrentState)
			{
				LeftClick();
			}
			_leftClickPreviousState = leftClickCurrentState;
			
			bool rightClickPreviousState = Glfw.GetMouseButton(Context.Window, MouseButton.Right) == InputState.Press;
			if (!_rightClickPreviousState && rightClickPreviousState)
			{
				RightClick();
			}
			_rightClickPreviousState = rightClickPreviousState;
			
			dvec2 mouseOffset = new dvec2();
			Glfw.GetCursorPosition(Context.Window, out mouseOffset.x, out mouseOffset.y);
			mouseOffset -= _winCenter;

			SphericalCoords += new vec2((float)(-mouseOffset.x / 12.0), (float)(-mouseOffset.y / 12.0));
			
			Glfw.SetCursorPosition(Context.Window, _winCenter.x, _winCenter.y);
		}

		private void LeftClick()
		{
			Context.ObjectContainer.Add(
				new RenderObject(
					Material.GetOrLoad("Sci-Fi/SpaceCase1", true),
					Mesh.GetOrLoad("cube"),
					Position + Orientation,
					angularVelocity: new vec3(0, 5f, 0)
				)
			);
		}

		private void RightClick()
		{
			Context.ObjectContainer.Add(
				new RenderObject(
					Material.GetOrLoad("Metals/OrnateBrass", true),
					Mesh.GetOrLoad("teapot"),
					Position + Orientation,
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