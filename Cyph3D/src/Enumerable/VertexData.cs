using System.Runtime.InteropServices;
using GlmSharp;

namespace Cyph3D.Enumerable
{
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexData
	{
		public vec3 Position;
		public vec2 UV;
		public vec3 Normal;
		public vec3 Tangent;
	}
}