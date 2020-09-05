using System;
using Cyph3D.Enumerable;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class Cubemap : BufferBase
	{
		private ivec2 _size;
		public ivec2 Size => _size;
		
		private long _bindlessHandle;
		public long BindlessHandle
		{
			get
			{
				if (!GL.Arb.IsTextureHandleResident(_bindlessHandle))
				{
					GL.Arb.MakeTextureHandleResident(_bindlessHandle);
				}

				return _bindlessHandle;
			}
		}
		
		public Cubemap(CubemapCreateInfo createInfo)
		{
			_size = createInfo.Size;
			
			GL.CreateTextures(TextureTarget.TextureCubeMap, 1, out _id);

			int finteringRaw = createInfo.Filtering switch
			{
				TextureFiltering.Linear => (int)All.Linear,
				TextureFiltering.Nearest => (int)All.Nearest,
				_ => throw new ArgumentOutOfRangeException(nameof(createInfo.Filtering), createInfo.Filtering, null)
			};
			
			GL.TextureParameter(_id, TextureParameterName.TextureMinFilter, finteringRaw);
			GL.TextureParameter(_id, TextureParameterName.TextureMagFilter, finteringRaw);
			GL.TextureParameter(_id, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
			GL.TextureParameter(_id, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
			GL.TextureParameter(_id, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);


			GL.TextureStorage2D(_id, 1, (SizedInternalFormat)createInfo.InternalFormat, _size.x, _size.y);
			
			_bindlessHandle = GL.Arb.GetTextureHandle(_id);
		}
		
		public void PutData(byte[] data, int face, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage3D(_id, 0, 0, 0, face, _size.x, _size.y, 1, format, type, data);
		}
		
		public void PutData(IntPtr data, int face, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage3D(_id, 0, 0, 0, face, _size.x, _size.y, 1, format, type, data);
		}

		protected override void DeleteBuffer()
		{
			GL.DeleteTexture(_id);
		}

		public void Bind(int unit)
		{
			GL.BindTextureUnit(unit, _id);
		}
	}
}