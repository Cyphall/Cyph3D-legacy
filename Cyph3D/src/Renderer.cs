using System;
using Cyph3D.Enumerable;
using Cyph3D.GLObject;
using Cyph3D.Lighting;
using Cyph3D.ResourceManagement;
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
		private Texture _geometryNormalTexture;
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
				.WithTexture(FramebufferAttachment.ColorAttachment0, new TextureSetting
				{
					InternalFormat = (InternalFormat) All.Rgb32f
				}, out _positionTexture)
				.WithTexture(FramebufferAttachment.ColorAttachment1, new TextureSetting
				{
					InternalFormat = InternalFormat.Rgb16f
				}, out _normalTexture)
				.WithTexture(FramebufferAttachment.ColorAttachment2, new TextureSetting
				{
					InternalFormat = InternalFormat.Rgb16f
				}, out _colorTexture)
				.WithTexture(FramebufferAttachment.ColorAttachment3, new TextureSetting
				{
					InternalFormat = InternalFormat.Rgba8
				}, out _materialTexture)
				.WithTexture(FramebufferAttachment.ColorAttachment4, new TextureSetting
				{
					InternalFormat = InternalFormat.Rgb16f
				}, out _geometryNormalTexture)
				.WithTexture(FramebufferAttachment.DepthAttachment, new TextureSetting
				{
					InternalFormat = (InternalFormat) All.DepthComponent24,
					Filtering = TextureFiltering.Linear
				}, out _depthTexture)
				.Complete();

			GL.ClearColor(0, 0, 0, 0);

			_lightingPassShader = Engine.GlobalResourceManager.RequestShaderProgram(
				new ShaderProgramRequest()
					.WithShader(ShaderType.VertexShader,
						"lightingPass")
					.WithShader(ShaderType.FragmentShader,
						"lightingPass")
				);
				
			
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

			_skyboxShader = Engine.GlobalResourceManager.RequestShaderProgram(
				new ShaderProgramRequest()
					.WithShader(ShaderType.VertexShader,
						"internal/skybox/skybox")
					.WithShader(ShaderType.FragmentShader,
						"internal/skybox/skybox")
				);

			_pointLightsBuffer = new ShaderStorageBuffer<PointLight.NativeLightData>();
			_directionalLightsBuffer = new ShaderStorageBuffer<DirectionalLight.NativeLightData>();
		}

		public void Render(Camera camera)
		{
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);
			
			Engine.Scene.LightManager.UpdateShadowMaps();
			
			GL.Enable(EnableCap.CullFace);
			GL.FrontFace(FrontFaceDirection.Ccw);
			
			GL.Viewport(0, 0, Engine.Window.Size.x, Engine.Window.Size.y);
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
			
			for (int i = 0; i < Engine.Scene.Objects.Count; i++)
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
			
			_skyboxShader.SetValue("skybox", Engine.Scene.Skybox);
			
			GL.BindVertexArray(_skyboxVAO);
			
			GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
			GL.DepthMask(true);
		}

		private void LightingPass(vec3 viewPos)
		{
			GL.Disable(EnableCap.DepthTest);
			
			_pointLightsBuffer.PutData(Engine.Scene.LightManager.PointLightsNative);
			_pointLightsBuffer.Bind(0);
			_directionalLightsBuffer.PutData(Engine.Scene.LightManager.DirectionalLightsNative);
			_directionalLightsBuffer.Bind(1);

			_lightingPassShader.SetValue("viewPos", viewPos);

			_lightingPassShader.SetValue("debug", Debug ? 1 : 0);
			
			_lightingPassShader.SetValue("positionTexture", _positionTexture);
			_lightingPassShader.SetValue("normalTexture", _normalTexture);
			_lightingPassShader.SetValue("colorTexture", _colorTexture);
			_lightingPassShader.SetValue("materialTexture", _materialTexture);
			_lightingPassShader.SetValue("geometryNormalTexture", _geometryNormalTexture);
			_lightingPassShader.SetValue("depthTexture", _depthTexture);

			Framebuffer.DrawToDefault(_lightingPassShader, true);
		}
	}
}