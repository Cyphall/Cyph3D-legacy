using System;
using System.Collections.Generic;
using System.IO;
using GlmSharp;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;
using OpenToolkit.Graphics.OpenGL4;
using Renderer.Misc;
using OBJTexture = ObjLoader.Loader.Data.VertexData.Texture;

namespace Renderer.GLObject
{
	public class Mesh : IDisposable
	{
		private int _verticesBufferID;
		private int _uvsBufferID;
		private int _normalsBufferID;
		private int _tangentsBufferID;
		private int _bitangentsBufferID;

		private int _vaoID;

		private int _verticesCount;

		private static ObjLoaderFactory _loaderFactory;

		private static Dictionary<string, Mesh> _meshes = new Dictionary<string, Mesh>();

		private Mesh(string name)
		{
			_verticesBufferID = GL.GenBuffer();
			_uvsBufferID = GL.GenBuffer();
			_normalsBufferID = GL.GenBuffer();
			_tangentsBufferID = GL.GenBuffer();
			_bitangentsBufferID = GL.GenBuffer();

			_vaoID = GL.GenVertexArray();

			GL.BindVertexArray(_vaoID);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferID);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			GL.EnableVertexAttribArray(0);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _uvsBufferID);
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			GL.EnableVertexAttribArray(1);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _normalsBufferID);
			GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			GL.EnableVertexAttribArray(2);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _tangentsBufferID);
			GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			GL.EnableVertexAttribArray(3);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _bitangentsBufferID);
			GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			GL.EnableVertexAttribArray(4);

			GL.BindVertexArray(0);


			LoadResult rawMesh;
			using (FileStream stream = File.OpenRead($"resources/meshes/{name}.obj"))
			{
				rawMesh = _loaderFactory.Create().Load(stream);
			}

			_verticesCount = rawMesh.Groups[0].Faces.Count * 3;
			NativeArray<float> vertices = new NativeArray<float>(_verticesCount * 3);
			NativeArray<float> uvs = new NativeArray<float>(_verticesCount * 2);
			NativeArray<float> normals = new NativeArray<float>(_verticesCount * 3);
			NativeArray<float> tangents = new NativeArray<float>(_verticesCount * 3);
			NativeArray<float> bitangents = new NativeArray<float>(_verticesCount * 3);

			for (int i = 0; i < rawMesh.Groups[0].Faces.Count; i++)
			{
				Face f = rawMesh.Groups[0].Faces[i];


				Vertex v0 = rawMesh.Vertices[f[0].VertexIndex - 1];
				vertices[i * 9 + 0] = v0.X;
				vertices[i * 9 + 1] = v0.Y;
				vertices[i * 9 + 2] = v0.Z;

				Vertex v1 = rawMesh.Vertices[f[1].VertexIndex - 1];
				vertices[i * 9 + 3] = v1.X;
				vertices[i * 9 + 4] = v1.Y;
				vertices[i * 9 + 5] = v1.Z;

				Vertex v2 = rawMesh.Vertices[f[2].VertexIndex - 1];
				vertices[i * 9 + 6] = v2.X;
				vertices[i * 9 + 7] = v2.Y;
				vertices[i * 9 + 8] = v2.Z;


				OBJTexture t0 = rawMesh.Textures[f[0].TextureIndex - 1];
				uvs[i * 6 + 0] = t0.X;
				uvs[i * 6 + 1] = t0.Y;

				OBJTexture t1 = rawMesh.Textures[f[1].TextureIndex - 1];
				uvs[i * 6 + 2] = t1.X;
				uvs[i * 6 + 3] = t1.Y;

				OBJTexture t2 = rawMesh.Textures[f[2].TextureIndex - 1];
				uvs[i * 6 + 4] = t2.X;
				uvs[i * 6 + 5] = t2.Y;


				Normal n0 = rawMesh.Normals[f[0].NormalIndex - 1];
				normals[i * 9 + 0] = n0.X;
				normals[i * 9 + 1] = n0.Y;
				normals[i * 9 + 2] = n0.Z;

				Normal n1 = rawMesh.Normals[f[1].NormalIndex - 1];
				normals[i * 9 + 3] = n1.X;
				normals[i * 9 + 4] = n1.Y;
				normals[i * 9 + 5] = n1.Z;

				Normal n2 = rawMesh.Normals[f[2].NormalIndex - 1];
				normals[i * 9 + 6] = n2.X;
				normals[i * 9 + 7] = n2.Y;
				normals[i * 9 + 8] = n2.Z;

				vec3 deltaPos1 = new vec3(v1.X, v1.Y, v1.Z) - new vec3(v0.X, v0.Y, v0.Z);
				vec3 deltaPos2 = new vec3(v2.X, v2.Y, v2.Z) - new vec3(v0.X, v0.Y, v0.Z);

				vec2 deltaUV1 = new vec2(t1.X, t1.Y) - new vec2(t0.X, t0.Y);
				vec2 deltaUV2 = new vec2(t2.X, t2.Y) - new vec2(t0.X, t0.Y);

				float r = 1.0f / (deltaUV1.x * deltaUV2.y - deltaUV1.y * deltaUV2.x);
				vec3 tangent = ((deltaPos1 * deltaUV2.y - deltaPos2 * deltaUV1.y) * r).Normalized;
				vec3 bitangent = ((deltaPos2 * deltaUV1.x - deltaPos1 * deltaUV2.x) * r).Normalized;

				tangents[i * 9 + 0] = tangent.x;
				tangents[i * 9 + 1] = tangent.y;
				tangents[i * 9 + 2] = tangent.z;
				tangents[i * 9 + 3] = tangent.x;
				tangents[i * 9 + 4] = tangent.y;
				tangents[i * 9 + 5] = tangent.z;
				tangents[i * 9 + 6] = tangent.x;
				tangents[i * 9 + 7] = tangent.y;
				tangents[i * 9 + 8] = tangent.z;

				bitangents[i * 9 + 0] = bitangent.x;
				bitangents[i * 9 + 1] = bitangent.y;
				bitangents[i * 9 + 2] = bitangent.z;
				bitangents[i * 9 + 3] = bitangent.x;
				bitangents[i * 9 + 4] = bitangent.y;
				bitangents[i * 9 + 5] = bitangent.z;
				bitangents[i * 9 + 6] = bitangent.x;
				bitangents[i * 9 + 7] = bitangent.y;
				bitangents[i * 9 + 8] = bitangent.z;
			}

			GL.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferID);
			GL.BufferData(BufferTarget.ArrayBuffer, (int) (vertices.Count * sizeof(float)), vertices, BufferUsageHint.DynamicDraw);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _uvsBufferID);
			GL.BufferData(BufferTarget.ArrayBuffer, uvs.Count * sizeof(float), uvs, BufferUsageHint.DynamicDraw);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _normalsBufferID);
			GL.BufferData(BufferTarget.ArrayBuffer, normals.Count * sizeof(float), normals, BufferUsageHint.DynamicDraw);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _tangentsBufferID);
			GL.BufferData(BufferTarget.ArrayBuffer, tangents.Count * sizeof(float), tangents, BufferUsageHint.DynamicDraw);

			GL.BindBuffer(BufferTarget.ArrayBuffer, _bitangentsBufferID);
			GL.BufferData(BufferTarget.ArrayBuffer, bitangents.Count * sizeof(float), bitangents, BufferUsageHint.DynamicDraw);

			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


			vertices.Dispose();
			uvs.Dispose();
			normals.Dispose();
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
			GL.DeleteBuffers(5, new []
				{
					_verticesBufferID,
					_uvsBufferID,
					_normalsBufferID,
					_tangentsBufferID,
					_bitangentsBufferID
				}
			);
			_verticesBufferID = 0;
			_uvsBufferID = 0;
			_normalsBufferID = 0;
			_tangentsBufferID = 0;
			_bitangentsBufferID = 0;
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
	}
}