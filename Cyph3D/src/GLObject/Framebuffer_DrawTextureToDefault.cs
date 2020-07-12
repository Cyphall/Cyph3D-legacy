using System;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public partial class Framebuffer
	{
		private static int _quadVAO;
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
			
			_quadVAO = GL.GenVertexArray();
			int quadVBO = GL.GenBuffer();
			GL.BindVertexArray(_quadVAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
			GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), IntPtr.Zero);
			GL.EnableVertexAttribArray(1);
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (IntPtr) (2 * sizeof(float)));
			
			_shaderProgram = new ShaderProgram("framebuffer/drawToDefault");
			_shaderProgram.SetValue("Texture", 0);
		}

		public static void DrawToDefault(ShaderProgram shader, params Texture[] textures)
		{
			int previousBlend = GL.GetInteger(GetPName.Blend);
			int previousBlendSrc= GL.GetInteger(GetPName.BlendSrc);
			int previousBlendDst= GL.GetInteger(GetPName.BlendDst);
			int previousBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
			int previousBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
			
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			
			shader.Bind();

			for (int i = 0; i < textures.Length; i++)
			{
				textures[i].Bind(i);
			}
			
			GL.BindVertexArray(_quadVAO);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			if (previousBlend == 1)
				GL.Enable(EnableCap.Blend);
			else 
				GL.Disable(EnableCap.Blend);
			GL.BlendFunc((BlendingFactor)previousBlendSrc, (BlendingFactor)previousBlendDst);
			GL.BlendEquationSeparate((BlendEquationMode)previousBlendEquationRgb, (BlendEquationMode)previousBlendEquationAlpha);
		}
		
		public static void DrawToDefault(Texture texture)
		{
			DrawToDefault(_shaderProgram, texture);
		}
	}
}