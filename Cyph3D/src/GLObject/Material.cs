using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using Cyph3D.Enumerable;
using Cyph3D.Helper;
using Cyph3D.Misc;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class Material
	{
		private MaterialShaderProgram _shaderProgram;

		private Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
		
		private int _remainingTextures;

		public string Name { get; }
		
		public static Material Default { get; private set; }

		public Material(string name)
		{
			JsonObject jsonRoot = (JsonObject)JsonValue.Parse(File.ReadAllText($"resources/materials/{name}/material.json"));

			if (!jsonRoot.ContainsKey("shader"))
			{
				throw new NotSupportedException($"material.json of material {name} doesn't contain a \"shader\" entry.");
			}
			
			_shaderProgram = new MaterialShaderProgram(jsonRoot["shader"]);

			foreach ((string mapName, MapDefinition mapDefinition) in _shaderProgram.MapDefinitions)
			{
				(InternalFormat internalFormat, PixelFormat pixelFormat) = TextureHelper.GetTextureSetting(mapDefinition.DefaultData.Length, mapDefinition.Compressed, mapDefinition.sRGB);

				Texture defaultTexture = new TextureSetting
				{
					Size = new ivec2(1),
					InternalFormat = internalFormat,
					Filtering = TextureFiltering.Nearest
				}.CreateTexture();
				defaultTexture.PutData(mapDefinition.DefaultData, pixelFormat);

				_textures[mapName] = defaultTexture;
				
				if (jsonRoot.ContainsKey(mapName))
				{
					Engine.Scene.ResourceManager.RequestImageTexture(
						$"{name}/{(string)jsonRoot[mapName]}",
						texture => HandleLoadedTexture(mapName, texture),
						mapDefinition.sRGB,
						mapDefinition.Compressed
					);
					_remainingTextures++;
				}
			}

			Name = name;
		}

		private Material()
		{
			_shaderProgram = new MaterialShaderProgram("unlit", Engine.GlobalResourceManager);
			
			(InternalFormat internalFormat, PixelFormat pixelFormat) = TextureHelper.GetTextureSetting(3, true, true);

			Texture defaultColor = new TextureSetting
			{
				Size = new ivec2(1),
				InternalFormat = internalFormat,
				Filtering = TextureFiltering.Nearest
			}.CreateTexture();
			defaultColor.PutData(new byte[]{255, 0, 255}, pixelFormat);

			_textures.Add("colorMap", defaultColor);
			
			Name = "Default Material";
		}

		private void HandleLoadedTexture(string name, Texture texture)
		{
			_textures[name].Dispose();
			_textures[name] = texture;

			_remainingTextures--;
			if (_remainingTextures == 0)
			{
				Logger.Info($"Material \"{Name}\" loaded");
			}
		}

		public void Bind(mat4 model, mat4 view, mat4 projection, vec3 cameraPos)
		{
			foreach ((string name, Texture texture) in _textures)
			{
				_shaderProgram.ShaderProgram.SetValue(name, texture);
			}
			
			_shaderProgram.ShaderProgram.SetValue("normalMatrix", new mat3(model).Inverse.Transposed);
			_shaderProgram.ShaderProgram.SetValue("model", model);
			_shaderProgram.ShaderProgram.SetValue("view", view);
			_shaderProgram.ShaderProgram.SetValue("projection", projection);
			_shaderProgram.ShaderProgram.SetValue("viewPos", cameraPos);
			
			
			_shaderProgram.ShaderProgram.Bind();
		}
		
		public static void InitializeDefault()
		{
			Default = new Material();
		}
	}
}