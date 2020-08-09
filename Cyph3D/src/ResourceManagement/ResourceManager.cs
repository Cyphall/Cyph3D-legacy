using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using Cyph3D.Enumerable;
using Cyph3D.Extension;
using Cyph3D.GLObject;
using Cyph3D.Misc;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using StbImageNET;

namespace Cyph3D.ResourceManagement
{
	public class ResourceManager : IDisposable
	{
		public void Update()
		{
			TextureUpdate();
			SkyboxUpdate();
			MeshUpdate();
		}
		
		public void Dispose()
		{
			foreach (ResourceHandler<Texture> handler in _textures.Values)
			{
				handler.Dispose();
			}
			foreach (ResourceHandler<Skybox> handler in _skyboxes.Values)
			{
				handler.Dispose();
			}
			foreach (ShaderProgram shaderProgram in _shaderPrograms.Values)
			{
				shaderProgram.Dispose();
			}
			foreach (ResourceHandler<Mesh> handler in _meshes.Values)
			{
				handler.Dispose();
			}
		}
		
		#region Textures
		
		private Dictionary<string, ResourceHandler<Texture>> _textures = new Dictionary<string, ResourceHandler<Texture>>();
		private ConcurrentQueue<Tuple<string, Texture>> _loadedTextures = new ConcurrentQueue<Tuple<string, Texture>>();

		public void RequestImageTexture(string name, ResourceHandler<Texture>.ResourceCallback callback, bool sRGB = false, bool compressed = false)
		{
			string path = $"resources/materials/{name}";
			
			if (!_textures.ContainsKey(path))
			{
				_textures.Add(path, new ResourceHandler<Texture>());
				LoadImageTexture(path, sRGB, compressed);
			}
			
			_textures[path].AddCallback(callback);
		}

		private void LoadImageTexture(string path, bool sRGB, bool compressed)
		{
			Logger.Info($"Loading image texture \"{path}\"");

			int x;
			int y;
			Components comp;
			
			try
			{
				StbImage.Info(path, out x, out y, out comp);
			}
			catch (IOException)
			{
				throw new IOException($"Unable to load image {path} from disk");
			}

			(InternalFormat internalFormat, PixelFormat pixelFormat) = TextureHelper.GetTextureSetting((int) comp, compressed, sRGB);
			
			Texture texture = new TextureSetting
			{
				Size = new ivec2(x, y),
				InternalFormat = internalFormat,
				Filtering = TextureFiltering.Linear,
				UseMipmaps = true
			}.CreateTexture();
			
			GL.Finish();
			
			Engine.ThreadPool.Schedule(() => {
				byte[] image;
				
				try
				{
					image = StbImage.Load(path, out int _, out int _, out Components _, comp);
				}
				catch (IOException)
				{
					throw new IOException($"Unable to load image {path} from disk");
				}
				
				texture.PutData(image, pixelFormat);
				
				GL.Finish();
				
				Logger.Info($"Texture \"{path}\" loaded (id: {(int)texture})");
				
				_loadedTextures.Enqueue(new Tuple<string, Texture>(path, texture));
			});
		}

		private void TextureUpdate()
		{
			while (_loadedTextures.TryDequeue(out Tuple<string, Texture> data))
			{
				_textures[data.Item1].ValidateLoading(data.Item2);
			}
		}
		
		#endregion

		#region Skybox

		private Dictionary<string, ResourceHandler<Skybox>> _skyboxes = new Dictionary<string, ResourceHandler<Skybox>>();
		private ConcurrentQueue<Tuple<string, Skybox>> _loadedSkyboxes = new ConcurrentQueue<Tuple<string, Skybox>>();

		public void RequestSkybox(string name, ResourceHandler<Skybox>.ResourceCallback callback)
		{
			string path = $"resources/skyboxes/{name}";
			
			if (!_skyboxes.ContainsKey(path))
			{
				_skyboxes.Add(path, new ResourceHandler<Skybox>());
				LoadSkybox(path, name);
			}
			
			_skyboxes[path].AddCallback(callback);
		}

