using System;
using System.Collections.Generic;
using Cyph3D.Misc;

namespace Cyph3D
{
	public class Scene : IDisposable
	{
		public Transform Root { get; } = new Transform();
		public List<SceneObject> Objects { get; } = new List<SceneObject>();
		public LightManager LightManager { get; } = new LightManager();
		public Camera Camera { get; }

		public Scene(Camera camera = null)
		{
			Camera = camera ?? new Camera();
		}

		public void Dispose()
		{
			Objects.ForEach(o => o.Invalidate());
			LightManager?.Dispose();
		}

		public void Add(SceneObject obj)
		{
			Objects.Add(obj);

			if (obj is PointLight pointLight)
			{
				LightManager.AddPointLight(pointLight);
			}
		}
		
		public void Remove(SceneObject obj)
		{
			Objects.Remove(obj);

			if (obj is PointLight pointLight)
			{
				LightManager.RemovePointLight(pointLight);
			}
		}
	}
}