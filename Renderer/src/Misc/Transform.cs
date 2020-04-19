using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GlmSharp;

namespace Renderer.Misc
{
	[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
	public class Transform
	{
		protected vec3 _position;
		protected vec3 _rotation;
		protected vec3 _scale;
		protected mat4 _matrix;

		protected bool _shouldRecalculate = true;
		
		public vec3 Position
		{
			get => _position;
			set
			{
				if (value == _position) return;
				Debug.Assert(value.x != _position.x || value.y != _position.y || value.z != _position.z);
				
				_position = value;
				_shouldRecalculate = true;
			}
		}

		public vec3 Rotation
		{
			get => _rotation;
			set
			{
				if (value == _rotation) return;
				Debug.Assert(value.x != _rotation.x || value.y != _rotation.y || value.z != _rotation.z);
				
				_rotation = value;
				_shouldRecalculate = true;
			}
		}

		public vec3 Scale
		{
			get => _scale;
			set
			{
				if (value == _scale) return;
				Debug.Assert(value.x != _scale.x || value.y != _scale.y || value.z != _scale.z);
				
				_scale = value;
				_shouldRecalculate = true;
			}
		}

		public mat4 Matrix
		{
			get
			{
				// ReSharper disable once InvertIf
				if (_shouldRecalculate)
				{
					_matrix = mat4.Translate(_position) *
					         mat4.RotateZ(glm.Radians(_rotation.z)) *
					         mat4.RotateY(glm.Radians(_rotation.y)) *
					         mat4.RotateX(glm.Radians(_rotation.x)) *
					         mat4.Scale(_scale);
					_shouldRecalculate = false;
				}

				return _matrix;
			}
		}

		public Transform(vec3? position = null, vec3? rotation = null, vec3? scale = null)
		{
			Position = position ?? vec3.Zero;
			Rotation = rotation ?? vec3.Zero;
			Scale = scale ?? vec3.Ones;
		}
	}
}