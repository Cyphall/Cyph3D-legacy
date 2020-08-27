using System;
using Cyph3D.Enumerable;
using Cyph3D.GLObject;
using Cyph3D.Helper;
using Cyph3D.Lighting;
using Cyph3D.ResourceManagement;
using Cyph3D.StateManagement;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.Rendering
{
	public class Renderer : IDisposable
	{
		private Framebuffer _gbuffer;
		private Texture _normalTexture;
		private Texture _colorTexture;
		private Texture _materialTexture;
		private Texture _geometryNormalTexture;
		private Texture _depthTexture;
		private VertexArray _skyboxVAO;
		private VertexBuffer<float> _skyboxVBO;
		private ShaderProgram _lightingPassShader;
		private ShaderProgram _skyboxShader;
		private ShaderStorageBuffer<PointLight.NativeLightData> _pointLightsBuffer;
		private ShaderStorageBuffer<DirectionalLight.NativeLightData> _directionalLightsBuffer;
		
		public bool Debug { get; set; }

		public Renderer()
		{
			_gbuffer = new Framebuffer(Engine.Window.Size)
				.SetTexture(FramebufferAttachment.ColorAttachment0, new TextureSetting
				{
					InternalFormat = InternalFormat.Rgb16f
				}, out _normalTexture)
				.SetTexture(FramebufferAttachment.ColorAttachment1, new TextureSetting
				{
					InternalFormat = InternalFormat.Rgb16f
				}, out _colorTexture)
				.SetTexture(FramebufferAttachment.ColorAttachment2, new TextureSetting
				{
					InternalFormat = InternalFormat.Rgba8
				}, out _materialTexture)
				.SetTexture(FramebufferAttachment.ColorAttachment3, new TextureSetting
				{
					InternalFormat = InternalFormat.Rgb16f
				}, out _geometryNormalTexture)
				.SetTexture(FramebufferAttachment.DepthAttachment, new TextureSetting
				{
					InternalFormat = (InternalFormat) All.DepthComponent24,
					Filtering = TextureFiltering.Linear
				}, out _depthTexture);

			_lightingPassShader = Engine.GlobalResourceManager.RequestShaderProgram(
				new ShaderProgramRequest()
					.WithShader(ShaderType.VertexShader,
						"lightingPass")
					.WithShader(ShaderType.FragmentShader,
						"lightingPass")
				);

			_skyboxVAO = new VertexArray();
			
			_skyboxVBO = new VertexBuffer<float>(false, Stride.Get<float>(3));
			_skyboxVBO.PutData(new []{
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
			});
			
			_skyboxVAO.RegisterAttrib(_skyboxVBO, 0, 3, VertexAttribType.Float, 0);

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
			ShadowMapPass();
			
			UpdateLightBuffers();
			
			_gbuffer.ClearAll();

			FirstPass(camera.View, camera.Projection, camera.Position);
			if (Engine.Scene.Skybox != null)
				SkyboxPass(camera.View, camera.Projection);
			LightingPass(camera.Position, camera.View, camera.Projection);
		}
		
		private void UpdateLightBuffers()
		{
			_pointLightsBuffer.PutData(Engine.Scene.LightManager.PointLightsNative);
			_directionalLightsBuffer.PutData(Engine.Scene.LightManager.DirectionalLightsNative);
		}

		private void ShadowMapPass()
		{
			GLStateManager.Push();

			GLStateManager.DepthTest = true;
			GLStateManager.DepthFunc = DepthFunction.Lequal;
			
			Engine.Scene.LightManager.UpdateShadowMaps();
			
			GLStateManager.Pop();
		}

		private void FirstPass(mat4 view, mat4 projection, vec3 viewPos)
		{
			GLStateManager.Push();
			
			GLStateManager.DepthTest = true;
			GLStateManager.DepthFunc = DepthFunction.Lequal;
			
			GLStateManager.CullFace = true;
			GLStateManager.FrontFace = FrontFaceDirection.Ccw;
			
			_gbuffer.Bind();
			
			for (int i = 0; i < Engine.Scene.Objects.Count; i++)
			{
				if (Engine.Scene.Objects[i] is MeshObject meshObject)
					meshObject.Render(view, projection, viewPos);
			}
			
			GLStateManager.Pop();
		}
		
		private void SkyboxPass(mat4 view, mat4 projection)
		{
			GLStateManager.Push();

			GLStateManager.DepthTest = true;
			GLStateManager.DepthFunc = DepthFunction.Lequal;
			GLStateManager.DepthMask = false;
			
			_gbuffer.Bind();
			
			_skyboxShader.Bind();
			
			_skyboxShader.SetValue("model", mat4.RotateY(glm.Radians(Engine.Scene.Skybox.Rotation)));
			_skyboxShader.SetValue("view", new mat4(new mat3(view)));
			_skyboxShader.SetValue("projection", projection);
			
			_skyboxShader.SetValue("skybox", Engine.Scene.Skybox);
			
			_skyboxVAO.Bind();
			
			GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
			
			GLStateManager.Pop();
		}

		private void LightingPass(vec3 viewPos, mat4 view, mat4 projection)
		{
			_pointLightsBuffer.Bind(0);
			_directionalLightsBuffer.Bind(1);

			_lightingPassShader.SetValue("viewPos", viewPos);

			_lightingPassShader.SetValue("debug", Debug ? 1 : 0);
			_lightingPassShader.SetValue("viewProjectionInv", (projection * view).Inverse);
			
			_lightingPassShader.SetValue("normalTexture", _normalTexture);
			_lightingPassShader.SetValue("colorTexture", _colorTexture);
			_lightingPassShader.SetValue("materialTexture", _materialTexture);
			_lightingPassShader.SetValue("geometryNormalTexture", _geometryNormalTexture);
			_lightingPassShader.SetValue("depthTexture", _depthTexture);

			Framebuffer.DrawToDefault(_lightingPassShader, true);
		}

		public void Dispose()
		{
			_gbuffer?.Dispose();
			_normalTexture?.Dispose();
			_colorTexture?.Dispose();
			_materialTexture?.Dispose();
			_geometryNormalTexture?.Dispose();
			_depthTexture?.Dispose();
			_skyboxVAO?.Dispose();
			_skyboxVBO?.Dispose();
			_lightingPassShader?.Dispose();
			_skyboxShader?.Dispose();
			_pointLightsBuffer?.Dispose();
			_directionalLightsBuffer?.Dispose();
		}
	}
}