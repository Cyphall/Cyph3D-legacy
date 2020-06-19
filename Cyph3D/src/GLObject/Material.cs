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
		
		private static Texture _defaultColorMap;
		private static Texture _defaultNormalMap;
		private static Texture _defaultRoughnessMap;
		private static Texture _defaultDisplacementMap;
		private static Texture _defaultMetallicMap;
		private static Texture _defaultEmissiveMap;
		
		private bool _loadedMessageDisplayed;

		public string Name { get; }
		public bool IsLit { get; }

		private static Dictionary<string, Material> _materials = new Dictionary<string, Material>();
		
		public static Material Default { get; private set; }

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
			
			if (Texture.ExistsOnDisk($"{name}/nrm"))
			{
				_normalMap = Texture.FromFile($"{name}/nrm");
			}
			
			if (Texture.ExistsOnDisk($"{name}/rgh"))
			{
				_roughnessMap = Texture.FromFile($"{name}/rgh", compressed: true);
			}
			
			if (Texture.ExistsOnDisk($"{name}/disp"))
			{
				_displacementMap = Texture.FromFile($"{name}/disp", compressed: true);
			}
			
			if (Texture.ExistsOnDisk($"{name}/met"))
			{
				_metallicMap = Texture.FromFile($"{name}/met", compressed: true);
			}
			
			if (Texture.ExistsOnDisk($"{name}/emis"))
			{
				_emissiveMap = Texture.FromFile($"{name}/emis", compressed: true);
			}

			Name = name;
			IsLit = isLit;
			_materials.Add(name, this);
		}

		public void Bind(mat4 model, mat4 view, mat4 projection, vec3 cameraPos)
		{
			if (!_loadedMessageDisplayed)
			{
				Logger.Info($"Material \"{Name}\" loaded");
				_loadedMessageDisplayed = true;
			}

			if (_colorMap != null && _colorMap.IsReady)
				_colorMap.Bind(0);
			else
				_defaultColorMap.Bind(0);
			
			if (_normalMap != null && _normalMap.IsReady)
				_normalMap.Bind(1);
			else
				_defaultNormalMap.Bind(1);
			
			if (_roughnessMap != null && _roughnessMap.IsReady)
				_roughnessMap.Bind(2);
			else
				_defaultRoughnessMap.Bind(2);
			
			if (_displacementMap != null && _displacementMap.IsReady)
				_displacementMap.Bind(3);
			else
				_defaultDisplacementMap.Bind(3);
			
			if (_metallicMap != null && _metallicMap.IsReady)
				_metallicMap.Bind(4);
			else
				_defaultMetallicMap.Bind(4);
			
			if (_emissiveMap != null && _emissiveMap.IsReady)
				_emissiveMap.Bind(5);
			else
				_defaultEmissiveMap.Bind(5);
			
			_shaderProgram.SetValue("model", model);
			_shaderProgram.SetValue("view", view);
			_shaderProgram.SetValue("projection", projection);
			_shaderProgram.SetValue("viewPos", cameraPos);
			_shaderProgram.SetValue("isLit", IsLit ? 1 : 0);
			
			
			_shaderProgram.Bind();
		}
		
		public static void InitializeDefault()
		{
			Default = new Material("default", false);
			
			_defaultColorMap = new Texture(new ivec2(1), InternalFormat.CompressedSrgbS3tcDxt1Ext);
			_defaultColorMap.PutData(new byte[]{255, 0, 255});
			
			_defaultNormalMap = new Texture(new ivec2(1), InternalFormat.Rgb8);
			_defaultNormalMap.PutData(new byte[]{128, 128, 255});
			
			_defaultRoughnessMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
			_defaultRoughnessMap.PutData(new byte[]{128}, PixelFormat.Luminance);
		
			_defaultDisplacementMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
			_defaultDisplacementMap.PutData(new byte[]{255}, PixelFormat.Luminance);
		
			_defaultMetallicMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
			_defaultMetallicMap.PutData(new byte[]{0, 0, 0});
		
			_defaultEmissiveMap = new Texture(new ivec2(1), InternalFormat.CompressedRgbS3tcDxt1Ext);
			_defaultEmissiveMap.PutData(new byte[]{0, 0, 0});
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