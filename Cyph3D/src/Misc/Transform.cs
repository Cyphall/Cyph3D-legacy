using System.Collections.Generic;
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
		private Transform _parent;
		public List<Transform> Children { get; } = new List<Transform>();

		private bool _matrixChanged;
		private bool _worldMatrixChanged;
		
		private mat4 _cachedWorldMatrix;

		public string Name { get; protected set; }
		
		protected void MatrixChanged()
		{
			_matrixChanged = true;
			WorldMatrixChanged();
		}
		
		protected void WorldMatrixChanged()
		{
			_worldMatrixChanged = true;
			int childrenCount = Children.Count;
			for (int i = 0; i < childrenCount; i++)
			{
				Children[i].WorldMatrixChanged();
			}
		}

		public Transform Parent
		{
			get => _parent;
			set
			{
				if (this == Context.SceneRoot) return;
				if (this == value) return;
				
				_parent?.Children.Remove(this);
				_parent = value ?? Context.SceneRoot;
				_parent?.Children.Add(this);
			}
		}
		
		public vec3 Position
		{
			get => _position;
			set
			{
				if (value == _position) return;
				Debug.Assert(value.x != _position.x || value.y != _position.y || value.z != _position.z);
				
				_position = value;
				MatrixChanged();
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
				MatrixChanged();
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
				MatrixChanged();
			}
		}

		public mat4 Matrix
		{
			get
			{
				// ReSharper disable once InvertIf
				if (_matrixChanged)
				{
					_matrix = mat4.Translate(_position) *
					         mat4.RotateZ(glm.Radians(_rotation.z)) *
					         mat4.RotateY(glm.Radians(_rotation.y)) *
					         mat4.RotateX(glm.Radians(_rotation.x)) *
					         mat4.Scale(_scale);
					_matrixChanged = false;
				}

				return _matrix;
			}
		}

		public mat4 WorldMatrix
		{
			get
			{
				if (_worldMatrixChanged)
				{
					_cachedWorldMatrix = Parent != null ? Parent.WorldMatrix * Matrix : Matrix;
					_worldMatrixChanged = false;
				}
				return _cachedWorldMatrix;
			}
		}

		public Transform(string name = null, Transform parent = null, vec3? position = null, vec3? rotation = null, vec3? scale = null)
		{
			Name = name ?? "Object";
			Parent = parent;
			Position = position ?? vec3.Zero;
			Rotation = rotation ?? vec3.Zero;
			Scale = scale ?? vec3.Ones;
			
			MatrixChanged();
		}
	}
}