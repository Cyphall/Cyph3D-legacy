using System;
using System.Runtime.InteropServices;
using Cyph3D.GLObject;
using Cyph3D.Misc;
using Cyph3D.ResourceManagement;
using Cyph3D.StateManagement;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.Lighting
{
	public class PointLight : Light
	{
		private const int SIZE = 1024;
		private const float NEAR = 0.01f;
		private const float FAR = 100;
		
		public Cubemap ShadowMap { get; private set; }
		private Framebuffer _shadowMapFb;
		private ShaderProgram _shadowMapProgram;
		
		private bool _castShadows;

		public mat4[] ViewProjections { get; private set; } = new mat4[6];
		private static mat4 _projection = mat4.Perspective(glm.Radians(90f), 1, NEAR, FAR);

		public PointLight(Transform parent, vec3 srgbColor, float intensity, string name = "PointLight", vec3? position = null, vec3? rotation = null, bool castShadows = false):
			base(parent, srgbColor, intensity, name, position, rotation)
		{
			_shadowMapProgram = Engine.GlobalResourceManager.RequestShaderProgram(
				new ShaderProgramRequest()
					.WithShader(ShaderType.VertexShader,
						"internal/shadowMapping/pointLight")
					.WithShader(ShaderType.GeometryShader,
						"internal/shadowMapping/pointLight")
					.WithShader(ShaderType.FragmentShader,
						"internal/shadowMapping/pointLight")
			);
			CastShadows = castShadows;
		}
		
		public bool CastShadows
		{
			get => _castShadows;
			set
			{
				_castShadows = value;
				if (value)
				{
					_shadowMapFb = new Framebuffer(new ivec2(SIZE))
						.SetCubemap(FramebufferAttachment.DepthAttachment, new CubemapCreateInfo
						{
							InternalFormat = (InternalFormat) All.DepthComponent24
						}, out Cubemap shadowMap);
					ShadowMap = shadowMap;
				}
				else
				{
					_shadowMapFb?.Dispose();
					ShadowMap?.Dispose();
				}
			}
		}
		
		public void UpdateShadowMap()
		{
			if (!_castShadows) throw new InvalidOperationException("UpdateShadowMap() should not be called on non-shadow-casting lights");
			
			GLStateManager.Push();
			
			GLStateManager.Viewport = new GlViewport(0, 0, SIZE, SIZE);

			vec3 worldPos = Transform.WorldPosition;
			
			ViewProjections[0] = _projection * 
			                     mat4.LookAt(worldPos, worldPos + new vec3(1, 0, 0), new vec3(0, -1, 0));
			ViewProjections[1] = _projection * 
			                     mat4.LookAt(worldPos, worldPos + new vec3(-1, 0, 0), new vec3(0, -1, 0));
			ViewProjections[2] = _projection * 
			                     mat4.LookAt(worldPos, worldPos + new vec3(0, 1, 0), new vec3(0, 0, 1));
			ViewProjections[3] = _projection * 
			                     mat4.LookAt(worldPos, worldPos + new vec3(0, -1, 0), new vec3(0, 0, -1));
			ViewProjections[4] = _projection * 
			                     mat4.LookAt(worldPos, worldPos + new vec3(0, 0, 1), new vec3(0, -1, 0));
			ViewProjections[5] = _projection * 
			                     mat4.LookAt(worldPos, worldPos + new vec3(0, 0, -1), new vec3(0, -1, 0));
			                     
			
			_shadowMapFb.Bind();
			_shadowMapProgram.Bind();
			_shadowMapProgram.SetValue("viewProjections", ViewProjections);
			_shadowMapProgram.SetValue("lightPos", Transform.WorldPosition);
			_shadowMapProgram.SetValue("far", FAR);
			
			_shadowMapFb.ClearDepth();
			
			for (int i = 0; i < Engine.Scene.Objects.Count; i++)
			{
				if (Engine.Scene.Objects[i] is MeshObject meshObject && meshObject.ContributeShadows)
				{
					_shadowMapProgram.SetValue("model", meshObject.Transform.WorldMatrix);
					meshObject.Mesh?.Render();
				}
			}
			
			GLStateManager.Pop();
		}
		
		public NativeLightData NativeLight =>
			new NativeLightData
			{
				Pos = Transform.WorldPosition,
				Color = LinearColor,
				Intensity = Intensity,
				CastShadows = CastShadows ? 1 : 0,
				ShadowMap = CastShadows ? ShadowMap.BindlessHandle : 0,
				Far = CastShadows ? FAR : 0
			};

		[StructLayout(LayoutKind.Sequential)]
		public struct NativeLightData
		{
			public vec3  Pos;
			public float Intensity;
			public vec3  Color;
			public int   CastShadows;
			public long  ShadowMap;
			public float Far;
			public float Padding;
		}
	}
}