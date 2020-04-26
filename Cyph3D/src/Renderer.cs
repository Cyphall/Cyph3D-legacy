﻿using System;
using System.Runtime.InteropServices;
using GlmSharp;
using OpenGL;
using Renderer.GLObject;
using Renderer.Misc;

namespace Renderer
{
	public class Renderer
	{
		private Framebuffer _gbuffer;
		private Texture _positionTexture;
		private Texture _normalTexture;
		private Texture _colorTexture;
		private Texture _materialTexture;
		private uint _quadVAO;
		private ShaderProgram _lightingPassShader;
		private ShaderStorageBuffer _lightsBuffer;

		public Renderer()
		{
			_gbuffer = new Framebuffer(out _positionTexture, Context.WindowSize, (InternalFormat) 34837); // 34837 = RGB32F
			_normalTexture = _gbuffer.AddTexture(FramebufferAttachment.ColorAttachment1, InternalFormat.Rgb);
			_colorTexture = _gbuffer.AddTexture(FramebufferAttachment.ColorAttachment2, InternalFormat.Rgb16f);
			_materialTexture = _gbuffer.AddTexture(FramebufferAttachment.ColorAttachment3, InternalFormat.Rgba);
			_gbuffer.AddRenderbuffer(FramebufferAttachment.DepthAttachment, InternalFormat.DepthComponent);

			_gbuffer.SetDrawBuffers(
				FramebufferAttachment.ColorAttachment0,
				FramebufferAttachment.ColorAttachment1,
				FramebufferAttachment.ColorAttachment2,
				FramebufferAttachment.ColorAttachment3
			);

			Gl.Enable(EnableCap.DepthTest);

			Gl.ClearColor(0, 0, 0, 1);

			Gl.Enable(EnableCap.CullFace);
			Gl.FrontFace(FrontFaceDirection.Ccw);

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

			_quadVAO = Gl.GenVertexArray();
			uint quadVBO = Gl.GenBuffer();
			Gl.BindVertexArray(_quadVAO);
			Gl.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
			Gl.BufferData(BufferTarget.ArrayBuffer, (uint) quadVertices.Length * sizeof(float), quadVertices, BufferUsage.StaticDraw);
			Gl.EnableVertexAttribArray(0);
			Gl.VertexAttribPointer(0, 2, VertexAttribType.Float, false, 4 * sizeof(float), IntPtr.Zero);
			Gl.EnableVertexAttribArray(1);
			Gl.VertexAttribPointer(1, 2, VertexAttribType.Float, false, 4 * sizeof(float), (IntPtr) (2 * sizeof(float)));


			_lightingPassShader = ShaderProgram.Get("deferred/lightingPass");

			_lightingPassShader.SetValue("positionTexture", 0);
			_lightingPassShader.SetValue("normalTexture", 1);
			_lightingPassShader.SetValue("colorTexture", 2);
			_lightingPassShader.SetValue("materialTexture", 3);
			
			
			_lightsBuffer = new ShaderStorageBuffer(0);
		}

		public void Render(mat4 view, mat4 projection, vec3 viewPos)
		{
			FirstPass(view, projection, viewPos);
			LightingPass(viewPos);
		}

		private void FirstPass(mat4 view, mat4 projection, vec3 viewPos)
		{
			_gbuffer.Bind();

			Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			int objectCount = Context.ObjectContainer.Count;
			for (int i = 0; i < objectCount; i++)
			{
				Context.ObjectContainer[i].Render(view, projection, viewPos);
			}
		}

		private void LightingPass(vec3 viewPos)
		{
			int lightCount = Context.LightContainer.Count;
			NativeArray<PointLight.Light> light = new NativeArray<PointLight.Light>(lightCount);
			for (int i = 0; i < lightCount; i++)
			{
				light[i] = Context.LightContainer[i].GLLight;
			}
			_lightsBuffer.PutData(light);
			light.Dispose();

			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

			Gl.Clear(ClearBufferMask.ColorBufferBit);

			_lightingPassShader.Bind();

			_lightingPassShader.SetValue("viewPos", viewPos);

			_lightingPassShader.SetValue("debug", 0);
			_lightingPassShader.SetValue("numLights", lightCount);

			Gl.ActiveTexture(TextureUnit.Texture0);
			_positionTexture.Bind();
			Gl.ActiveTexture(TextureUnit.Texture1);
			_normalTexture.Bind();
			Gl.ActiveTexture(TextureUnit.Texture2);
			_colorTexture.Bind();
			Gl.ActiveTexture(TextureUnit.Texture3);
			_materialTexture.Bind();

			Gl.BindVertexArray(_quadVAO);

			Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
		}
	}
}