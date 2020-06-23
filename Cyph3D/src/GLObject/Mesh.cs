using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cyph3D.Misc;
using GlmSharp;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;
using OpenToolkit.Graphics.OpenGL4;
using OBJTexture = ObjLoader.Loader.Data.VertexData.Texture;

namespace Cyph3D.GLObject
{
	public class Mesh : IDisposable
	{
		private int _verticesDataBufferID;

		private int _vaoID;

		private int[] _indices;
		
		public string Name { get; }

		private static ObjLoaderFactory _loaderFactory;

		private static Dictionary<string, Mesh> _meshes = new Dictionary<string, Mesh>();

		private unsafe Mesh(string name)
		{
			Name = name;
			
			_verticesDataBufferID = GL.GenBuffer();

			_vaoID = GL.GenVertexArray();

			GL.BindVertexArray(_vaoID);

				GL.BindBuffer(BufferTarget.ArrayBuffer, _verticesDataBufferID);

				GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(VertexData), Marshal.OffsetOf<VertexData>(nameof(VertexData.Position)));
				GL.EnableVertexAttribArray(0);

				GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(VertexData), Marshal.OffsetOf<VertexData>(nameof(VertexData.UV)));
				GL.EnableVertexAttribArray(1);

				GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, sizeof(VertexData), Marshal.OffsetOf<VertexData>(nameof(VertexData.Normal)));
				GL.EnableVertexAttribArray(2);

				GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, sizeof(VertexData), Marshal.OffsetOf<VertexData>(nameof(VertexData.Tangent)));
				GL.EnableVertexAttribArray(3);
				
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

			GL.BindVertexArray(0);


			LoadResult rawMesh;
			using (FileStream stream = File.OpenRead($"resources/meshes/{name}.obj"))
			{
				rawMesh = _loaderFactory.Create().Load(stream);
			}
			
			Dictionary<ivec3, int> indexMapping = new Dictionary<ivec3, int>();

			List<VertexData> vertexData = new List<VertexData>();
			List<int> indices = new List<int>();

			for (int i = 0; i < rawMesh.Groups[0].Faces.Count; i++)
			{
				Face f = rawMesh.Groups[0].Faces[i];
				
				indices.Add(GetIndex(f, 0));
				indices.Add(GetIndex(f, 1));
				indices.Add(GetIndex(f, 2));
			}

			GL.BindBuffer(BufferTarget.ArrayBuffer, _verticesDataBufferID);
			
			GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Count * sizeof(VertexData), vertexData.ToArray(), BufferUsageHint.DynamicDraw);
			
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

			_indices = indices.ToArray();

			int GetIndex(Face face, int index)
			{
				ivec3 comboIndex = new ivec3(face[index].VertexIndex - 1, face[index].TextureIndex - 1, face[index].NormalIndex - 1);
				
				if (!indexMapping.Keys.Contains(comboIndex))
				{
					VertexData vData = new VertexData();
					

					Vertex v = rawMesh.Vertices[comboIndex[0]];
					vData.Position = *(vec3*) &v;
					Vertex v0 = rawMesh.Vertices[face[0].VertexIndex - 1];
					Vertex v1 = rawMesh.Vertices[face[1].VertexIndex - 1];
					Vertex v2 = rawMesh.Vertices[face[2].VertexIndex - 1];


					OBJTexture t = rawMesh.Textures[comboIndex[1]];
					vData.UV = *(vec2*) &t;
					OBJTexture t0 = rawMesh.Textures[face[0].TextureIndex - 1];
					OBJTexture t1 = rawMesh.Textures[face[1].TextureIndex - 1];
					OBJTexture t2 = rawMesh.Textures[face[2].TextureIndex - 1];
					
					
					Normal n = rawMesh.Normals[comboIndex[2]];
					vData.Normal = (*(vec3*) &n).Normalized;
					

					vec3 deltaPos1 = new vec3(v1.X, v1.Y, v1.Z) - new vec3(v0.X, v0.Y, v0.Z);
					vec3 deltaPos2 = new vec3(v2.X, v2.Y, v2.Z) - new vec3(v0.X, v0.Y, v0.Z);

					vec2 deltaUV1 = new vec2(t1.X, t1.Y) - new vec2(t0.X, t0.Y);
					vec2 deltaUV2 = new vec2(t2.X, t2.Y) - new vec2(t0.X, t0.Y);

					float r = 1.0f / (deltaUV1.x * deltaUV2.y - deltaUV1.y * deltaUV2.x);
					vec3 tangent = ((deltaPos1 * deltaUV2.y - deltaPos2 * deltaUV1.y) * r).Normalized;

					vData.Tangent = (tangent - vec3.Dot(vData.Normal, tangent) * vData.Normal).Normalized;
					
					
					vertexData.Add(vData);
					indexMapping.Add(comboIndex, vertexData.Count - 1);
				}

				return indexMapping[comboIndex];
			}
		}

		public static Mesh GetOrLoad(string name)
		{
			if (!_meshes.ContainsKey(name))
			{
				Logger.Info($"Loading mesh \"{name}\"");
				_meshes.Add(name, new Mesh(name));
				Logger.Info($"Mesh \"{name}\" loaded");
			}

			return _meshes[name];
		}

		public void Render()
		{
			GL.BindVertexArray(_vaoID);
			GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, _indices);
		}

		public void Dispose()
		{
			GL.DeleteBuffer(_verticesDataBufferID);
			_verticesDataBufferID = 0;
		}

		public static void DisposeAll()
		{
			foreach (Mesh mesh in _meshes.Values)
			{
				mesh.Dispose();
			}
		}

		static Mesh()
		{
			_loaderFactory = new ObjLoaderFactory();
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct VertexData
		{
			public vec3 Position;
			public vec2 UV;
			public vec3 Normal;
			public vec3 Tangent;
		}
	}
}