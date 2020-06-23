using System;
using System.Collections.Generic;
using Cyph3D.Lighting;
using Cyph3D.Misc;

namespace Cyph3D
{
	public class LightManager : IDisposable
	{
#region PointLight
		private List<PointLight> _pointLights = new List<PointLight>();

		private NativeArray<PointLight.NativeLightData> _pointLightsNative = new NativeArray<PointLight.NativeLightData>(0);
		public NativeArray<PointLight.NativeLightData> PointLightsNative
		{
			get
			{
				_pointLightsNative.Dispose();
		
				int count = _pointLights.Count;
		
				_pointLightsNative = new NativeArray<PointLight.NativeLightData>(count);
				for (int i = 0; i < count; i++)
				{
					_pointLightsNative[i] = _pointLights[i].NativeLight;
				}

				return _pointLightsNative;
			}
		}
#endregion

#region DirectionalLight
		private List<DirectionalLight> _directionalLights = new List<DirectionalLight>();

		private NativeArray<DirectionalLight.NativeLightData> _directionalLightsNative = new NativeArray<DirectionalLight.NativeLightData>(0);
		public NativeArray<DirectionalLight.NativeLightData> DirectionalLightsNative
		{
			get
			{
				_directionalLightsNative.Dispose();
				
				int count = _directionalLights.Count;
				
				_directionalLightsNative = new NativeArray<DirectionalLight.NativeLightData>(count);
				for (int i = 0; i < count; i++)
				{
					_directionalLightsNative[i] = _directionalLights[i].NativeLight;
				}

				return _directionalLightsNative;
			}
		}
#endregion
		
		public void Add(Light light)
		{
			switch (light)
			{
				case PointLight pointLight:
					if (!_pointLights.Contains(pointLight))
					{
						_pointLights.Add(pointLight);
					}
					break;
				case DirectionalLight directionalLight:
					if (!_directionalLights.Contains(directionalLight))
					{
						_directionalLights.Add(directionalLight);
					}
					break;
			}
		}
		
		public void Remove(Light light)
		{
			switch (light)
			{
				case PointLight pointLight:
					if (_pointLights.Contains(pointLight))
					{
						_pointLights.Remove(pointLight);
					}
					break;
				case DirectionalLight directionalLight:
					if (_directionalLights.Contains(directionalLight))
					{
						_directionalLights.Remove(directionalLight);
					}
					break;
			}
		}

		public void Dispose()
		{
			PointLightsNative?.Dispose();
			DirectionalLightsNative?.Dispose();
		}
	}
}