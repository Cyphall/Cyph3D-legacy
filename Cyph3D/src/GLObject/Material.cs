using System.Collections.Generic;
using System.IO;
using System.Json;
using Cyph3D.Enumerable;
using Cyph3D.Misc;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
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
		
		private static Texture _defaultColorMap;
		private static Texture _defaultNormalMap;
		private static Texture _defaultRoughnessMap;
		private static Texture _defaultDisplacementMap;
		private static Texture _defaultMetallicMap;
		private static Texture _defaultEmissiveMap;
		
		private int _remainingTextures;

		public string Name { get; }
		public bool IsLit { get; }
		
		public static Material Default { get; private set; }

		public Material(string name, ResourceManager resourceManager)
		{
			_shaderProgram = resourceManager.RequestShaderProgram("deferred/firstPass");
			
			JsonObject jsonRoot = (JsonObject)JsonValue.Parse(File.ReadAllText($"resources/materials/{name}/material.json"));
			
			if (jsonRoot.ContainsKey("colorMap"))
			{
				resourceManager.RequestImageTexture(
					$"{name}/{(string)jsonRoot["colorMap"]}",
					texture => HandleLoadedTexture(texture, TextureType.Color),
					true,
					true
				);
				_remainingTextures++;
			}
			
			if (jsonRoot.ContainsKey("normalMap"))
			{
				resourceManager.RequestImageTexture(
					$"{name}/{(string)jsonRoot["normalMap"]}",
					texture => HandleLoadedTexture(texture, TextureType.Normal)
				);
				_remainingTextures++;
			}
			
			if (jsonRoot.ContainsKey("roughnessMap"))
			{
				resourceManager.RequestImageTexture(
					$"{name}/{(string)jsonRoot["roughnessMap"]}",
					texture => HandleLoadedTexture(texture, TextureType.Roughness),
					compressed: true
				);
				_remainingTextures++;
			}
			
			if (jsonRoot.ContainsKey("displacementMap"))
			{
				resourceManager.RequestImageTexture(
					$"{name}/{(string)jsonRoot["displacementMap"]}",
					texture => HandleLoadedTexture(texture, TextureType.Displacement),
					compressed: true
				);
				_remainingTextures++;
			}
			
			if (jsonRoot.ContainsKey("metallicMap"))
			{
				resourceManager.RequestImageTexture(
					$"{name}/{(string)jsonRoot["metallicMap"]}",
					texture => HandleLoadedTexture(texture, TextureType.Metallic),
					compressed: true
				);
				_remainingTextures++;
			}
			
			if (jsonRoot.ContainsKey("emissiveMap"))
			{
				resourceManager.RequestImageTexture(
					$"{name}/{(string)jsonRoot["emissiveMap"]}",
					texture => HandleLoadedTexture(texture, TextureType.Emissive),
					compressed: true
				);
				_remainingTextures++;
			}

			Name = name;
			IsLit = jsonRoot["lit"];
		}

		private Material()
		{
			_shaderProgram = Engine.GlobalResourceManager.RequestShaderProgram("deferred/firstPass");
			
			Name = "Default Material";
			IsLit = false;
		}

		private void HandleLoadedTexture(Texture texture, TextureType textureType)
		{
			switch (textureType)
			{
				case TextureType.Color:
					_colorMap = texture;
					break;
				case TextureType.Normal:
					_normalMap = texture;
					break;
				case TextureType.Roughness:
					_roughnessMap = texture;
					break;
				case TextureType.Displacement:
					_displacementMap = texture;
					break;
				case TextureType.Metallic:
					_metallicMap = texture;
					break;
				case TextureType.Emissive:
					_emissiveMap = texture;
					break;
			}

			_remainingTextures--;
			if (_remainingTextures == 0)
			{
				Logger.Info($"Material \"{Name}\" loaded");
			}
		}

		public void Bind(mat4 model, mat4 view, mat4 projection, vec3 cameraPos)
		{
			_shaderProgram.SetValue("colorMap", _colorMap ?? _defaultColorMap);
			_shaderProgram.SetValue("normalMap", _normalMap ?? _defaultNormalMap);
			_shaderProgram.SetValue("roughnessMap", _roughnessMap ?? _defaultRoughnessMap);
			_shaderProgram.SetValue("displacementMap", _displacementMap ?? _defaultDisplacementMap);
			_shaderProgram.SetValue("metallicMap", _metallicMap ?? _defaultMetallicMap);
			_shaderProgram.SetValue("emissiveMap", _emissiveMap ?? _defaultEmissiveMap);
			
			_shaderProgram.SetValue("model", model);
			_shaderProgram.SetValue("view", view);
			_shaderProgram.SetValue("projection", projection);
			_shaderProgram.SetValue("viewPos", cameraPos);
			_shaderProgram.SetValue("isLit", IsLit ? 1 : 0);
			
			
			_shaderProgram.Bind();
		}
		
		public static void InitializeDefault()
		{
			Default = new Material();
			
			_defaultColorMap = new TextureSetting
			{
				Size = new ivec2(1),
				InternalFormat = InternalFormat.CompressedSrgbS3tcDxt1Ext,
				Filtering = TextureFiltering.Nearest
			}.CreateTexture();
			_defaultColorMap.PutData(new byte[]{255, 0, 255});
			
			_defaultNormalMap = new TextureSetting
			{
				Size = new ivec2(1),
				InternalFormat = InternalFormat.Rgb8,
				Filtering = TextureFiltering.Nearest
			}.CreateTexture();
			_defaultNormalMap.PutData(new byte[]{128, 128, 255});
			
			_defaultRoughnessMap = new TextureSetting
			{
				Size = new ivec2(1),
				InternalFormat = InternalFormat.CompressedRgbS3tcDxt1Ext,
				Filtering = TextureFiltering.Nearest
			}.CreateTexture();
			_defaultRoughnessMap.PutData(new byte[]{128}, PixelFormat.Luminance);
			
			_defaultDisplacementMap = new TextureSetting
			{
				Size = new ivec2(1),
				InternalFormat = InternalFormat.CompressedRgbS3tcDxt1Ext,
				Filtering = TextureFiltering.Nearest
			}.CreateTexture();
			_defaultDisplacementMap.PutData(new byte[]{255}, PixelFormat.Luminance);
			
			_defaultMetallicMap = new TextureSetting
			{
				Size = new ivec2(1),
				InternalFormat = InternalFormat.CompressedRgbS3tcDxt1Ext,
				Filtering = TextureFiltering.Nearest
			}.CreateTexture();
			_defaultMetallicMap.PutData(new byte[]{0, 0, 0});
			
			_defaultEmissiveMap = new TextureSetting
			{
				Size = new ivec2(1),
				InternalFormat = InternalFormat.CompressedRgbS3tcDxt1Ext,
				Filtering = TextureFiltering.Nearest
			}.CreateTexture();
			_defaultEmissiveMap.PutData(new byte[]{0, 0, 0});
		}

		private enum TextureType
		{
			Color,
			Normal,
			Roughness,
			Displacement,
			Metallic,
			Emissive
		}
	}
}