using Cyph3D.Helper;
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
			float[] quadVertices =
			{
				// positions   // texCoords
				-1.0f, 1.0f, 0.0f, 1.0f,
				-1.0f, -1.0f, 0.0f, 0.0f,
				1.0f, -1.0f, 1.0f, 0.0f,

				-1.0f, 1.0f, 0.0f, 1.0f,
				1.0f, -1.0f, 1.0f, 0.0f,
				1.0f, 1.0f, 1.0f, 1.0f
			};
			
			_quadVAO = new VertexArray();
			
			_quadVBO = new VertexBuffer<float>(false, Stride.Get<float>(4));
			_quadVBO.PutData(quadVertices);
			
			_quadVAO.RegisterAttrib(_quadVBO, 0, 2, VertexAttribType.Float, 0);
			_quadVAO.RegisterAttrib(_quadVBO, 1, 2, VertexAttribType.Float, 2 * sizeof(float));
			
			_shaderProgram = Engine.GlobalResourceManager.RequestShaderProgram("internal/framebuffer/drawToDefault");
			_shaderProgram.SetValue("Texture", 0);
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
				GL.Clear(ClearBufferMask.ColorBufferBit);
			
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			
			shader.Bind();
			
			_quadVAO.Bind();
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

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