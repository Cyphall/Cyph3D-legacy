using System;
using System.Runtime.InteropServices;
using Cyph3D.GLObject;
using Cyph3D.Misc;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.Lighting
{
	public class DirectionalLight : Light
	{
		public Texture ShadowMap { get; private set; }
		private Framebuffer _shadowMapFb;
		private static ShaderProgram _shadowMapProgram;

		private bool _castShadows;

		public mat4 ViewProjection { get; private set; }

		private const int SIZE = 4096;
		
		public DirectionalLight(Transform parent, vec3 srgbColor, float intensity, string name = "DirectionalLight", vec3? position = null, vec3? rotation = null, bool castShadows = false):
			base(parent, srgbColor, intensity, name, position, rotation)
		{
			_shadowMapProgram ??= Engine.GlobalResourceManager.RequestShaderProgram("internal/shadowMapping/directionalLight");
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
						.WithTexture(FramebufferAttachment.DepthAttachment, new TextureSetting
						{
							InternalFormat = (InternalFormat) All.DepthComponent24,
							IsShadowMap = true
						}, out Texture shadowMap)
						.Complete();
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
			
			GL.Viewport(0, 0, SIZE, SIZE);
			
			ViewProjection = mat4.Ortho(-30, 30, -30, 30, 0, 100) *
			                 mat4.LookAt(
				                 Engine.Scene.Camera.Position - LightDirection * 50, 
				                 Engine.Scene.Camera.Position, 
				                 new vec3(0, 1, 0));
			
			_shadowMapFb.Bind();
			_shadowMapProgram.Bind();
			_shadowMapProgram.SetValue("viewProjection", ViewProjection);
			
			GL.Clear(ClearBufferMask.DepthBufferBit);
			
			for (int i = 0; i < Engine.Scene.Objects.Count; i++)
			{
				if (Engine.Scene.Objects[i] is MeshObject meshObject)
				{
					_shadowMapProgram.SetValue("model", meshObject.Transform.WorldMatrix);
					meshObject.Mesh?.Render();
				}
			}
		}

		private vec3 LightDirection => (new mat4(new mat3(Transform.WorldMatrix)) * new vec4(0, -1, 0, 1)).xyz;
		
		public NativeLightData NativeLight =>
			new NativeLightData
			{
				FragToLightDirection = -LightDirection,
				Color = LinearColor,
				Intensity = Intensity,
				CastShadows = CastShadows ? 1 : 0,
				LightViewProjection = CastShadows ? ViewProjection : mat4.Identity,
				ShadowMap = CastShadows ? ShadowMap.BindlessHandle : 0
			};

		[StructLayout(LayoutKind.Sequential)]
		public struct NativeLightData
		{
			public vec3  FragToLightDirection;
			public float Intensity;
			public vec3  Color;
			public int   CastShadows;
			public mat4  LightViewProjection;
			public long  ShadowMap;
			public vec2  Padding;
		}
	}
}