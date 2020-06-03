using System;
using System.Collections.Generic;
using Cyph3D.Extension;
using Cyph3D.Light;

namespace Cyph3D
{
	public class LightManager : IDisposable
	{
		private List<PointLight> _pointLights = new List<PointLight>();

		private NativeArray<PointLight.NativePointLight> _pointLightsNative = new NativeArray<PointLight.NativePointLight>(0);
		public NativeArray<PointLight.NativePointLight> PointLightsNative
		{
			get
			{
				_pointLightsNative.Dispose();
		
				int count = _pointLights.Count;
		
				_pointLightsNative = new NativeArray<PointLight.NativePointLight>(count);
				for (int i = 0; i < count; i++)
				{
					_pointLightsNative[i] = _pointLights[i].NativeLight;
				}

				return _pointLightsNative;
			}
		}
		
		public void AddPointLight(PointLight light)
		{
			if (!_pointLights.Contains(light))
			{
				_pointLights.Add(light);
			}
		}
		
		public void RemovePointLight(PointLight light)
		{
			if (_pointLights.Contains(light))
			{
				_pointLights.Remove(light);
			}
		}

		public void ClearAll()
		{
			_pointLights.Clear();
		}

		public void Dispose()
		{
			PointLightsNative?.Dispose();
		}
	}
}