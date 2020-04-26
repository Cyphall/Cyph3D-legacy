using System.Collections.Generic;
using GlmSharp;
using OpenGL;
using Renderer.GLObject;
using Renderer.Misc;

namespace Renderer.GLObject
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
		
		private static Dictionary<string, Material> _materials = new Dictionary<string, Material>();
		
		public Material(string name, bool isLit)
		{
			_shaderProgram = ShaderProgram.Get("deferred/firstPass");
			_shaderProgram.Bind();
			
			_shaderProgram.SetValue("colorMap", 0);
			_shaderProgram.SetValue("normalMap", 1);
			_shaderProgram.SetValue("roughnessMap", 2);
			_shaderProgram.SetValue("displacementMap", 3);
			_shaderProgram.SetValue("metallicMap", 4);

			_colorMap = Texture.FromFile($"{name}/col", true);
			if (_colorMap == null)
			{
				_colorMap = new Texture(new ivec2(1), InternalFormat.Srgb8);
				_colorMap.PutData(new byte[]{255, 0, 255});
			}
			
			_normalMap = Texture.FromFile($"{name}/nrm");
			if (_normalMap == null)
			{
				_normalMap = new Texture(new ivec2(1), InternalFormat.Rgb);
				_normalMap.PutData(new byte[]{128, 128, 255});
			}
			
			_roughnessMap = Texture.FromFile($"{name}/rgh");
			if (_roughnessMap == null)
			{
				_roughnessMap = new Texture(new ivec2(1), InternalFormat.Rgb);
				_roughnessMap.PutData(new byte[]{128}, PixelFormat.Luminance);
			}
			
			_displacementMap = Texture.FromFile($"{name}/disp");
			if (_displacementMap == null)
			{
				_displacementMap = new Texture(new ivec2(1), InternalFormat.Rgb);
				_displacementMap.PutData(new byte[]{255}, PixelFormat.Luminance);
			}
			
			_metallicMap = Texture.FromFile($"{name}/met");
			if (_metallicMap == null)
			{
				_metallicMap = new Texture(new ivec2(1), InternalFormat.Rgb);
				_metallicMap.PutData(new byte[]{0, 0, 0});
			}

			_isLit = isLit;
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
		
		public static Material GetOrLoad(string name, bool isLit)
		{
			if (!_materials.ContainsKey(name))
			{
				Logger.Info($"Loading material \"{name}\"");
				_materials.Add(name, new Material(name, isLit));
				Logger.Info($"Material \"{name}\" loaded");
			}

			return _materials[name];
		}
	}
}