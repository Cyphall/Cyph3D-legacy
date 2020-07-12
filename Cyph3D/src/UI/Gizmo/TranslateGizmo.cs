using Cyph3D.GLObject;
using Cyph3D.UI.Window;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.UI.Gizmo
{
	public static class TranslateGizmo
	{
		private static Framebuffer _framebuffer;

		private static Texture _texture;
		
		private static Mesh _arrow;
		private static Mesh _base;

		private static ShaderProgram _program;

		public static void Init()
		{
			_framebuffer = new Framebuffer(Engine.Window.Size)
				.WithTexture(FramebufferAttachment.ColorAttachment0, InternalFormat.Rgba8, out _texture)
				.WithRenderbuffer(FramebufferAttachment.DepthAttachment, RenderbufferStorage.DepthComponent16)
				.Complete();
			
			Engine.GlobalResourceManager.RequestMesh("Gizmo/TranslateArrow", mesh => _arrow = mesh);
			Engine.GlobalResourceManager.RequestMesh("Gizmo/TranslateBase", mesh => _base = mesh);

			_program = Engine.GlobalResourceManager.RequestShaderProgram("gizmo/solidColor");
			_program.SetValue("Texture", 0);
		}

		public static void Update()
		{
			if (UIInspector.Selected == null || !(UIInspector.Selected is SceneObject)) return;
			int previousDepthTest = GL.GetInteger(GetPName.DepthTest);
			
			_framebuffer.Bind();
			
			GL.Enable(EnableCap.DepthTest);
			
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			SceneObject obj = (SceneObject) UIInspector.Selected;

			vec3 fixedPosition = (Engine.Scene.Camera.Position - obj.Transform.WorldPosition).NormalizedSafe * 15;
			mat4 fixedDistanceView = mat4.LookAt(fixedPosition,  fixedPosition + Engine.Scene.Camera.Orientation, new vec3(0, 1, 0));
			mat4 model;
			
			_program.Bind();
			
			_program.SetValue("viewDir", Engine.Scene.Camera.Position.Normalized);

			model = obj.Transform.WorldMatrix;
			_program.SetValue("mvp", Engine.Scene.Camera.Projection * fixedDistanceView * model);
			_program.SetValue("model", obj.Transform.WorldMatrix);
			_program.SetValue("color", new vec3(1, 1, 1));
			_base.Render();
			
			_program.SetValue("color", new vec3(0, 1, 0));
			_arrow.Render();
			
			model = obj.Transform.WorldMatrix * mat4.RotateZ(glm.Radians(-90f));
			_program.SetValue("mvp", Engine.Scene.Camera.Projection * fixedDistanceView * model);
			_program.SetValue("model", model);
			_program.SetValue("color", new vec3(1, 0, 0));
			_arrow.Render();
			
			model = obj.Transform.WorldMatrix * mat4.RotateX(glm.Radians(90f));
			_program.SetValue("mvp", Engine.Scene.Camera.Projection * fixedDistanceView * model);
			_program.SetValue("model", model);
			_program.SetValue("color", new vec3(0, 0, 1));
			_arrow.Render();

			if (previousDepthTest == 1)
				GL.Enable(EnableCap.DepthTest);
			else
				GL.Disable(EnableCap.DepthTest);
			Framebuffer.DrawToDefault(_texture);
		}
	}
}