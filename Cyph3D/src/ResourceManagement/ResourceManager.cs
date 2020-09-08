using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Json;
using Cyph3D.GLObject;
using Cyph3D.Misc;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.ResourceManagement
{
	public class ResourceManager : IDisposable
	{
		public void Update()
		{
			ImageUpdate();
			SkyboxUpdate();
			MeshUpdate();
		}
		
		public void Dispose()
		{
			foreach (Image image in _images.Values)
			{
				image.Dispose();
			}
			foreach (Material material in _materials.Values)
			{
				material.Dispose();
			}
			foreach (Skybox skybox in _skyboxes.Values)
			{
				skybox.Dispose();
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
		
		#region Images
		
		private Dictionary<string, Image> _images = new Dictionary<string, Image>();
		private ConcurrentQueue<(Image, ImageFinalizationData)> _imagesLoadingFinalizationQueue = new ConcurrentQueue<(Image, ImageFinalizationData)>();

		public Image RequestImage(string name, bool sRGB = false, bool compressed = false)
		{
			if (!_images.ContainsKey(name))
			{
				Image image = new Image(name);
				_images.Add(name, image);
				LoadImage(image, name, sRGB, compressed);
			}

			return _images[name];
		}

		private void LoadImage(Image image, string name, bool sRGB, bool compressed)
		{
			Logger.Info($"Loading image texture \"{name}\"");
			
			string path = $"resources/materials/{name}";
			
			Engine.ThreadPool.Schedule(() => {
				ImageFinalizationData imageData = Image.LoadFromFile(path, sRGB, compressed);
				
				_imagesLoadingFinalizationQueue.Enqueue((image, imageData));
			});
		}

		private void ImageUpdate()
		{
			while (_imagesLoadingFinalizationQueue.TryDequeue(out (Image, ImageFinalizationData) data))
			{
				data.Item1.FinalizeLoading(data.Item2);
				
				Logger.Info($"Texture \"{data.Item1.Name}\" loaded");
			}
		}
		
		#endregion

		#region Skybox

		private Dictionary<string, Skybox> _skyboxes = new Dictionary<string, Skybox>();
		private ConcurrentQueue<(Skybox, SkyboxFinalizationData)> _skyboxLoadingFinalizationQueue = new ConcurrentQueue<(Skybox, SkyboxFinalizationData)>();

		public Skybox RequestSkybox(string name)
		{
			if (!_skyboxes.ContainsKey(name))
			{
				Skybox skybox = new Skybox(name);
				_skyboxes.Add(name, skybox);
				LoadSkybox(name, skybox);
			}

			return _skyboxes[name];
		}

		private void LoadSkybox(string name, Skybox skybox)
		{
			Logger.Info($"Loading skybox \"{name}\"");
			
			string path = $"resources/skyboxes/{name}";
			
			JsonObject jsonRoot = (JsonObject)JsonValue.Parse(File.ReadAllText($"{path}/skybox.json"));

			string[] facesPath = 
			{
				$"{path}/{(string)jsonRoot["right"]}",
				$"{path}/{(string)jsonRoot["left"]}",
				$"{path}/{(string)jsonRoot["down"]}",
				$"{path}/{(string)jsonRoot["up"]}",
				$"{path}/{(string)jsonRoot["front"]}",
				$"{path}/{(string)jsonRoot["back"]}"
			};

			Engine.ThreadPool.Schedule(() => {
				SkyboxFinalizationData skyboxData = Skybox.LoadFromFiles(facesPath);

				_skyboxLoadingFinalizationQueue.Enqueue((skybox, skyboxData));
			});
		}

		private void SkyboxUpdate()
		{
			while (_skyboxLoadingFinalizationQueue.TryDequeue(out (Skybox, SkyboxFinalizationData) data))
			{
				data.Item1.FinalizeLoading(data.Item2);
				
				Logger.Info($"Skybox \"{data.Item1.Name}\" loaded");
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
		
		private Dictionary<string, Mesh> _meshes = new Dictionary<string, Mesh>();
		private ConcurrentQueue<(Mesh, MeshFinalizationData)> _meshLoadingFinalizationQueue = new ConcurrentQueue<(Mesh, MeshFinalizationData)>();
		
		public Mesh RequestMesh(string name)
		{
			if (!_meshes.ContainsKey(name))
			{
				Mesh mesh = new Mesh(name);
				_meshes.Add(name, mesh);
				LoadMesh(name, mesh);
			}

			return _meshes[name];
		}

		private void LoadMesh(string name, Mesh mesh)
		{
			Logger.Info($"Loading mesh \"{name}\"");
			
			Engine.ThreadPool.Schedule(() => {
				MeshFinalizationData meshData = Mesh.LoadFromFile(name);

				_meshLoadingFinalizationQueue.Enqueue((mesh, meshData));
			});
		}

		private void MeshUpdate()
		{
			while (_meshLoadingFinalizationQueue.TryDequeue(out (Mesh, MeshFinalizationData) data))
			{
				data.Item1.FinalizeLoading(data.Item2);
				Logger.Info($"Mesh \"{data.Item1.Name}\" loaded");
			}
		}
		
		#endregion
	}
}