		private void LoadSkybox(string path, string name)
		{
			string[] jsonDataNames =
			{
				"right",
				"left",
				"down",
				"up",
				"front",
				"back"
			};
			
			Logger.Info($"Loading skybox \"{path}\"");
			
			JsonObject jsonRoot = (JsonObject)JsonValue.Parse(File.ReadAllText($"{path}/skybox.json"));

			int x;
			int y;
			Components comp;
			
			try
			{
				StbImage.Info($"{path}/{(string)jsonRoot["front"]}", out x, out y, out comp);
			}
			catch (IOException)
			{
				throw new IOException($"Unable to load skybox {path}/{(string)jsonRoot["front"]} from disk");
			}

			InternalFormat internalFormat;
			PixelFormat pixelFormat;
			switch (comp)
			{
				case Components.Grey:
					pixelFormat = PixelFormat.Luminance;
					internalFormat = InternalFormat.CompressedSrgbS3tcDxt1Ext;
					break;
				case Components.GreyAlpha:
					pixelFormat = PixelFormat.LuminanceAlpha;
					internalFormat = InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
					break;
				case Components.RedGreenBlue:
					pixelFormat = PixelFormat.Rgb;
					internalFormat = InternalFormat.CompressedSrgbS3tcDxt1Ext;
					break;
				case Components.RedGreenBlueAlpha:
					pixelFormat = PixelFormat.Rgba;
					internalFormat = InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
					break;
				default:
					throw new NotSupportedException($"The colors format {comp} is not supported");
			}
			
			Skybox skybox = new Skybox(new ivec2(x, y), name, internalFormat, TextureFiltering.Linear);

			GL.Finish();

			Engine.ThreadPool.Schedule(
				() => {
					// 0 = positive x face
					// 1 = negative x face
					// 2 = positive y face
					// 3 = negative y face
					// 4 = positive z face
					// 5 = negative z face
					
					for (int i = 0; i < 6; i++)
					{
						byte[] image;
			
						try
						{
							image = StbImage.Load($"{path}/{(string)jsonRoot[jsonDataNames[i]]}", out int _, out int _, out Components _, comp);
						}
						catch (IOException)
						{
							throw new IOException($"Unable to load image {path}/{(string)jsonRoot[jsonDataNames[i]]} from disk");
						}
				
						skybox.PutData(image, i, pixelFormat);
					}
					
					GL.Finish();
			
					Logger.Info($"Skybox \"{path}\" loaded (id: {(int)skybox})");
			
					_loadedSkyboxes.Enqueue(new Tuple<string, Skybox>(path, skybox));
				});
		}

		private void SkyboxUpdate()
		{
			while (_loadedSkyboxes.TryDequeue(out Tuple<string, Skybox> data))
			{
				_skyboxes[data.Item1].ValidateLoading(data.Item2);
			}
		}
		
		#endregion

		#region Materials

		private Dictionary<string, Material> _materials = new Dictionary<string, Material>();

		public Material RequestMaterial(string name)
		{
			if (!_materials.ContainsKey(name))
			{
				Logger.Info($"Loading material \"{name}\"");
				_materials.Add(name, new Material(name));
			}

			return _materials[name];
		}

		#endregion

		#region ShaderPrograms

		private Dictionary<Dictionary<ShaderType, string[]>, ShaderProgram> _shaderPrograms = new Dictionary<Dictionary<ShaderType, string[]>, ShaderProgram>(new ShaderProgramEqualityComparer());

		public ShaderProgram RequestShaderProgram(ShaderProgramRequest request)
		{
			if (!_shaderPrograms.ContainsKey(request.Data))
			{
				Logger.Info("Loading shader program");
				
				ShaderProgram shaderProgram = new ShaderProgram(request.Data);
				
				_shaderPrograms.Add(request.Data, shaderProgram);
				Logger.Info($"Shader program loaded (id: {(int)shaderProgram})");
			}

			return _shaderPrograms[request.Data];
		}

		#endregion
		
		#region Meshes
		
		private Dictionary<string, ResourceHandler<Mesh>> _meshes = new Dictionary<string, ResourceHandler<Mesh>>();
		private ConcurrentQueue<Tuple<string, Mesh>> _loadedMeshes = new ConcurrentQueue<Tuple<string, Mesh>>();
		
		public void RequestMesh(string name, ResourceHandler<Mesh>.ResourceCallback callback)
		{
			if (!_meshes.ContainsKey(name))
			{
				_meshes.Add(name, new ResourceHandler<Mesh>());
				LoadMesh(name);
			}
			
			_meshes[name].AddCallback(callback);
		}

		private void LoadMesh(string name)
		{
			Logger.Info($"Loading mesh \"{name}\"");
			
			Mesh mesh = new Mesh();

			GL.Finish();
			
			Engine.ThreadPool.Schedule(() => {
				mesh.LoadFromFile(name);
				
				GL.Finish();
				
				Logger.Info($"Mesh \"{name}\" loaded");
				
				_loadedMeshes.Enqueue(new Tuple<string, Mesh>(name, mesh));
			});
		}

		private void MeshUpdate()
		{
			while (_loadedMeshes.TryDequeue(out Tuple<string, Mesh> data))
			{
				_meshes[data.Item1].ValidateLoading(data.Item2);
			}
		}
		
		#endregion
	}
}