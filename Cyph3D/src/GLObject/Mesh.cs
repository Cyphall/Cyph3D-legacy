using System;
using System.Collections.Generic;
using Assimp;
using Cyph3D.Enumerable;
using Cyph3D.Helper;
using Cyph3D.ResourceManagement;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using AssImpScene = Assimp.Scene;
using AssImpMesh = Assimp.Mesh;
using PrimitiveType = OpenToolkit.Graphics.OpenGL4.PrimitiveType;

namespace Cyph3D.GLObject
{
	public class Mesh : IDisposable
	{
		private VertexBuffer<VertexData> _vbo;
		private Buffer<int> _ibo;

		private VertexArray _vao;
		
		public string Name { get; }
		public bool IsResourceReady { get; private set; }

		public Mesh(string name)
		{
			Name = name;
			
			_vbo = new VertexBuffer<VertexData>(false, Stride.Get<VertexData>(1));
			_ibo = new Buffer<int>(false);

			_vao = new VertexArray();
			_vao.RegisterAttrib(_vbo, 0, 3, VertexAttribType.Float, nameof(VertexData.Position));
			_vao.RegisterAttrib(_vbo, 1, 2, VertexAttribType.Float, nameof(VertexData.UV));
			_vao.RegisterAttrib(_vbo, 2, 3, VertexAttribType.Float, nameof(VertexData.Normal));
			_vao.RegisterAttrib(_vbo, 3, 3, VertexAttribType.Float, nameof(VertexData.Tangent));
			_vao.RegisterIndexBuffer(_ibo);
		}

		public void FinalizeLoading(MeshFinalizationData data)
		{
			_vbo.PutData(data.VertexData);
			_ibo.PutData(data.Indices);

			IsResourceReady = true;
		}

		public void Render()
		{
			_vao.Bind();
			GL.DrawElements(PrimitiveType.Triangles, _ibo.Size, DrawElementsType.UnsignedInt, 0);
		}

		public void Dispose()
		{
			_vao.Dispose();
			_vbo.Dispose();
			
			IsResourceReady = false;
		}
		
		public static unsafe MeshFinalizationData LoadFromFile(string name)
		{
			AssImpScene scene = new AssimpContext().ImportFile($"resources/meshes/{name}.obj",
				PostProcessSteps.CalculateTangentSpace | PostProcessSteps.Triangulate);
			AssImpMesh mesh = scene.Meshes[0];

			List<VertexData> vertexData = new List<VertexData>();
			List<int> indices = new List<int>();

			for (int i = 0; i < mesh.FaceCount; i++)
			{
				indices.Add(mesh.Faces[i].Indices[0]);
				indices.Add(mesh.Faces[i].Indices[1]);
				indices.Add(mesh.Faces[i].Indices[2]);
			}
			
			for (int i = 0; i < mesh.VertexCount; i++)
			{
				VertexData vData = new VertexData();

				Vector3D v = mesh.Vertices[i];
				vData.Position = *(vec3*) &v;
				
				Vector3D t = mesh.TextureCoordinateChannels[0][i];
				vData.UV = (*(vec3*) &t).xy;
				
				Vector3D n = mesh.Normals[i];
				vData.Normal = *(vec3*) &n;

				Vector3D tan = mesh.Tangents[i];
				vData.Tangent = *(vec3*) &tan;
				
				vertexData.Add(vData);
			}

			return new MeshFinalizationData(
				vertexData.ToArray(),
				indices.ToArray()
			);
		}
	}
}