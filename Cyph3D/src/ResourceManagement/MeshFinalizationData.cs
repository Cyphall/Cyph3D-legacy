using Cyph3D.Enumerable;

namespace Cyph3D.ResourceManagement
{
	public class MeshFinalizationData
	{
		public VertexData[] VertexData { get; }
		public int[] Indices { get; }

		public MeshFinalizationData(VertexData[] vertexData, int[] indices)
		{
			VertexData = vertexData;
			Indices = indices;
		}
	}
}