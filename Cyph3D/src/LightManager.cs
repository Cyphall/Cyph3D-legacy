﻿using System;
using System.Collections.Generic;
using Cyph3D.Lighting;

namespace Cyph3D
{
	public class LightManager : IDisposable
	{
#region PointLight
		private List<PointLight> _pointLights = new List<PointLight>();
		
		public PointLight.NativeLightData[] PointLightsNative
		{
			get
			{
				PointLight.NativeLightData[] pointLightsNative = new PointLight.NativeLightData[_pointLights.Count];
				for (int i = 0; i < _pointLights.Count; i++)
				{
					pointLightsNative[i] = _pointLights[i].NativeLight;
				}

				return pointLightsNative;
			}
		}
#endregion

#region DirectionalLight
		private List<DirectionalLight> _directionalLights = new List<DirectionalLight>();

		public DirectionalLight.NativeLightData[] DirectionalLightsNative
		{
			get
			{
				DirectionalLight.NativeLightData[] directionalLightsNative = new DirectionalLight.NativeLightData[_directionalLights.Count];
				for (int i = 0; i < _directionalLights.Count; i++)
				{
					directionalLightsNative[i] = _directionalLights[i].NativeLight;
				}

				return directionalLightsNative;
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

		public void UpdateShadowMaps()
		{
			for (int i = 0; i < _directionalLights.Count; i++)
			{
				if (_directionalLights[i].CastShadows)
				{
					_directionalLights[i].UpdateShadowMap();
				}
			}
			
			for (int i = 0; i < _pointLights.Count; i++)
			{
				if (_pointLights[i].CastShadows)
				{
					_pointLights[i].UpdateShadowMap();
				}
			}
		}

		public void Dispose()
		{
			foreach (DirectionalLight light in _directionalLights)
			{
				light.Dispose();
			}
			foreach (PointLight light in _pointLights)
			{
				light.Dispose();
			}
		}
	}
}