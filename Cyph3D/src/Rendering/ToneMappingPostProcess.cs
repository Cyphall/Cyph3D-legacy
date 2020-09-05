using Cyph3D.GLObject;
using Cyph3D.Helper;
using Cyph3D.ResourceManagement;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.Rendering
{
	public class ToneMappingPostProcess : IPostProcessPass
	{
		private Framebuffer _framebuffer;
		private Texture _outputTexture;
		private ShaderProgram _shaderProgram;
		
		public ToneMappingPostProcess()
		{
			_framebuffer = new Framebuffer(Engine.Window.Size)
				.SetTexture(FramebufferAttachment.ColorAttachment0, new TextureCreateInfo
				{
					InternalFormat = InternalFormat.Rgb8
				}, out _outputTexture);

			_shaderProgram = Engine.GlobalResourceManager.RequestShaderProgram(
				new ShaderProgramRequest()
					.WithShader(ShaderType.VertexShader, "postProcessing/toneMapping")
					.WithShader(ShaderType.FragmentShader, "postProcessing/toneMapping"));
		}
		
		public Texture Render(Texture currentRenderResult, Texture renderRaw, Texture depth)
		{
			_shaderProgram.SetValue("colorTexture", currentRenderResult);
			
			_framebuffer.Bind();
			_shaderProgram.Bind();
			
			RenderHelper.DrawScreenQuad();
			
			return _outputTexture;
		}
	}
}