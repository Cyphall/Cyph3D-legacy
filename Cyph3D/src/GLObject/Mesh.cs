using System;
using System.Collections.Generic;
using System.IO;
using GlmSharp;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;
using OpenGL;
using Renderer.Misc;
using OBJTexture = ObjLoader.Loader.Data.VertexData.Texture;

namespace Renderer.GLObject
{
	public class Mesh : IDisposable
	{
		private uint _verticesBufferID;
		private uint _uvsBufferID;
		private uint _normalsBufferID;
		private uint _tangentsBufferID;
		private uint _bitangentsBufferID;

		private uint _vaoID;

		private int _verticesCount;
		
		private static Dictionary<string, Mesh> _meshes = new Dictionary<string, Mesh>();

		private Mesh(string name)
		{
			_verticesBufferID = Gl.GenBuffer();
			_uvsBufferID = Gl.GenBuffer();
			_normalsBufferID = Gl.GenBuffer();
			_tangentsBufferID = Gl.GenBuffer();
			_bitangentsBufferID = Gl.GenBuffer();

			_vaoID = Gl.GenVertexArray();

			Gl.BindVertexArray(_vaoID);

				Gl.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferID);
					Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
					Gl.EnableVertexAttribArray(0);

				Gl.BindBuffer(BufferTarget.ArrayBuffer, _uvsBufferID);
					Gl.VertexAttribPointer(1, 2, VertexAttribType.Float, false, 0, IntPtr.Zero);
					Gl.EnableVertexAttribArray(1);

				Gl.BindBuffer(BufferTarget.ArrayBuffer, _normalsBufferID);
					Gl.VertexAttribPointer(2, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
					Gl.EnableVertexAttribArray(2);

				Gl.BindBuffer(BufferTarget.ArrayBuffer, _tangentsBufferID);
					Gl.VertexAttribPointer(3, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
					Gl.EnableVertexAttribArray(3);
				
				Gl.BindBuffer(BufferTarget.ArrayBuffer, _bitangentsBufferID);
					Gl.VertexAttribPointer(4, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
					Gl.EnableVertexAttribArray(4);

			Gl.BindVertexArray(0);
			
			
			LoadResult rawMesh;
			using (FileStream stream = File.OpenRead($"resources/meshes/{name}.obj"))
			{
				rawMesh = new ObjLoaderFactory().Create().Load(stream);
			}

			_verticesCount = rawMesh.Groups[0].Faces.Count * 3;
			NativeArray<float> vertices = new NativeArray<float>(_verticesCount*3);
			NativeArray<float> uvs = new NativeArray<float>(_verticesCount*2);
			NativeArray<float> normals = new NativeArray<float>(_verticesCount*3);
			NativeArray<float> tangents = new NativeArray<float>(_verticesCount*3);
			NativeArray<float> bitangents = new NativeArray<float>(_verticesCount*3);

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
				vec3 tangent = ((deltaPos1 * deltaUV2.y   - deltaPos2 * deltaUV1.y)*r).Normalized;
				vec3 bitangent = ((deltaPos2 * deltaUV1.x   - deltaPos1 * deltaUV2.x)*r).Normalized;

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
			
			Gl.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices, BufferUsage.DynamicDraw);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _uvsBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, uvs.Count * sizeof(float), uvs, BufferUsage.DynamicDraw);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _normalsBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, normals.Count * sizeof(float), normals, BufferUsage.DynamicDraw);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _tangentsBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, tangents.Count * sizeof(float), tangents, BufferUsage.DynamicDraw);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _bitangentsBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, bitangents.Count * sizeof(float), bitangents, BufferUsage.DynamicDraw);
			
			Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);


			vertices.Dispose();
			uvs.Dispose();
			normals.Dispose();
		}
		
		public static Mesh Get(string name)
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
			Gl.BindVertexArray(_vaoID);
				Gl.DrawArrays(PrimitiveType.Triangles, 0, _verticesCount);
		}

		public void Dispose()
		{
			Gl.DeleteBuffers(
				_verticesBufferID,
				_uvsBufferID,
				_normalsBufferID,
				_tangentsBufferID,
				_bitangentsBufferID
				);
		}
		
		public static void DisposeAll()
		{
			foreach (Mesh mesh in _meshes.Values)
			{
				mesh.Dispose();
			}
		}
	}
}