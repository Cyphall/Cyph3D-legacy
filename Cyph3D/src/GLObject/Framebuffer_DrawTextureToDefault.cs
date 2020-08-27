using Cyph3D.Helper;
using Cyph3D.ResourceManagement;
using Cyph3D.StateManagement;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public partial class Framebuffer
	{
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
			GLStateManager.Push();
			
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			
			if (clearFramebuffer)
			{
				GL.Clear(ClearBufferMask.ColorBufferBit);
			}
			
			GLStateManager.Blend = true;
			GLStateManager.BlendEquation = BlendEquationMode.FuncAdd;
			GLStateManager.BlendFunc = new GLBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			
			shader.Bind();
			
			RenderHelper.DrawScreenQuad();

			GLStateManager.Pop();
		}
		
		public static void DrawToDefault(Texture texture, bool clearFramebuffer = false)
		{
			_shaderProgram.SetValue("Texture", texture);
			DrawToDefault(_shaderProgram, clearFramebuffer);
		}
	}
}