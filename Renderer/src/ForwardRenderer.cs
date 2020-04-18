using System;
using System.Linq;
using GlmSharp;
using OpenGL;

namespace Renderer
{
	public class ForwardRenderer : IRenderer
	{
		private Framebuffer _framebuffer;
		private Texture _renderTexture;
		private uint _quadVAO;
		private ShaderProgram _shaderProgram;
		private vec2 _pixelSize =  1 / new vec2(Context.WindowSize);
		
		private bool _blend;
		private bool Blend
		{
			set
			{
				if (_blend == value) return;
				
				_blend = value;
				if (_blend)
					Gl.Enable(EnableCap.Blend);
				else
					Gl.Disable(EnableCap.Blend);
			}
		}
		
		public ForwardRenderer()
		{
			_framebuffer = new Framebuffer(out _renderTexture, Context.WindowSize, InternalFormat.Rgb16f, PixelFormat.Rgb);
			_framebuffer.AddRenderbuffer(FramebufferAttachment.DepthAttachment, InternalFormat.DepthComponent);
			
			Gl.Enable(EnableCap.DepthTest);
			Gl.DepthFunc(DepthFunction.Lequal);

			Gl.Enable(EnableCap.CullFace);
			Gl.FrontFace(FrontFaceDirection.Ccw);

			Gl.BlendFunc(BlendingFactor.One, BlendingFactor.One);
			Gl.BlendEquation(BlendEquationMode.FuncAdd);
			
			float[] quadVertices = {
				// positions   // texCoords
				-1.0f,  1.0f,  0.0f, 1.0f,
				-1.0f, -1.0f,  0.0f, 0.0f,
				1.0f, -1.0f,  1.0f, 0.0f,

				-1.0f,  1.0f,  0.0f, 1.0f,
				1.0f, -1.0f,  1.0f, 0.0f,
				1.0f,  1.0f,  1.0f, 1.0f
			};
			
			_quadVAO = Gl.GenVertexArray();
			uint quadVBO = Gl.GenBuffer();
			Gl.BindVertexArray(_quadVAO);
			Gl.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
			Gl.BufferData(BufferTarget.ArrayBuffer, (uint)quadVertices.Length * sizeof(float), quadVertices, BufferUsage.StaticDraw);
			Gl.EnableVertexAttribArray(0);
			Gl.VertexAttribPointer(0, 2, VertexAttribType.Float, false, 4 * sizeof(float), IntPtr.Zero);
			Gl.EnableVertexAttribArray(1);
			Gl.VertexAttribPointer(1, 2, VertexAttribType.Float, false, 4 * sizeof(float), (IntPtr)(2 * sizeof(float)));
			
			_shaderProgram = ShaderProgram.Get("forward/postprocessing");
		}

		public void Render(mat4 view, mat4 projection, vec3 viewPos)
		{
			RenderPass(view, projection, viewPos);
			DrawPass();
		}

		private void RenderPass(mat4 view, mat4 projection, vec3 viewPos)
		{
			_framebuffer.Bind();
			
			Gl.Enable(EnableCap.DepthTest);
			
			Gl.ClearColor(0, 0, 0, 1);
			Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			int objectCount = Context.ObjectContainer.Count;
			for (int i = 0; i < objectCount; i++)
			{
				RenderObject obj = Context.ObjectContainer[i];
				
				Blend = false;
				for (int j = 0; j < objectCount; j++)
				{
					RenderObject light = Context.ObjectContainer[j];
					if (light == obj) continue;
					
					obj.Render(view, projection, viewPos, light.Transform.Position);
					Blend = true;
				}
			}
		}

		private void DrawPass()
		{
			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			
			Gl.Disable(EnableCap.DepthTest);
			
			Gl.ClearColor(0, 0, 0, 255);
			Gl.Clear(ClearBufferMask.ColorBufferBit);

			_shaderProgram.Bind();
			_shaderProgram.SetValue("pixelSize", _pixelSize);
			
			Gl.BindVertexArray(_quadVAO);

			_renderTexture.Bind();
			
			Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
		}
	}
}