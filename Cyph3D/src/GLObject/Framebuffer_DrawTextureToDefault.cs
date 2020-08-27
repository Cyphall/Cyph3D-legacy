using Cyph3D.Helper;
using Cyph3D.ResourceManagement;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public partial class Framebuffer
	{
		private static VertexArray _quadVAO;
		private static VertexBuffer<float> _quadVBO;
		private static ShaderProgram _shaderProgram;

		public static void InitDrawToDefault()
		{
			_shaderProgram = Engine.GlobalResourceManager.RequestShaderProgram(
				new ShaderProgramRequest()
					.WithShader(ShaderType.VertexShader,
						"internal/framebuffer/drawToDefault")
					.WithShader(ShaderType.FragmentShader,
						"internal/framebuffer/drawToDefault")
			);
		}

		public static void DrawToDefault(ShaderProgram shader, bool clearFramebuffer = false)
		{
			int previousBlend = GL.GetInteger(GetPName.Blend);
			int previousBlendSrc= GL.GetInteger(GetPName.BlendSrc);
			int previousBlendDst= GL.GetInteger(GetPName.BlendDst);
			int previousBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
			int previousBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
			
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			
			if (clearFramebuffer)
			{
				GL.ClearColor(0, 0, 0, 0);
				GL.Clear(ClearBufferMask.ColorBufferBit);
			}
			
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			
			shader.Bind();
			
			RenderHelper.DrawScreenQuad();

			if (previousBlend == 1)
				GL.Enable(EnableCap.Blend);
			else 
				GL.Disable(EnableCap.Blend);
			GL.BlendFunc((BlendingFactor)previousBlendSrc, (BlendingFactor)previousBlendDst);
			GL.BlendEquationSeparate((BlendEquationMode)previousBlendEquationRgb, (BlendEquationMode)previousBlendEquationAlpha);
		}
		
		public static void DrawToDefault(Texture texture, bool clearFramebuffer = false)
		{
			_shaderProgram.SetValue("Texture", texture);
			DrawToDefault(_shaderProgram, clearFramebuffer);
		}
	}
}