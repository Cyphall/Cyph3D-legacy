using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using Cyph3D.Extension;
using Cyph3D.GLObject;
using Cyph3D.Lighting;
using Cyph3D.Misc;
using GlmSharp;
using ObjLoader.Loader.Common;

namespace Cyph3D
{
	public class Scene : IDisposable
	{
		public Transform Root { get; } = new Transform();
		public List<SceneObject> Objects { get; } = new List<SceneObject>();
		public LightManager LightManager { get; } = new LightManager();
		public Camera Camera { get; }
		public string Name { get; set; }
		public Cubemap Skybox { get; }

		public Scene(Camera camera = null, string name = null)
		{
			Camera = camera ?? new Camera();
			Name = name ?? "Untitled Scene";
			Skybox = Cubemap.FromFiles("Skybox/space{0}.png", true, true);
		}

		public void Dispose()
		{
			LightManager?.Dispose();
		}

		public void Add(SceneObject obj)
		{
			Objects.Add(obj);

			if (obj is Light light)
			{
				LightManager.Add(light);
			}
		}
		
		public void Remove(SceneObject obj)
		{
			RemoveFromHierarchy(obj);
			RemoveFromObjects(obj);
		}

		private void RemoveFromHierarchy(SceneObject obj)
		{
			obj.Transform.Parent.Children.Remove(obj.Transform);
		}
		
		private void RemoveFromObjects(SceneObject obj)
		{
			Objects.Remove(obj);
			
			if (obj is Light light)
			{
				LightManager.Remove(light);
			}
			
			for (int i = 0; i < obj.Transform.Children.Count; i++)
			{
				RemoveFromObjects(obj.Transform.Children[i].Owner);
			}
		}

		public static Scene Load(string name)
		{
			JsonObject jsonRoot = (JsonObject)JsonValue.Parse(File.ReadAllText($"resources/scenes/{name}.json"));

			int version = jsonRoot["version"];
			

			JsonArray cameraPosArray = (JsonArray)jsonRoot["camera"]["position"];
			JsonArray cameraSphCoordsArray = (JsonArray)jsonRoot["camera"]["spherical_coords"];
			
			Camera camera = new Camera(
				new vec3(cameraPosArray[0], cameraPosArray[1], cameraPosArray[2]),
				new vec2(cameraSphCoordsArray[0], cameraSphCoordsArray[1])
			);
			
			Scene scene = new Scene(camera, name);
			
			
			JsonArray jsonSceneObjects = (JsonArray)jsonRoot["objects"];

			foreach (JsonValue value in jsonSceneObjects)
			{
				ParseSceneObject((JsonObject)value, scene, scene.Root, version);
			}

			return scene;
		}

		private static void ParseSceneObject(JsonObject jsonObject, Scene scene, Transform parent, int version)
		{
			JsonObject jsonData = (JsonObject)jsonObject["data"];
			
			SceneObject sceneObject;

			JsonArray positionArray = (JsonArray)jsonObject["position"];
			vec3 position = new vec3(positionArray[0], positionArray[1], positionArray[2]);

			JsonArray rotationArray = (JsonArray)jsonObject["rotation"];
			vec3 rotation = new vec3(rotationArray[0], rotationArray[1], rotationArray[2]);

			JsonArray scaleArray = (JsonArray)jsonObject["scale"];
			vec3 scale = new vec3(scaleArray[0], scaleArray[1], scaleArray[2]);

			string name = jsonObject["name"];

			switch ((string)jsonObject["type"])
			{
				case "mesh_object":
					JsonArray velocityArray = (JsonArray)jsonData["velocity"];
					vec3 velocity = new vec3(velocityArray[0], velocityArray[1], velocityArray[2]);
					
					JsonArray angularVelocityArray = (JsonArray)jsonData["angular_velocity"];
					vec3 angularVelocity = new vec3(angularVelocityArray[0], angularVelocityArray[1], angularVelocityArray[2]);

					string meshName = jsonData["mesh"];
					Mesh mesh = meshName.IsNullOrEmpty() ? null : Mesh.GetOrLoad(meshName);
					
					string materialName = jsonData["material"];
					Material material = materialName.IsNullOrEmpty() ? null : Material.GetOrLoad(materialName);
					
					sceneObject = new MeshObject(parent, material, mesh, name, position, rotation, scale, velocity, angularVelocity);
					break;
				case "point_light":
				{
					JsonArray colorArray = (JsonArray)jsonData["color"];
					vec3 srgbColor = new vec3(colorArray[0], colorArray[1], colorArray[2]);
					
					float intensity = jsonData["intensity"];
					
					sceneObject = new PointLight(parent, srgbColor, intensity, name, position);
				}break;
				case "directional_light":
				{
					JsonArray colorArray = (JsonArray)jsonData["color"];
					vec3 srgbColor = new vec3(colorArray[0], colorArray[1], colorArray[2]);
					
					float intensity = jsonData["intensity"];
					
					sceneObject = new DirectionalLight(parent, srgbColor, intensity, name, position);
				}break;
				default:
					throw new InvalidOperationException($"The object type {jsonObject["type"]} is not recognized");
			}
			
			scene.Add(sceneObject);

			foreach (JsonValue value in jsonObject["children"])
			{
				ParseSceneObject((JsonObject)value, scene, sceneObject.Transform, version);
			}
		}

