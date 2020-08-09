using System.Collections.Generic;
using System.IO;
using System.Json;
using Cyph3D.GLObject;
using Cyph3D.ResourceManagement;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D
{
	public class MaterialShaderProgram
	{
		public ShaderProgram ShaderProgram { get; }
		public Dictionary<string, MapDefinition> MapDefinitions { get; } = new Dictionary<string, MapDefinition>();

		public MaterialShaderProgram(string shaderName, ResourceManager resourceManager = null)
		{
			ShaderProgram = (resourceManager ?? Engine.Scene.ResourceManager).RequestShaderProgram(
				new ShaderProgramRequest()
					.WithShader(ShaderType.VertexShader,
						"internal/gbuffer/renderToGbuffer")
					.WithShader(ShaderType.FragmentShader,
						$"materialLayout/{shaderName}")
			);
			
			JsonObject root = (JsonObject)JsonValue.Parse(File.ReadAllText($"resources/shaders/materialLayout/{shaderName}.json"));

			foreach ((string name, JsonValue definition) in root)
			{
				JsonArray defaultDataJson = (JsonArray) definition["default_data"];
				byte[] defaultData = new byte[defaultDataJson.Count];

				for (int i = 0; i < defaultDataJson.Count; i++)
				{
					defaultData[i] = defaultDataJson[i];
				}
				
				MapDefinitions.Add(name, new MapDefinition(definition["compressed"], definition["srgb"], defaultData));
			}
		}
	}
}