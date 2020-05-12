using System;
using System.Collections.Generic;
using Cyph3D.Misc;

namespace Cyph3D
{
	public class LightManager : IDisposable
	{
		private List<PointLight> _pointLights = new List<PointLight>();

		public bool PointLightsChanged { get; private set; }
		private NativeArray<PointLight.NativePointLight> _pointLightsNative = new NativeArray<PointLight.NativePointLight>(0);
		public NativeArray<PointLight.NativePointLight> PointLightsNative
		{
			get
			{
				if (PointLightsChanged)
				{
					_pointLightsNative.Dispose();
			
					int count = _pointLights.Count;
			
					_pointLightsNative = new NativeArray<PointLight.NativePointLight>(count);
					for (int i = 0; i < count; i++)
					{
						_pointLightsNative[i] = _pointLights[i].GLLight;
					}

					PointLightsChanged = false;
				}

				return _pointLightsNative;
			}
		}

		public void AddPointLight(PointLight light)
		{
			if (!_pointLights.Contains(light))
			{
				_pointLights.Add(light);
				PointLightsChanged = true;
			}
		}
		
		public void RemovePointLight(PointLight light)
		{
			if (_pointLights.Contains(light))
			{
				_pointLights.Remove(light);
				PointLightsChanged = true;
			}
		}

		public void Dispose()
		{
			PointLightsNative?.Dispose();
		}
	}
}