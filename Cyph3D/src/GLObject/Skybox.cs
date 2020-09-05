using System;
using System.IO;
using Cyph3D.Enumerable;
using Cyph3D.ResourceManagement;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using StbImageNET;

namespace Cyph3D.GLObject
{
	public class Skybox : IDisposable
	{
		public string Name { get; set; }
		public float Rotation { get; set; }
		public Cubemap ResourceData { get; private set; }
		public bool IsResourceReady { get; private set; }
		
		public Skybox(string name)
		{
			Name = name;
		}

		public void FinalizeLoading(SkyboxFinalizationData data)
		{
			CubemapCreateInfo createInfo = new CubemapCreateInfo
			{
				Size = data.Size,
				InternalFormat = data.InternalFormat,
				Filtering = TextureFiltering.Linear
			};
			
			ResourceData = new Cubemap(createInfo);
			
			for(int i = 0; i < 6; i++)
			{
				ResourceData.PutData(data.CubemapData[i], i, data.PixelFormat);
			}

			IsResourceReady = true;
		}

		public void Dispose()
		{
			ResourceData?.Dispose();
			ResourceData = null;
			IsResourceReady = false;
		}

		public static SkyboxFinalizationData LoadFromFiles(string[] facesPath)
		{
			// 0 = positive x face
	        // 1 = negative x face
	        // 2 = positive y face
	        // 3 = negative y face
	        // 4 = positive z face
	        // 5 = negative z face
	        
	        byte[][] cubemapData = new byte[6][];
	        
	        ivec2 size = new ivec2();
	        Components comp = Components.Grey;

	        bool firstIteration = true;
	        for (int i = 0; i < 6; i++)
	        {
	            ivec2 faceSize = new ivec2();
	            Components faceComp;
	            try
	            {
            		cubemapData[i] = StbImage.Load(facesPath[i], out faceSize.x, out faceSize.y, out faceComp);
	            }
	            catch (IOException)
	            {
            		throw new IOException($"Unable to load image {facesPath[i]} from disk");
	            }

	            if (firstIteration)
	            {
            		size = faceSize;
            		comp = faceComp;
            		firstIteration = false;
            		continue;
	            }
	            
	            if (faceSize != size || faceComp != comp)
	            {
            		throw new InvalidDataException("Skybox cannot have images with different formats");
	            }
	        }
	        
	        InternalFormat internalFormat;
	        PixelFormat pixelFormat;
	        switch (comp)
	        {
	            case Components.Grey:
            		pixelFormat = PixelFormat.Luminance;
            		internalFormat = InternalFormat.CompressedSrgbS3tcDxt1Ext;
            		break;
	            case Components.GreyAlpha:
            		pixelFormat = PixelFormat.LuminanceAlpha;
            		internalFormat = InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
            		break;
	            case Components.RedGreenBlue:
            		pixelFormat = PixelFormat.Rgb;
            		internalFormat = InternalFormat.CompressedSrgbS3tcDxt1Ext;
            		break;
	            case Components.RedGreenBlueAlpha:
            		pixelFormat = PixelFormat.Rgba;
            		internalFormat = InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
            		break;
	            default:
            		throw new NotSupportedException($"The colors format {comp} is not supported");
	        }

	        return new SkyboxFinalizationData(
	            size,
	            internalFormat,
	            pixelFormat,
	            cubemapData
	        );
		}
	}
}