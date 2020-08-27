using Cyph3D.GLObject;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.Helper
{
	public static class RenderHelper
	{
		private static VertexArray _quadVAO;
		private static VertexBuffer<float> _quadVBO;

		private static bool _initialized;

		public static void DrawScreenQuad()
		{
			if (!_initialized)
			{
				_quadVAO = new VertexArray();
				
				_quadVBO = new VertexBuffer<float>(false, Stride.Get<float>(4));
				_quadVBO.PutData(new []
				{
					// positions   // texCoords
					-1.0f,  1.0f,  0.0f,  1.0f,
					-1.0f, -1.0f,  0.0f,  0.0f,
					 1.0f, -1.0f,  1.0f,  0.0f,

					-1.0f,  1.0f,  0.0f,  1.0f,
					 1.0f, -1.0f,  1.0f,  0.0f,
					 1.0f,  1.0f,  1.0f,  1.0f
				});

				_quadVAO.RegisterAttrib(_quadVBO, 0, 2, VertexAttribType.Float, 0);
				_quadVAO.RegisterAttrib(_quadVBO, 1, 2, VertexAttribType.Float, 2 * sizeof(float));

				_initialized = true;
			}
			
			_quadVAO.Bind();
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
		}
	}
}