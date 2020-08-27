using System;
using System.Collections.Generic;
using Cyph3D.GLObject;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.StateManagement
{
	public static class GLStateManager
	{
		private static Stack<Stack<Action>> _restoreStateActions = new Stack<Stack<Action>>();

		public static void Push()
		{
			_restoreStateActions.Push(new Stack<Action>());
		}
		
		public static void Pop()
		{
			Stack<Action> actions = _restoreStateActions.Pop();
			while (actions.TryPop(out Action action))
			{
				action.Invoke();
			}
		}

		#region GL Enablables

		public static bool DepthTest
		{
			set
			{
				bool oldValue = GL.IsEnabled(EnableCap.DepthTest);

				if (value)
					GL.Enable(EnableCap.DepthTest);
				else
					GL.Disable(EnableCap.DepthTest);
			
				_restoreStateActions.Peek().Push(() =>
				{
					if (oldValue)
						GL.Enable(EnableCap.DepthTest);
					else
						GL.Disable(EnableCap.DepthTest);
				});
			}
		}

		public static bool CullFace
		{
			set
			{
				bool oldValue = GL.IsEnabled(EnableCap.CullFace);

				if (value)
					GL.Enable(EnableCap.CullFace);
				else
					GL.Disable(EnableCap.CullFace);
			
				_restoreStateActions.Peek().Push(() =>
				{
					if (oldValue)
						GL.Enable(EnableCap.CullFace);
					else
						GL.Disable(EnableCap.CullFace);
				});
			}
		}
		
		public static bool Blend
		{
			set
			{
				bool oldValue = GL.IsEnabled(EnableCap.Blend);

				if (value)
					GL.Enable(EnableCap.Blend);
				else
					GL.Disable(EnableCap.Blend);
			
				_restoreStateActions.Peek().Push(() =>
				{
					if (oldValue)
						GL.Enable(EnableCap.Blend);
					else
						GL.Disable(EnableCap.Blend);
				});
			}
		}
		
		public static bool DepthMask
		{
			set
			{
				bool oldValue = GL.GetBoolean(GetPName.DepthWritemask);

				GL.DepthMask(value);
			
				_restoreStateActions.Peek().Push(() =>
				{
					GL.DepthMask(oldValue);
				});
			}
		}
		
		#endregion
		
		#region Enums

		public static DepthFunction DepthFunc
		{
			set
			{
				DepthFunction oldValue = (DepthFunction)GL.GetInteger(GetPName.DepthFunc);
			
				GL.DepthFunc(value);
			
				_restoreStateActions.Peek().Push(() =>
				{
					GL.DepthFunc(oldValue);
				});
			}
		}
		
		public static FrontFaceDirection FrontFace
		{
			set
			{
				FrontFaceDirection oldValue = (FrontFaceDirection)GL.GetInteger(GetPName.FrontFace);
			
				GL.FrontFace(value);
			
				_restoreStateActions.Peek().Push(() =>
				{
					GL.FrontFace(oldValue);
				});
			}
		}
		
		public static GLBlendFunc BlendFunc
		{
			set
			{
				BlendingFactor oldValue1 = (BlendingFactor)GL.GetInteger(GetPName.BlendSrc);
				BlendingFactor oldValue2 = (BlendingFactor)GL.GetInteger(GetPName.BlendDst);
			
				GL.BlendFunc(value.SFactor, value.DFactor);
			
				_restoreStateActions.Peek().Push(() =>
				{
					GL.BlendFunc(oldValue1, oldValue2);
				});
			}
		}
		
		public static BlendEquationMode BlendEquation
		{
			set
			{
				BlendEquationMode oldValue1 = (BlendEquationMode)GL.GetInteger(GetPName.BlendEquationRgb);
				BlendEquationMode oldValue2 = (BlendEquationMode)GL.GetInteger(GetPName.BlendEquationAlpha);
			
				GL.BlendEquation(value);
			
				_restoreStateActions.Peek().Push(() =>
				{
					GL.BlendEquationSeparate(oldValue1, oldValue2);
				});
			}
		}
		
		#endregion

		#region Others

		public static GlViewport Viewport
		{
			set
			{
				int[] oldValues = new int[4];
				GL.GetInteger(GetPName.Viewport, oldValues);

				GL.Viewport(value.X, value.Y, value.Width, value.Height);
			
				_restoreStateActions.Peek().Push(() =>
				{
					GL.Viewport(oldValues[0], oldValues[1], oldValues[2], oldValues[3]);
				});
			}
		}
		
		public static GLClearColor ClearColor
		{
			set
			{
				float[] oldValues = new float[4];
				GL.GetFloat(GetPName.ColorClearValue, oldValues);

				GL.ClearColor(value.R, value.G, value.B, value.A);
			
				_restoreStateActions.Peek().Push(() =>
				{
					GL.ClearColor(oldValues[0], oldValues[1], oldValues[2], oldValues[3]);
				});
			}
		}
		
		#endregion
	}
}