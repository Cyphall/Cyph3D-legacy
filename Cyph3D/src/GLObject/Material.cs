using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using Cyph3D.Enumerable;
using Cyph3D.Helper;
using Cyph3D.Misc;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class Material : IDisposable
	{
		private MaterialShaderProgram _shaderProgram;

		private Dictionary<string, (Texture, Image)> _textures = new Dictionary<string, (Texture, Image)>();
		
		private bool _loaded;

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

				TextureCreateInfo createInfo = new TextureCreateInfo
				{
					Size = new ivec2(1),
					InternalFormat = internalFormat,
					Filtering = TextureFiltering.Nearest
				};
				
				Texture defaultColor = new Texture(createInfo);
				defaultColor.PutData(mapDefinition.DefaultData, pixelFormat);

				Image image = null;
				
				if (jsonRoot.ContainsKey(mapName))
				{
					image = Engine.Scene.ResourceManager.RequestImage(
						$"{name}/{(string)jsonRoot[mapName]}",
						mapDefinition.sRGB,
						mapDefinition.Compressed
					);
				}
				
				_textures[mapName] = (defaultColor, image);
			}

			Name = name;
		}

		private Material()
		{
			_shaderProgram = new MaterialShaderProgram("unlit", Engine.GlobalResourceManager);
			
			(InternalFormat internalFormat, PixelFormat pixelFormat) = TextureHelper.GetTextureSetting(3, true, true);

			TextureCreateInfo createInfo = new TextureCreateInfo
			{
				Size = new ivec2(1),
				InternalFormat = internalFormat,
				Filtering = TextureFiltering.Nearest
			};
			
			Texture defaultColor = new Texture(createInfo);
			defaultColor.PutData(new byte[]{255, 0, 255}, pixelFormat);

			_textures.Add("colorMap", (defaultColor, null));
			
			Name = "Default Material";
		}

		public void Bind(mat4 model, mat4 view, mat4 projection, vec3 cameraPos)
		{
			bool allImagesAreReady = true;
			foreach (string name in _textures.Keys.ToArray())
			{
				(Texture texture, Image image) = _textures[name];
				
				if (texture != null && image != null && image.IsResourceReady)
				{
					texture.Dispose();
					_textures[name] = (null, image);
				}
				else if (image != null && !image.IsResourceReady)
				{
					allImagesAreReady = false;
				}

				if (image != null && image.IsResourceReady)
				{
					_shaderProgram.ShaderProgram.SetValue(name, image.ResourceData);
				}
				else
				{
					_shaderProgram.ShaderProgram.SetValue(name, texture);
				}
			}

			if (!_loaded && allImagesAreReady)
			{
				Logger.Info($"Material \"{Name}\" loaded");
				_loaded = true;
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

		public void Dispose()
		{
			foreach ((Texture texture, Image image) in _textures.Values)
			{
				texture?.Dispose();
				image?.Dispose();
			}
		}
	}
}