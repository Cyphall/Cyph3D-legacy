using System;
using Cyph3D.Enumerable;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;

namespace Cyph3D.GLObject
{
	public class Cubemap : IDisposable
	{
		private int _ID;
		private ivec2 _size;
		
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
		
		public static implicit operator int(Cubemap cubemap) => cubemap._ID;
		
		public Cubemap(CubemapSetting setting)
		{
			_size = setting.Size;
			
			GL.CreateTextures(TextureTarget.TextureCubeMap, 1, out _ID);

			int finteringRaw = setting.Filtering switch
			{
				TextureFiltering.Linear => (int)All.Linear,
				TextureFiltering.Nearest => (int)All.Nearest,
				_ => throw new ArgumentOutOfRangeException(nameof(setting.Filtering), setting.Filtering, null)
			};
			
			GL.TextureParameter(_ID, TextureParameterName.TextureMinFilter, finteringRaw);
			GL.TextureParameter(_ID, TextureParameterName.TextureMagFilter, finteringRaw);
			GL.TextureParameter(_ID, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
			GL.TextureParameter(_ID, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
			GL.TextureParameter(_ID, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);


			GL.TextureStorage2D(_ID, 1, (SizedInternalFormat)setting.InternalFormat, _size.x, _size.y);
			
			_bindlessHandle = GL.Arb.GetTextureHandle(_ID);
		}
		
		public void PutData(byte[] data, int face, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage3D(_ID, 0, 0, 0, face, _size.x, _size.y, 1, format, type, data);
		}
		
		public void PutData(IntPtr data, int face, PixelFormat format = PixelFormat.Rgb, PixelType type = PixelType.UnsignedByte)
		{
			GL.TextureSubImage3D(_ID, 0, 0, 0, face, _size.x, _size.y, 1, format, type, data);
		}
		
		public void Dispose()
		{
			GL.DeleteTexture(_ID);
			_ID = 0;
		}
		
		public void Bind(int unit)
		{
			GL.BindTextureUnit(unit, _ID);
		}
	}
}