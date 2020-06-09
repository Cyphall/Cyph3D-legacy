using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Cyph3D.Enumerable;
using Cyph3D.GLObject;
using GlmSharp;
using ImGuiNET;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.UI.Impl
{
	public static unsafe class ImplOpenGL
	{
		private static Texture	     _fontTexture;
		private static ShaderProgram _shaderProgram;
		private static int           _VAO;
		private static int           _VBO;
		private static int           _EBO;

		private delegate void Del(ImDrawList a, ImDrawCmd b);

		public static void Init()
		{
			ImGuiIOPtr io = ImGui.GetIO();
			fixed(byte* p = &Encoding.ASCII.GetBytes("imgui_impl_opengl3")[0])
			{
				io.NativePtr->BackendRendererName = p;
			}

			io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
		}
		
		public static void Shutdown()
		{
			if (_VBO != 0)
			{
				GL.DeleteBuffer(_VBO);
				_VBO = 0;
			}

			if (_EBO != 0)
			{
				GL.DeleteBuffer(_EBO);
				_EBO = 0;
			}
			
			ImGuiIOPtr io = ImGui.GetIO();
			io.Fonts.SetTexID(IntPtr.Zero);
			_fontTexture.Dispose();
		}

		public static void NewFrame()
		{
			if (_shaderProgram == null)
				CreateDeviceObjects();
		}

		public static void CreateDeviceObjects()
		{
			ImGuiIOPtr io = ImGui.GetIO();

			_shaderProgram = ShaderProgram.Get("deferred/imgui");

			_VBO = GL.GenBuffer();
			_EBO = GL.GenBuffer();
			
			_VAO = GL.GenVertexArray();
			
			GL.BindVertexArray(_VAO);
			
				GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, _EBO);
				
				GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float,       false, sizeof(ImDrawVert), Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.pos)));
				GL.EnableVertexAttribArray(0);

				GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float,       false, sizeof(ImDrawVert), Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.uv)));
				GL.EnableVertexAttribArray(1);
				
				GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(ImDrawVert), Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.col)));
				GL.EnableVertexAttribArray(2);
			
			GL.BindVertexArray(0);
			
			io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height);
			
			_fontTexture = new Texture(new ivec2(width, height), InternalFormat.Rgba8, TextureFiltering.Linear);
			_fontTexture.PutData((IntPtr)pixels, PixelFormat.Rgba);
			
			io.Fonts.SetTexID((IntPtr)(int)_fontTexture);
		}

		public static void SetupRenderState(ImDrawData drawData, ivec2 fbSize)
		{
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.DepthTest);
			GL.Enable(EnableCap.ScissorTest);
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			
			GL.Viewport(0, 0, fbSize.x, fbSize.y);
			float l = drawData.DisplayPos.X;
			float r = drawData.DisplayPos.X + drawData.DisplaySize.X;
			float t = drawData.DisplayPos.Y;
			float b = drawData.DisplayPos.Y + drawData.DisplaySize.Y;
			
			mat4 orthoProjection = new mat4
			(
				2.0f/(r-l),   0.0f,		 0.0f,   0.0f,
				0.0f,		 2.0f/(t-b),   0.0f,   0.0f,
				0.0f,		 0.0f,		-1.0f,   0.0f,
				(r+l)/(l-r),  (t+b)/(b-t),  0.0f,   1.0f
			);
			
			_shaderProgram.SetValue("Texture", 0);
			_shaderProgram.SetValue("ProjMtx", orthoProjection);
			
			_shaderProgram.Bind();
			
			GL.BindSampler(0, 0);
		}
		
		public static void RenderDrawData(ImDrawData drawData)
		{
			ivec2 fbSize = new ivec2()
			{
				x = (int) (drawData.DisplaySize.X * drawData.FramebufferScale.X),
				y = (int) (drawData.DisplaySize.Y * drawData.FramebufferScale.Y)
			};
			if (fbSize.x <= 0 || fbSize.y <= 0)
				return;
			
			
			// Backup GL state
			GL.GetInteger(GetPName.ActiveTexture, out int lastActiveTexture);
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.GetInteger(GetPName.CurrentProgram, out int lastProgram);
			GL.GetInteger(GetPName.TextureBinding2D, out int lastTexture);
			GL.GetInteger(GetPName.SamplerBinding, out int lastSampler);
			GL.GetInteger(GetPName.ArrayBufferBinding, out int lastArrayBuffer);
			GL.GetInteger(GetPName.VertexArrayBinding, out int lastVertexArrayObject);
			GL.GetInteger(GetPName.PolygonMode, out int lastPolygonMode);
			int[] lastViewport = new int[4]; GL.GetInteger(GetPName.Viewport, lastViewport);
			int[] lastScissorBox = new int[4]; GL.GetInteger(GetPName.ScissorBox, lastScissorBox);
			GL.GetInteger(GetPName.BlendSrcRgb, out int lastBlendSrcRGB);
			GL.GetInteger(GetPName.BlendDstRgb, out int lastBlendDstRGB);
			GL.GetInteger(GetPName.BlendSrcAlpha, out int lastBlendSrcAlpha);
			GL.GetInteger(GetPName.BlendDstAlpha, out int lastBlendDstAlpha);
			GL.GetInteger(GetPName.BlendEquationRgb, out int lastBlendEquationRGB);
			GL.GetInteger(GetPName.BlendEquationAlpha, out int lastBlendEquationAlpha);
			bool lastEnableBlend = GL.IsEnabled(EnableCap.Blend);
			bool lastEnableCullFace = GL.IsEnabled(EnableCap.CullFace);
			bool lastEnableDepthTest = GL.IsEnabled(EnableCap.DepthTest);
			bool lastEnableScissorTest = GL.IsEnabled(EnableCap.ScissorTest);
			
			SetupRenderState(drawData, fbSize);
			
			// Will project scissor/clipping rectangles into framebuffer space
			Vector2 clipOff = drawData.DisplayPos;		 // (0,0) unless using multi-viewports
			Vector2 clipScale = drawData.FramebufferScale; // (1,1) unless using retina display which are often (2,2)

			// Render command lists
			for (int n = 0; n < drawData.CmdListsCount; n++)
			{
				ImDrawList cmdList = *drawData.CmdLists[n];

				// Upload vertex/index buffers
				GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
					GL.BufferData(BufferTarget.ArrayBuffer, cmdList.VtxBuffer.Size * sizeof(ImDrawVert), cmdList.VtxBuffer.Data, BufferUsageHint.StreamDraw);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, _EBO);
					GL.BufferData(BufferTarget.ElementArrayBuffer, cmdList.IdxBuffer.Size * sizeof(ushort), cmdList.IdxBuffer.Data, BufferUsageHint.StreamDraw);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

				for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
				{
					ImDrawCmd pcmd = cmdList.CmdBuffer.Ref<ImDrawCmd>(i);
					if (pcmd.UserCallback != IntPtr.Zero)
					{
						// User callback, registered via ImDrawList::AddCallback()
						// (ImDrawCallback_ResetRenderState is a special callback value used by the user to request the renderer to reset render state.)
						Marshal.GetDelegateForFunctionPointer<Del>(pcmd.UserCallback)(cmdList, pcmd);
					}
					else
					{
						// Project scissor/clipping rectangles into framebuffer space
						vec4 clipRect = vec4.Zero;
						clipRect.x = (pcmd.ClipRect.X - clipOff.X) * clipScale.X;
						clipRect.y = (pcmd.ClipRect.Y - clipOff.Y) * clipScale.Y;
						clipRect.z = (pcmd.ClipRect.Z - clipOff.X) * clipScale.X;
						clipRect.w = (pcmd.ClipRect.W - clipOff.Y) * clipScale.Y;

						if (clipRect.x < fbSize.x && clipRect.y < fbSize.y && clipRect.z >= 0.0f && clipRect.w >= 0.0f)
						{
							// Apply scissor/clipping rectangle
							GL.Scissor((int)clipRect.x, (int)(fbSize.y - clipRect.w), (int)(clipRect.z - clipRect.x), (int)(clipRect.w - clipRect.y));

							// Bind texture, Draw
							GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);

							GL.BindVertexArray(_VAO);
							GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), (int)pcmd.VtxOffset);
							GL.BindVertexArray(0);
						}
					}
				}
			}

			// Restore modified GL state
			GL.UseProgram(lastProgram);
			GL.BindTexture(TextureTarget.Texture2D, lastTexture);
			GL.BindSampler(0, lastSampler);
			GL.ActiveTexture((TextureUnit)lastActiveTexture);
			GL.BindVertexArray(lastVertexArrayObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, lastArrayBuffer);
			GL.BlendEquationSeparate((BlendEquationMode)lastBlendEquationRGB, (BlendEquationMode)lastBlendEquationAlpha);
			GL.BlendFuncSeparate((BlendingFactorSrc)lastBlendSrcRGB, (BlendingFactorDest)lastBlendDstRGB, (BlendingFactorSrc)lastBlendSrcAlpha, (BlendingFactorDest)lastBlendDstAlpha);
			if (lastEnableBlend) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
			if (lastEnableCullFace) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
			if (lastEnableDepthTest) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
			if (lastEnableScissorTest) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
			GL.PolygonMode(MaterialFace.FrontAndBack, (PolygonMode)lastPolygonMode);
			GL.Viewport(lastViewport[0], lastViewport[1], lastViewport[2], lastViewport[3]);
			GL.Scissor(lastScissorBox[0], lastScissorBox[1], lastScissorBox[2], lastScissorBox[3]);
		}
	}
}