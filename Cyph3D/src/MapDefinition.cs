namespace Cyph3D
{
	public class MapDefinition
	{
		public bool Compressed { get; }
		public bool sRGB { get; }
		public byte[] DefaultData { get; }

		public MapDefinition(bool compressed, bool sRGB, byte[] defaultData)
		{
			Compressed = compressed;
			this.sRGB = sRGB;
			DefaultData = defaultData;
		}
	}
}