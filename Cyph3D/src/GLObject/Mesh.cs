using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assimp;
using GlmSharp;
using OpenToolkit.Graphics.OpenGL4;
using AssImpScene = Assimp.Scene;
using AssImpMesh = Assimp.Mesh;
using PrimitiveType = OpenToolkit.Graphics.OpenGL4.PrimitiveType;

namespace Cyph3D.GLObject
{
	public class Mesh : IDisposable
	{
		private int _verticesDataBufferID;

		private int _vaoID;

		private int[] _indices;
		
		public string Name { get; private set; }

		public unsafe Mesh()
		{
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
		}

		public unsafe void LoadFromFile(string name)
		{
			Name = name;
			
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

			GL.BindBuffer(BufferTarget.ArrayBuffer, _verticesDataBufferID);
			
			GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Count * sizeof(VertexData), vertexData.ToArray(), BufferUsageHint.DynamicDraw);
			
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

			_indices = indices.ToArray();
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