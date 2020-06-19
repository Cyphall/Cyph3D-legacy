using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Cyph3D.Extension;
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
		private int _uvsBufferID;
		private int _normalsBufferID;
		private int _tangentsBufferID;
		private int _bitangentsBufferID;

		private int _vaoID;

		private int _verticesCount;
		
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

			_verticesCount = rawMesh.Groups[0].Faces.Count * 3;
			NativeArray<VertexData> vertexData = new NativeArray<VertexData>(_verticesCount * 3);

			for (int i = 0; i < rawMesh.Groups[0].Faces.Count; i++)
			{
				Face f = rawMesh.Groups[0].Faces[i];

				VertexData vData1 = new VertexData();
				VertexData vData2 = new VertexData();
				VertexData vData3 = new VertexData();

				Vertex v0 = rawMesh.Vertices[f[0].VertexIndex - 1];
				vData1.Position = *(vec3*) &v0;

				Vertex v1 = rawMesh.Vertices[f[1].VertexIndex - 1];
				vData2.Position = *(vec3*) &v1;

				Vertex v2 = rawMesh.Vertices[f[2].VertexIndex - 1];
				vData3.Position = *(vec3*) &v2;


				OBJTexture t0 = rawMesh.Textures[f[0].TextureIndex - 1];
				vData1.UV = *(vec2*) &t0;

				OBJTexture t1 = rawMesh.Textures[f[1].TextureIndex - 1];
				vData2.UV = *(vec2*) &t1;

				OBJTexture t2 = rawMesh.Textures[f[2].TextureIndex - 1];
				vData3.UV = *(vec2*) &t2;


				Normal n0 = rawMesh.Normals[f[0].NormalIndex - 1];
				vData1.Normal = *(vec3*) &n0;

				Normal n1 = rawMesh.Normals[f[1].NormalIndex - 1];
				vData2.Normal = *(vec3*) &n1;

				Normal n2 = rawMesh.Normals[f[2].NormalIndex - 1];
				vData3.Normal = *(vec3*) &n2;

				vec3 deltaPos1 = new vec3(v1.X, v1.Y, v1.Z) - new vec3(v0.X, v0.Y, v0.Z);
				vec3 deltaPos2 = new vec3(v2.X, v2.Y, v2.Z) - new vec3(v0.X, v0.Y, v0.Z);

				vec2 deltaUV1 = new vec2(t1.X, t1.Y) - new vec2(t0.X, t0.Y);
				vec2 deltaUV2 = new vec2(t2.X, t2.Y) - new vec2(t0.X, t0.Y);

				float r = 1.0f / (deltaUV1.x * deltaUV2.y - deltaUV1.y * deltaUV2.x);
				vec3 tangent = ((deltaPos1 * deltaUV2.y - deltaPos2 * deltaUV1.y) * r).Normalized;

				vData1.Tangent = tangent;
				vData2.Tangent = tangent;
				vData3.Tangent = tangent;

				vertexData[i * 3 + 0] = vData1;
				vertexData[i * 3 + 1] = vData2;
				vertexData[i * 3 + 2] = vData3;
			}

			GL.BindBuffer(BufferTarget.ArrayBuffer, _verticesDataBufferID);
			
			GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Count * sizeof(VertexData), vertexData, BufferUsageHint.DynamicDraw);
			
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


			vertexData.Dispose();
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
			GL.DrawArrays(PrimitiveType.Triangles, 0, _verticesCount);
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