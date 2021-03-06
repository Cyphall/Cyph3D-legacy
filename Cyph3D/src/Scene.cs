﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using Cyph3D.GLObject;
using Cyph3D.Helper;
using Cyph3D.Lighting;
using Cyph3D.Misc;
using Cyph3D.ResourceManagement;
using Cyph3D.UI.Window;
using GlmSharp;

namespace Cyph3D
{
	public class Scene : IDisposable
	{
		public Transform Root { get; } = new Transform();
		public List<SceneObject> Objects { get; } = new List<SceneObject>();
		public LightManager LightManager { get; } = new LightManager();
		public Camera Camera { get; }
		public string Name { get; set; }
		public Skybox Skybox { get; set; }
		public ResourceManager ResourceManager { get; } = new ResourceManager();

		public Scene(Camera camera = null, string name = null)
		{
			Camera = camera ?? new Camera();
			Name = name ?? "Untitled Scene";
		}

		public void Dispose()
		{
			Skybox?.Dispose();
			LightManager.Dispose();
			ResourceManager.Dispose();
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

		public static void Load(string name)
		{
			Engine.Scene.Dispose();
			Engine.Scene = null;
			
			JsonObject jsonRoot = (JsonObject)JsonValue.Parse(File.ReadAllText($"resources/scenes/{name}.json"));

			int version = jsonRoot["version"];
			

			JsonArray cameraPosArray = (JsonArray)jsonRoot["camera"]["position"];
			JsonArray cameraSphCoordsArray = (JsonArray)jsonRoot["camera"]["spherical_coords"];
			
			Camera camera = new Camera(
				new vec3(cameraPosArray[0], cameraPosArray[1], cameraPosArray[2]),
				new vec2(cameraSphCoordsArray[0], cameraSphCoordsArray[1])
			);

			if (version >= 6)
			{
				camera.Exposure = jsonRoot["camera"]["exposure"];
			}
			
			Engine.Scene = new Scene(camera, name);

			if (version == 2)
			{
				if (!string.IsNullOrEmpty(jsonRoot["skybox"]))
				{
					Engine.Scene.Skybox = Engine.Scene.ResourceManager.RequestSkybox(jsonRoot["skybox"]);
				}
			}
			else if (version >= 3)
			{
				if (jsonRoot["skybox"].Count > 0)
				{
					float skyboxRotation = jsonRoot["skybox"]["rotation"];
					Engine.Scene.Skybox = Engine.Scene.ResourceManager.RequestSkybox(jsonRoot["skybox"]["name"]);
					Engine.Scene.Skybox.Rotation = skyboxRotation;
				}
			}
			
			
			JsonArray jsonSceneObjects = (JsonArray)jsonRoot["objects"];

			foreach (JsonValue value in jsonSceneObjects)
			{
				ParseSceneObject((JsonObject)value, Engine.Scene.Root, version);
			}
		}

		private static void ParseSceneObject(JsonObject jsonObject, Transform parent, int version)
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
					
					string materialName = jsonData["material"];
					Material material = string.IsNullOrEmpty(materialName) ? null : Engine.Scene.ResourceManager.RequestMaterial(materialName);
					
					sceneObject = new MeshObject(parent, material, null, name, position, rotation, scale, velocity, angularVelocity);
					
					string meshName = jsonData["mesh"];
					if (!string.IsNullOrEmpty(meshName))
						((MeshObject)sceneObject).Mesh = Engine.Scene.ResourceManager.RequestMesh(meshName);

					((MeshObject)sceneObject).ContributeShadows = version >= 5 ? (bool)jsonData["contributeShadows"] : true;
					
					break;
				case "point_light":
				{
					JsonArray colorArray = (JsonArray)jsonData["color"];
					vec3 srgbColor = new vec3(colorArray[0], colorArray[1], colorArray[2]);
					
					float intensity = jsonData["intensity"];
					bool castShadows = version >= 5 ? (bool)jsonData["castShadows"] : false;
					
					sceneObject = new PointLight(parent, srgbColor, intensity, name, position, rotation, castShadows);
				}break;
				case "directional_light":
				{
					JsonArray colorArray = (JsonArray)jsonData["color"];
					vec3 srgbColor = new vec3(colorArray[0], colorArray[1], colorArray[2]);
					
					float intensity = jsonData["intensity"];
					bool castShadows = version >= 4 ? (bool)jsonData["castShadows"] : false;
					
					sceneObject = new DirectionalLight(parent, srgbColor, intensity, name, position, rotation, castShadows);
				}break;
				default:
					throw new InvalidOperationException($"The object type {jsonObject["type"]} is not recognized");
			}
			
			Engine.Scene.Add(sceneObject);

			foreach (JsonValue value in jsonObject["children"])
			{
				ParseSceneObject((JsonObject)value, sceneObject.Transform, version);
			}
		}

		public void Save()
		{
			JsonObject jsonRoot = new JsonObject();
			
			jsonRoot.Add("version", 6);
			

			JsonObject jsonCamera = new JsonObject
			{
				{"position", ConvertHelper.JsonConvert(Camera.Position)},
				{"spherical_coords", ConvertHelper.JsonConvert(Camera.SphericalCoords)},
				{"exposure", Camera.Exposure}
			};


			jsonRoot.Add("camera", jsonCamera);
			
			JsonObject skybox = new JsonObject();
			if (Skybox != null)
			{
				skybox["name"] = Skybox.Name;
				skybox["rotation"] = Skybox.Rotation;
			}
			jsonRoot.Add("skybox", skybox);
			
			
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
					
					jsonData.Add("contributeShadows", meshObject.ContributeShadows);
					break;
				case PointLight pointLight:
					jsonObject.Add("type", "point_light");
					
					jsonData.Add("color", ConvertHelper.JsonConvert(pointLight.SrgbColor));
					jsonData.Add("intensity", pointLight.Intensity);
					jsonData.Add("castShadows", pointLight.CastShadows);
					break;
				case DirectionalLight directionalLight:
					jsonObject.Add("type", "directional_light");
					
					jsonData.Add("color", ConvertHelper.JsonConvert(directionalLight.SrgbColor));
					jsonData.Add("intensity", directionalLight.Intensity);
					jsonData.Add("castShadows", directionalLight.CastShadows);
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