		public void Save()
		{
			JsonObject jsonRoot = new JsonObject();
			
			jsonRoot.Add("version", 1);
			

			JsonObject jsonCamera = new JsonObject
			{
				{"position", ConvertHelper.JsonConvert(Camera.Position)},
				{"spherical_coords", ConvertHelper.JsonConvert(Camera.SphericalCoords)}
			};


			jsonRoot.Add("camera", jsonCamera);
			
			
			JsonArray objects = new JsonArray();

			foreach (Transform transform in Root.Children)
			{
				objects.Add(SerializeSceneObject(transform.Owner));
			}
			
			jsonRoot.Add("objects", objects);
			
			File.WriteAllText($"resources/scenes/{Name}.json", jsonRoot.ToString());
		}

		private JsonObject SerializeSceneObject(SceneObject sceneObject)
		{
			JsonObject jsonObject = new JsonObject();
			
			jsonObject.Add("position", ConvertHelper.JsonConvert(sceneObject.Transform.Position));
			jsonObject.Add("rotation", ConvertHelper.JsonConvert(sceneObject.Transform.EulerRotation));
			jsonObject.Add("scale", ConvertHelper.JsonConvert(sceneObject.Transform.Scale));
			jsonObject.Add("name", sceneObject.Name);
			
			JsonObject jsonData = new JsonObject();
			jsonObject.Add("data", jsonData);

			switch (sceneObject)
			{
				case MeshObject meshObject:
					jsonObject.Add("type", "mesh_object");
					
					jsonData.Add("velocity", ConvertHelper.JsonConvert(meshObject.Velocity));
					jsonData.Add("angular_velocity", ConvertHelper.JsonConvert(meshObject.AngularVelocity));
					jsonData.Add("mesh", meshObject.Mesh?.Name);
					
					jsonData.Add("material", meshObject.Material?.Name);
					break;
				case PointLight pointLight:
					jsonObject.Add("type", "point_light");
					
					jsonData.Add("color", ConvertHelper.JsonConvert(pointLight.SrgbColor));
					jsonData.Add("intensity", pointLight.Intensity);
					break;
				case DirectionalLight directionalLight:
					jsonObject.Add("type", "directional_light");
					
					jsonData.Add("color", ConvertHelper.JsonConvert(directionalLight.SrgbColor));
					jsonData.Add("intensity", directionalLight.Intensity);
					break;
				default:
					throw new InvalidOperationException();
			}
			
			JsonArray jsonChildren = new JsonArray();

			foreach (Transform childTransform in sceneObject.Transform.Children)
			{
				jsonChildren.Add(SerializeSceneObject(childTransform.Owner));
			}
			
			jsonObject.Add("children", jsonChildren);

			return jsonObject;
		}
	}
}