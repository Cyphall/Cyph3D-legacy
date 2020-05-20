using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GlmSharp;

namespace Cyph3D.Misc
{
	[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
	public class Transform
	{
		private vec3 _position;
		private vec3 _rotation;
		private vec3 _scale;
		private mat4 _matrix;
		
		private Transform _parent;
		public SceneObject Owner { get; }
		
		public List<Transform> Children { get; } = new List<Transform>();

		private bool _matrixChanged;
		private bool _worldMatrixChanged;
		
		private mat4 _cachedWorldMatrix;
		
		private void MatrixChanged()
		{
			if (_matrixChanged) return;
			_matrixChanged = true;
			WorldMatrixChanged();
		}
		
		private void WorldMatrixChanged()
		{
			if (_worldMatrixChanged) return;
			_worldMatrixChanged = true;
			
			for (int i = 0; i < Children.Count; i++)
			{
				Children[i].WorldMatrixChanged();
			}
		}

		public Transform Parent
		{
			get => _parent;
			set
			{
				if (this == value) return;
				if (Parent == value) return;
				if (value == null) throw new InvalidOperationException("Cannot remove Transform parent, only changing it is allowed");
				
				_parent?.Children.Remove(this);
				_parent = value;
				_parent.Children.Add(this);
				
				WorldMatrixChanged();
			}
		}
		
		public vec3 Position
		{
			get => _position;
			set
			{
				if (value == _position) return;
				
				_position = value;
				MatrixChanged();
			}
		}

		public vec3 WorldPosition => (WorldMatrix * new vec4(0, 0, 0, 1)).xyz;

		public vec3 Rotation
		{
			get => _rotation;
			set
			{
				if (value == _rotation) return;
				
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

		public Transform(SceneObject owner = null, Transform parent = null, vec3? position = null, vec3? rotation = null, vec3? scale = null)
		{
			Owner = owner;
			Parent = parent;
			Position = position ?? vec3.Zero;
			Rotation = rotation ?? vec3.Zero;
			Scale = scale ?? vec3.Ones;
			
			MatrixChanged();
		}
	}
}