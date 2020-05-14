using System;
using Cyph3D.Enumerable;
using Cyph3D.GLObject;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D
{
	public class Renderer
	{
		private Framebuffer _gbuffer;
		private Texture _positionTexture;
		private Texture _normalTexture;
		private Texture _colorTexture;
		private Texture _materialTexture;
		private Texture _depthTexture;
		private int _quadVAO;
		private ShaderProgram _lightingPassShader;
		private ShaderStorageBuffer _lightsBuffer;
		
		public bool Debug { get; set; }

		public Renderer()
		{
			_gbuffer = new Framebuffer(out _positionTexture, Engine.Window.Size, (InternalFormat) All.Rgb32f);
			_normalTexture = _gbuffer.AddTexture(FramebufferAttachment.ColorAttachment1, InternalFormat.Rgb8);
			_colorTexture = _gbuffer.AddTexture(FramebufferAttachment.ColorAttachment2, InternalFormat.Rgb16f);
			_materialTexture = _gbuffer.AddTexture(FramebufferAttachment.ColorAttachment3, InternalFormat.Rgba8);
			_depthTexture = _gbuffer.AddTexture(FramebufferAttachment.DepthAttachment, (InternalFormat) All.DepthComponent24, TextureFiltering.Linear);
			
			_gbuffer.SetDrawBuffers(
				DrawBuffersEnum.ColorAttachment0,
				DrawBuffersEnum.ColorAttachment1,
				DrawBuffersEnum.ColorAttachment2,
				DrawBuffersEnum.ColorAttachment3
			);

			GL.Enable(EnableCap.DepthTest);

			GL.ClearColor(0, 0, 0, 1);

			GL.Enable(EnableCap.CullFace);
			GL.FrontFace(FrontFaceDirection.Ccw);

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


			_lightingPassShader = ShaderProgram.Get("deferred/lightingPassPBR");

			_lightingPassShader.SetValue("positionTexture", 0);
			_lightingPassShader.SetValue("normalTexture", 1);
			_lightingPassShader.SetValue("colorTexture", 2);
			_lightingPassShader.SetValue("materialTexture", 3);
			_lightingPassShader.SetValue("depthTexture", 4);
			
			_lightingPassShader.Unbind();
			
			
			_lightsBuffer = new ShaderStorageBuffer(0);
		}

		public void Render(Camera camera)
		{
			FirstPass(camera.View, camera.Projection, camera.Position);
			LightingPass(camera.Position);
		}

		private void FirstPass(mat4 view, mat4 projection, vec3 viewPos)
		{
			_gbuffer.Bind();

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			int objectCount = Engine.Scene.Objects.Count;
			for (int i = 0; i < objectCount; i++)
			{
				if (Engine.Scene.Objects[i] is MeshObject meshObject)
					meshObject.Render(view, projection, viewPos);
			}
		}

		private void LightingPass(vec3 viewPos)
		{
			_lightsBuffer.PutData(Engine.Scene.LightManager.PointLightsNative);

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

			GL.Clear(ClearBufferMask.ColorBufferBit);

			_lightingPassShader.Bind();

			_lightingPassShader.SetValue("viewPos", viewPos);

			_lightingPassShader.SetValue("debug", Debug ? 1 : 0);

			GL.ActiveTexture(TextureUnit.Texture0);
			_positionTexture.Bind();
			GL.ActiveTexture(TextureUnit.Texture1);
			_normalTexture.Bind();
			GL.ActiveTexture(TextureUnit.Texture2);
			_colorTexture.Bind();
			GL.ActiveTexture(TextureUnit.Texture3);
			_materialTexture.Bind();
			GL.ActiveTexture(TextureUnit.Texture4);
			_depthTexture.Bind();

			GL.BindVertexArray(_quadVAO);

			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			
			_lightingPassShader.Unbind();
		}
	}
}