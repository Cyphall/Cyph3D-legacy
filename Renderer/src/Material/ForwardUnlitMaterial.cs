using System;
using System.Collections.Generic;
using GlmSharp;
using OpenGL;

namespace Renderer
{
	public class ForwardUnlitMaterial : Material
	{
		private ShaderProgram _shaderProgram;
		private Texture _colorMap;
		
		public ForwardUnlitMaterial(
			Texture colorMap
		)
		{
			_shaderProgram = ShaderProgram.Get("forward/unlit");
			_shaderProgram.Bind();
			
			_shaderProgram.SetValue("colorMap", 0);

			if (colorMap == null)
			{
				colorMap = new Texture(new ivec2(1), InternalFormat.SrgbAlpha, PixelFormat.Rgba);
				colorMap.PutData(new byte[]{255, 0, 255, 255});
			}
			
			_colorMap = colorMap;
		}

		public override void Bind(mat4 model, mat4 view, mat4 projection, vec3 cameraPos, vec3 lightPos)
		{
			_shaderProgram.Bind();

			Gl.ActiveTexture(TextureUnit.Texture0);
			_colorMap.Bind();
			
			_shaderProgram.SetValue("model", model);
			_shaderProgram.SetValue("view", view);
			_shaderProgram.SetValue("projection", projection);
			_shaderProgram.SetValue("lightPos", lightPos);
			_shaderProgram.SetValue("viewPos", cameraPos);
		}
	}
}