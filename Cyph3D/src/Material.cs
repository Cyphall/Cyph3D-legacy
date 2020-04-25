using System;
using System.Collections.Generic;
using GlmSharp;
using OpenGL;
using Renderer.GLObject;
using Renderer.Misc;

namespace Renderer
{
	public class Material
	{
		private ShaderProgram _shaderProgram;
		private Texture _colorMap;
		private Texture _normalMap;
		private Texture _roughnessMap;
		private Texture _displacementMap;
		private Texture _metallicMap;

		private bool _isLit;
		
		public Material(
			bool isLit,
			Texture colorMap = null,
			Texture normalMap = null,
			Texture roughnessMap = null,
			Texture displacementMap = null,
			Texture metallicMap = null
		)
		{
			_shaderProgram = ShaderProgram.Get("deferred/firstPass");
			_shaderProgram.Bind();
			
			_shaderProgram.SetValue("colorMap", 0);
			_shaderProgram.SetValue("normalMap", 1);
			_shaderProgram.SetValue("roughnessMap", 2);
			_shaderProgram.SetValue("displacementMap", 3);
			_shaderProgram.SetValue("metallicMap", 4);

			if (colorMap == null)
			{
				colorMap = new Texture(new ivec2(1), InternalFormat.Srgb);
				colorMap.PutData(new byte[]{255, 0, 255});
			}
			if (normalMap == null)
			{
				normalMap = new Texture(new ivec2(1), InternalFormat.Rgb);
				normalMap.PutData(new byte[]{128, 128, 255});
			}
			if (roughnessMap == null)
			{
				roughnessMap = new Texture(new ivec2(1), InternalFormat.Rgb);
				roughnessMap.PutData(new byte[]{128, 128, 128});
			}
			if (displacementMap == null)
			{
				displacementMap = new Texture(new ivec2(1), InternalFormat.Rgb);
				displacementMap.PutData(new byte[]{255, 255, 255});
			}
			if (metallicMap == null)
			{
				metallicMap = new Texture(new ivec2(1), InternalFormat.Rgb);
				metallicMap.PutData(new byte[]{0, 0, 0});
			}

			_isLit = isLit;
			
			_colorMap = colorMap;
			_normalMap = normalMap;
			_roughnessMap = roughnessMap;
			_displacementMap = displacementMap;
			_metallicMap = metallicMap;
		}

		public void Bind(mat4 model, mat4 view, mat4 projection, vec3 cameraPos)
		{
			_shaderProgram.Bind();

			Gl.ActiveTexture(TextureUnit.Texture0);
			_colorMap.Bind();
			Gl.ActiveTexture(TextureUnit.Texture1);
			_normalMap.Bind();
			Gl.ActiveTexture(TextureUnit.Texture2);
			_roughnessMap.Bind();
			Gl.ActiveTexture(TextureUnit.Texture3);
			_displacementMap.Bind();
			Gl.ActiveTexture(TextureUnit.Texture4);
			_metallicMap.Bind();
			
			_shaderProgram.SetValue("model", model);
			_shaderProgram.SetValue("view", view);
			_shaderProgram.SetValue("projection", projection);
			
			_shaderProgram.SetValue("viewPos", cameraPos);

			_shaderProgram.SetValue("isLit", _isLit ? 1 : 0);
		}
		
		private static Dictionary<string, Material> _materials = new Dictionary<string, Material>();
		
		public static Material Get(string name, Func<Material> creationMethod)
		{
			if (!_materials.ContainsKey(name))
			{
				Logger.Info($"Loading material \"{name}\"");
				_materials.Add(name, creationMethod.Invoke());
				Logger.Info($"Material \"{name}\" loaded");
			}

			return _materials[name];
		}
	}
}