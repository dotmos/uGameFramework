using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MeshTools{
	public partial class Modifier
	{
		//Smoothen a mesh using lapacian smoothing
		//smoothness: range 0-1
		public static void SmoothMeshLaplacian(Mesh mesh, float smoothness = 1)
		{
			Vector3[] vertices = mesh.vertices;
			int[] triangles = mesh.triangles;

			Vector3[] smoothVertices = new Vector3[vertices.Length];
			List<Vector3> adjacentVertices = new List<Vector3>();
			
//			float dx = 0.0f;
//			float dy = 0.0f;
//			float dz = 0.0f;
			Vector3 smoothVertex;

			Helper.VertexConnection[] sharedPositions = Helper.GetSharedVertices(vertices);
			
			for (int i=0; i<vertices.Length; ++i)
			{
				// Find the sv neighboring vertices
				adjacentVertices = Helper.FindAdjacentVertexNeighbours (vertices, triangles, vertices[i], sharedPositions);
				
				if (adjacentVertices.Count != 0)
				{
					smoothVertex = Vector3.zero;
					
					// Add the vertices and divide by the number of vertices
					for (int j=0; j<adjacentVertices.Count; ++j)
					{
						smoothVertex += adjacentVertices[j];
					}

					smoothVertices[i] = Vector3.Lerp(vertices[i], smoothVertex * (1.0f / adjacentVertices.Count), smoothness);
				}
			}
			
			mesh.vertices = smoothVertices;
		}

	}
}
