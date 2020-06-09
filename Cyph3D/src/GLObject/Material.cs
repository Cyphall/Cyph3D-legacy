using System.Collections.Generic;
using Cyph3D.Extension;
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

		public string Name { get; }
		public bool IsLit { get; }

		private static Dictionary<string, Material> _materials = new Dictionary<string, Material>();
		private bool _loadedMessageDisplayed;

		public Material(string name, bool isLit)
		{
			_shaderProgram = ShaderProgram.Get("deferred/firstPass");
			
			_shaderProgram.SetValue("colorMap", 0);
			_shaderProgram.SetValue("normalMap", 1);
			_shaderProgram.SetValue("roughnessMap", 2);
			_shaderProgram.SetValue("displacementMap", 3);
			_shaderProgram.SetValue("metallicMap", 4);
			_shaderProgram.SetValue("emissiveMap", 5);
			
			if (Texture.ExistsOnDisk($"{name}/col"))
			{
				_colorMap = Texture.FromFile($"{name}/col", true, true);
			}
			else
			{
				_colorMap = new Texture(new ivec2(1), InternalFormat.CompressedSrgbS3tcDxt1Ext);
				_colorMap.PutData(new byte[]{255, 0, 255});
			}
			
			if (Texture.ExistsOnDisk($"{name}/nrm"))
			{
				_normalMap = Texture.FromFile($"{name}/nrm");
			}
			else
			{
				_normalMap = new Texture(new ivec2(1), InternalFormat.Rgb8);
				_normalMap.PutData(new byte[]{128, 128, 255});
			}
			
			if (Texture.ExistsOnDisk($"{name}/rgh"))
			{
				_roughnessMap = Texture.FromFile($"{name}/rgh", compressed: true);
			}
			else
			{
				_roughnessMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
				_roughnessMap.PutData(new byte[]{128}, PixelFormat.Luminance);
			}
			
			if (Texture.ExistsOnDisk($"{name}/disp"))
			{
				_displacementMap = Texture.FromFile($"{name}/disp", compressed: true);
			}
			else
			{
				_displacementMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
				_displacementMap.PutData(new byte[]{255}, PixelFormat.Luminance);
			}
			
			if (Texture.ExistsOnDisk($"{name}/met"))
			{
				_metallicMap = Texture.FromFile($"{name}/met", compressed: true);
			}
			else
			{
				_metallicMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
				_metallicMap.PutData(new byte[]{0, 0, 0});
			}
			
			if (Texture.ExistsOnDisk($"{name}/emis"))
			{
				_emissiveMap = Texture.FromFile($"{name}/emis", compressed: true);
			}
			else
			{
				_emissiveMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
				_emissiveMap.PutData(new byte[]{0, 0, 0});
			}

			Name = name;
			IsLit = isLit;
			_materials.Add(name, this);
		}

		public bool Bind(mat4 model, mat4 view, mat4 projection, vec3 cameraPos)
		{
			if (!_colorMap.IsReady ||
			    !_normalMap.IsReady ||
			    !_roughnessMap.IsReady ||
			    !_displacementMap.IsReady ||
			    !_metallicMap.IsReady ||
			    !_emissiveMap.IsReady)
			{
				return false;
			}

			if (!_loadedMessageDisplayed)
			{
				Logger.Info($"Material \"{Name}\" loaded");
				_loadedMessageDisplayed = true;
			}

			_colorMap.Bind(0);
			_normalMap.Bind(1);
			_roughnessMap.Bind(2);
			_displacementMap.Bind(3);
			_metallicMap.Bind(4);
			_emissiveMap.Bind(5);
			
			_shaderProgram.SetValue("model", model);
			_shaderProgram.SetValue("view", view);
			_shaderProgram.SetValue("projection", projection);
			_shaderProgram.SetValue("viewPos", cameraPos);
			_shaderProgram.SetValue("isLit", IsLit ? 1 : 0);
			
			
			_shaderProgram.Bind();

			return true;
		}
		
		public static Material GetOrLoad(string name, bool isLit)
		{
			if (!_materials.ContainsKey(name))
			{
				Logger.Info($"Loading material \"{name}\"");
				// ReSharper disable once ObjectCreationAsStatement
				new Material(name, isLit);
			}

			return _materials[name];
		}
	}
}