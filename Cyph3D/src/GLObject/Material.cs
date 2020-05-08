using System.Collections.Generic;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
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
		private Texture _emissiveMap;

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
			_shaderProgram.SetValue("emissiveMap", 5);
			
			_shaderProgram.Unbind();

			_colorMap = Texture.FromFile($"{name}/col", true, true);
			if (_colorMap == null)
			{
				_colorMap = new Texture(new ivec2(1), InternalFormat.CompressedSrgbS3tcDxt1Ext);
				_colorMap.PutData(new byte[]{255, 0, 255});
			}
			
			_normalMap = Texture.FromFile($"{name}/nrm");
			if (_normalMap == null)
			{
				_normalMap = new Texture(new ivec2(1), InternalFormat.Rgb8);
				_normalMap.PutData(new byte[]{128, 128, 255});
			}
			
			_roughnessMap = Texture.FromFile($"{name}/rgh", compressed: true);
			if (_roughnessMap == null)
			{
				_roughnessMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
				_roughnessMap.PutData(new byte[]{128}, PixelFormat.Luminance);
			}
			
			_displacementMap = Texture.FromFile($"{name}/disp", compressed: true);
			if (_displacementMap == null)
			{
				_displacementMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
				_displacementMap.PutData(new byte[]{255}, PixelFormat.Luminance);
			}
			
			_metallicMap = Texture.FromFile($"{name}/met", compressed: true);
			if (_metallicMap == null)
			{
				_metallicMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
				_metallicMap.PutData(new byte[]{0, 0, 0});
			}
			
			_emissiveMap = Texture.FromFile($"{name}/emis", compressed: true);
			if (_emissiveMap == null)
			{
				_emissiveMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
				_emissiveMap.PutData(new byte[]{0, 0, 0});
			}

			_isLit = isLit;
			_materials.Add(name, this);
		}

		public void Bind(mat4 model, mat4 view, mat4 projection, vec3 cameraPos)
		{
			_shaderProgram.Bind();

			GL.ActiveTexture(TextureUnit.Texture0);
			_colorMap.Bind();
			GL.ActiveTexture(TextureUnit.Texture1);
			_normalMap.Bind();
			GL.ActiveTexture(TextureUnit.Texture2);
			_roughnessMap.Bind();
			GL.ActiveTexture(TextureUnit.Texture3);
			_displacementMap.Bind();
			GL.ActiveTexture(TextureUnit.Texture4);
			_metallicMap.Bind();
			GL.ActiveTexture(TextureUnit.Texture5);
			_emissiveMap.Bind();
			
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
				// ReSharper disable once ObjectCreationAsStatement
				new Material(name, isLit);
				Logger.Info($"Material \"{name}\" loaded");
			}

			return _materials[name];
		}
	}
}