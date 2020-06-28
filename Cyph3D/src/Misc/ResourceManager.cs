using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Json;
using Cyph3D.Enumerable;
using Cyph3D.Extension;
using Cyph3D.GLObject;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using StbImageSharp;

namespace Cyph3D.Misc
{
	public class ResourceManager : IDisposable
	{
		public void Update()
		{
			TextureUpdate();
			CubemapUpdate();
		}
		
		public void Dispose()
		{
			foreach (ResourceHandler<Texture> handler in _textures.Values)
			{
				handler.Dispose();
			}
			foreach (ResourceHandler<Cubemap> handler in _cubemaps.Values)
			{
				handler.Dispose();
			}
			foreach (ShaderProgram shaderProgram in _shaderPrograms.Values)
			{
				shaderProgram.Dispose();
			}
			foreach (Mesh mesh in _meshes.Values)
			{
				mesh.Dispose();
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

			ivec2 size;
			ColorComponents comp;
			
			try
			{
				using Stream stream = File.OpenRead(path);
				StbImageExt.StbImageInfo(stream, out size, out comp);
			}
			catch (IOException)
			{
				throw new IOException($"Unable to load image {path} from disk");
			}

			InternalFormat internalFormat;
			PixelFormat pixelFormat;
			switch (comp)
			{
				case ColorComponents.Grey:
					pixelFormat = PixelFormat.Luminance;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbS3tcDxt1Ext : InternalFormat.CompressedRedRgtc1;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8 : InternalFormat.Red;
					break;
				case ColorComponents.GreyAlpha:
					pixelFormat = PixelFormat.LuminanceAlpha;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8Alpha8 : InternalFormat.Rgba8;
					break;
				case ColorComponents.RedGreenBlue:
					pixelFormat = PixelFormat.Rgb;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbS3tcDxt1Ext : InternalFormat.CompressedRgbS3tcDxt1Ext;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8 : InternalFormat.Rgb8;
					break;
				case ColorComponents.RedGreenBlueAlpha:
					pixelFormat = PixelFormat.Rgba;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8Alpha8 : InternalFormat.Rgba8;
					break;
				default:
					throw new NotSupportedException($"The colors format {comp} is not supported");
			}
			
			Texture texture = new Texture(size, internalFormat, TextureFiltering.Linear);
			
			GL.Finish();
			
			Engine.ThreadPool.Schedule(() => {
				ImageResult image;

				try
				{
					using Stream stream = File.OpenRead(path);
					image = ImageResult.FromStream(stream, comp);
				}
				catch (IOException)
				{
					throw new IOException($"Unable to load image {path} from disk");
				}
				
				texture.PutData(image.Data, pixelFormat);
				
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

		#region Cubemaps

		private Dictionary<string, ResourceHandler<Cubemap>> _cubemaps = new Dictionary<string, ResourceHandler<Cubemap>>();
		private ConcurrentQueue<Tuple<string, Cubemap>> _loadedCubemaps = new ConcurrentQueue<Tuple<string, Cubemap>>();

		public void RequestImageCubemap(string name, ResourceHandler<Cubemap>.ResourceCallback callback, bool sRGB = false, bool compressed = false)
		{
			string path = $"resources/cubemaps/{name}";
			
			if (!_cubemaps.ContainsKey(path))
			{
				_cubemaps.Add(path, new ResourceHandler<Cubemap>());
				LoadImageCubemap(path, sRGB, compressed);
			}
			
			_cubemaps[path].AddCallback(callback);
		}

		private void LoadImageCubemap(string path, bool sRGB, bool compressed)
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
			
			Logger.Info($"Loading image cubemap \"{path}\"");
			
			JsonObject jsonRoot = (JsonObject)JsonValue.Parse(File.ReadAllText($"{path}/cubemap.json"));
			
			ivec2 size;
			ColorComponents comp;
			
			try
			{
				using Stream stream = File.OpenRead($"{path}/{(string)jsonRoot["front"]}");
				StbImageExt.StbImageInfo(stream, out size, out comp);
			}
			catch (IOException)
			{
				throw new IOException($"Unable to load image {path}/{(string)jsonRoot["front"]} from disk");
			}

			InternalFormat internalFormat;
			PixelFormat pixelFormat;
			switch (comp)
			{
				case ColorComponents.Grey:
					pixelFormat = PixelFormat.Luminance;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbS3tcDxt1Ext : InternalFormat.CompressedRedRgtc1;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8 : InternalFormat.Red;
					break;
				case ColorComponents.GreyAlpha:
					pixelFormat = PixelFormat.LuminanceAlpha;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8Alpha8 : InternalFormat.Rgba8;
					break;
				case ColorComponents.RedGreenBlue:
					pixelFormat = PixelFormat.Rgb;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbS3tcDxt1Ext : InternalFormat.CompressedRgbS3tcDxt1Ext;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8 : InternalFormat.Rgb8;
					break;
				case ColorComponents.RedGreenBlueAlpha:
					pixelFormat = PixelFormat.Rgba;
					if (compressed)
						internalFormat = sRGB ? InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext : InternalFormat.CompressedRgbaS3tcDxt5Ext;
					else
						internalFormat = sRGB ? InternalFormat.Srgb8Alpha8 : InternalFormat.Rgba8;
					break;
				default:
					throw new NotSupportedException($"The colors format {comp} is not supported");
			}
			
			Cubemap cubemap = new Cubemap(size, internalFormat, TextureFiltering.Linear);

			// 0 = positive x face
			// 1 = negative x face
			// 2 = positive y face
			// 3 = negative y face
			// 4 = positive z face
			// 5 = negative z face
			
			for (int i = 0; i < 6; i++)
			{
				ImageResult image;
			
				try
				{
					using Stream stream = File.OpenRead($"{path}/{(string)jsonRoot[jsonDataNames[i]]}");
					image = ImageResult.FromStream(stream);
				}
				catch (IOException)
				{
					throw new IOException($"Unable to load image {path}/{(string)jsonRoot[jsonDataNames[i]]} from disk");
				}
				
				cubemap.PutData(image.Data, i, pixelFormat);
			}
			
			Logger.Info($"Cubemap \"{path}\" loaded (id: {(int)cubemap})");
			
			_loadedCubemaps.Enqueue(new Tuple<string, Cubemap>(path, cubemap));
		}

		private void CubemapUpdate()
		{
			while (_loadedCubemaps.TryDequeue(out Tuple<string, Cubemap> data))
			{
				_cubemaps[data.Item1].ValidateLoading(data.Item2);
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
				_materials.Add(name, new Material(name, this));
			}

			return _materials[name];
		}

		#endregion

		#region ShaderPrograms

		private Dictionary<string, ShaderProgram> _shaderPrograms = new Dictionary<string, ShaderProgram>();

		public ShaderProgram RequestShaderProgram(string name)
		{
			if (!_shaderPrograms.ContainsKey(name))
			{
				Logger.Info($"Loading shader program \"{name}\"");
				ShaderProgram shaderProgram = new ShaderProgram(name);
				_shaderPrograms.Add(name, shaderProgram);
				Logger.Info($"Shader program \"{name}\" loaded (id: {(int)shaderProgram})");
			}

			return _shaderPrograms[name];
		}

		#endregion
		
		#region Meshes
		
		private Dictionary<string, Mesh> _meshes = new Dictionary<string, Mesh>();
		
		public Mesh RequestMesh(string name)
		{
			if (!_meshes.ContainsKey(name))
			{
				Logger.Info($"Loading mesh \"{name}\"");
				_meshes.Add(name, new Mesh(name));
				Logger.Info($"Mesh \"{name}\" loaded");
			}

			return _meshes[name];
		}
		
		#endregion
	}
}