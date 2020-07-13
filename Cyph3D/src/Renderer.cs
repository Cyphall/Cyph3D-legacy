using System;
using Cyph3D.Enumerable;
using Cyph3D.GLObject;
using Cyph3D.Lighting;
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
		private int _skyboxVAO;
		private ShaderProgram _lightingPassShader;
		private ShaderProgram _skyboxShader;
		private ShaderStorageBuffer<PointLight.NativeLightData> _pointLightsBuffer;
		private ShaderStorageBuffer<DirectionalLight.NativeLightData> _directionalLightsBuffer;
		
		public bool Debug { get; set; }

		public Renderer()
		{
			_gbuffer = new Framebuffer(Engine.Window.Size)
				.WithTexture(FramebufferAttachment.ColorAttachment0, (InternalFormat) All.Rgb32f, out _positionTexture)
				.WithTexture(FramebufferAttachment.ColorAttachment1, InternalFormat.Rgb8, out _normalTexture)
				.WithTexture(FramebufferAttachment.ColorAttachment2, InternalFormat.Rgb16f, out _colorTexture)
				.WithTexture(FramebufferAttachment.ColorAttachment3, InternalFormat.Rgba8, out _materialTexture)
				.WithTexture(FramebufferAttachment.DepthAttachment, (InternalFormat) All.DepthComponent24, out _depthTexture, TextureFiltering.Linear)
				.Complete();

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);

			GL.ClearColor(0, 0, 0, 0);

			GL.Enable(EnableCap.CullFace);
			GL.FrontFace(FrontFaceDirection.Ccw);
			
			_lightingPassShader = Engine.GlobalResourceManager.RequestShaderProgram("deferred/lightingPass");

			_lightingPassShader.SetValue("positionTexture", 0);
			_lightingPassShader.SetValue("normalTexture", 1);
			_lightingPassShader.SetValue("colorTexture", 2);
			_lightingPassShader.SetValue("materialTexture", 3);
			_lightingPassShader.SetValue("depthTexture", 4);
			
			
			float[] skyboxVertices = {
				-1.0f,  1.0f, -1.0f,
				-1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,
				 1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,
				
				-1.0f, -1.0f,  1.0f,
				-1.0f, -1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f,  1.0f,
				-1.0f, -1.0f,  1.0f,
				
				 1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,
				
				-1.0f, -1.0f,  1.0f,
				-1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f, -1.0f,  1.0f,
				-1.0f, -1.0f,  1.0f,
				
				-1.0f,  1.0f, -1.0f,
				 1.0f,  1.0f, -1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				-1.0f,  1.0f,  1.0f,
				-1.0f,  1.0f, -1.0f,
				
				-1.0f, -1.0f, -1.0f,
				-1.0f, -1.0f,  1.0f,
				 1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,
				-1.0f, -1.0f,  1.0f,
				 1.0f, -1.0f,  1.0f
			};
			
			_skyboxVAO = GL.GenVertexArray();
			int skyboxVBO = GL.GenBuffer();
			GL.BindVertexArray(_skyboxVAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, skyboxVBO);
			GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * sizeof(float), skyboxVertices, BufferUsageHint.StaticDraw);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			
			_skyboxShader = Engine.GlobalResourceManager.RequestShaderProgram("deferred/skybox");
			
			_pointLightsBuffer = new ShaderStorageBuffer<PointLight.NativeLightData>();
			_directionalLightsBuffer = new ShaderStorageBuffer<DirectionalLight.NativeLightData>();
		}

		public void Render(Camera camera)
		{
			_gbuffer.Bind();
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
			FirstPass(camera.View, camera.Projection, camera.Position);
			if (Engine.Scene.Skybox != null)
				SkyboxPass(camera.View, camera.Projection);
			LightingPass(camera.Position);
		}

		private void FirstPass(mat4 view, mat4 projection, vec3 viewPos)
		{
			_gbuffer.Bind();
			
			int objectCount = Engine.Scene.Objects.Count;
			for (int i = 0; i < objectCount; i++)
			{
				if (Engine.Scene.Objects[i] is MeshObject meshObject)
					meshObject.Render(view, projection, viewPos);
			}
		}
		
		private void SkyboxPass(mat4 view, mat4 projection)
		{
			GL.DepthMask(false);
			_gbuffer.Bind();
			
			_skyboxShader.Bind();
			
			_skyboxShader.SetValue("model", mat4.RotateY(glm.Radians(Engine.Scene.Skybox.Rotation)));
			_skyboxShader.SetValue("view", new mat4(new mat3(view)));
			_skyboxShader.SetValue("projection", projection);
			
			Engine.Scene.Skybox.Bind(0);
			
			GL.BindVertexArray(_skyboxVAO);
			
			GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
			GL.DepthMask(true);
		}

		private void LightingPass(vec3 viewPos)
		{
			_pointLightsBuffer.PutData(Engine.Scene.LightManager.PointLightsNative);
			_pointLightsBuffer.Bind(0);
			_directionalLightsBuffer.PutData(Engine.Scene.LightManager.DirectionalLightsNative);
			_directionalLightsBuffer.Bind(1);

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

			GL.Clear(ClearBufferMask.ColorBufferBit);

			_lightingPassShader.SetValue("viewPos", viewPos);

			_lightingPassShader.SetValue("debug", Debug ? 1 : 0);

			Framebuffer.DrawToDefault(
				_lightingPassShader,
				_positionTexture,
				_normalTexture,
				_colorTexture,
				_materialTexture,
				_depthTexture
			);
		}
	}
}