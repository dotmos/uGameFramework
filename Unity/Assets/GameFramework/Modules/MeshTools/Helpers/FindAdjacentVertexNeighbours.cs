using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Lapacian Smooth taken from http://wiki.unity3d.com/index.php/MeshSmoother (MarkGX, Jan 2011) and optimized by Wolfgang "dotmos" Reichardt (2x-4x faster depending on meshdata).

namespace MeshTools{
	public partial class Helper{

//		private class AdjacentNeightboursHelper{
//			// Does the vertex v exist in the list of vertices
//			public static bool isVertexExist(List<Vector3>adjacentV, Vector3 v)
//			{
//				bool marker = false;
//				foreach (Vector3 vec in adjacentV)
//					if (Mathf.Approximately(vec.x,v.x) && Mathf.Approximately(vec.y,v.y) && Mathf.Approximately(vec.z,v.z))
//					{
//						marker = true;
//						break;
//					}
//				
//				return marker;
//			}
//		}

		//Finds adjacent neighbours of the supplied vertex.
		//sharedPositions can be filled by Helper.GetSharedVertices() if the mesh has seams/gaps
		public static List<Vector3> FindAdjacentVertexNeighbours ( Vector3[] vertices, int[] triangles, Vector3 vertex, VertexConnection[] sharedPositions = null )
		{
			List<Vector3> adjacentV = new List<Vector3>();
			HashSet<int> facemarker = new HashSet<int>();
			int facecount = 0;	

			if(sharedPositions == null)
				sharedPositions = new VertexConnection[vertices.Length];

			//caches
			int v1 = 0;
			int v2 = 0;
			bool marker = false;
			int tk, tk1, tk2;
			VertexConnection sharedPosition;

			// Find matching vertices
			for (int i=0; i<vertices.Length; ++i)
				if( Mathf.Approximately( Vector3.Distance(vertex, vertices[i]), 0) )
				{
//					v1 = 0;
//					v2 = 0;
//					marker = false;
					
					// Find vertex indices from the triangle array
					for(int k=0; k<triangles.Length; k=k+3)
						if(facemarker.Contains(k) == false)
					{
						v1 = 0;
						v2 = 0;
						marker = false;
						
						tk = triangles[k];
						tk1 = triangles[k+1];
						tk2 = triangles[k+2];

						if(i == tk)
						{
							v1 = tk1;
							v2 = tk2;
							marker = true;
						}
						
						if(i == tk1)
						{
							v1 = tk;
							v2 = tk2;
							marker = true;
						}
						
						if(i == tk2)
						{
							v1 = tk;
							v2 = tk1;
							marker = true;
						}
						
						facecount++;
						if(marker)
						{
							// Once face has been used mark it so it does not get used again
							facemarker.Add(k);
							
	//						// Add non duplicate vertices to the list
	//						if (AdjacentNeightboursHelper.isVertexExist(adjacentV, v[v1]) == false )
	//						{	
	//							adjacentV.Add(v[v1]);
	//							//Debug.Log("Adjacent vertex index = " + v1);
	//						}
	//						
	//						if ( AdjacentNeightboursHelper.isVertexExist(adjacentV, v[v2]) == false )
	//						{
	//							adjacentV.Add(v[v2]);
	//							//Debug.Log("Adjacent vertex index = " + v2);
	//						}
							
							// Add non duplicate vertices to the list
							// This is faster than the above code
							sharedPosition = sharedPositions[v1];
							if(sharedPosition == null)
							{
								adjacentV.Add(vertices[v1]);
							} else{
								for(int sv=0; sv<sharedPosition.connections.Count; ++sv)
								{
									adjacentV.Add(vertices[ sharedPosition.connections[sv] ]);
								}
							}
							sharedPosition = sharedPositions[v2];
							if(sharedPosition == null)
							{
								adjacentV.Add(vertices[v2]);
							} else{
								for(int sv=0; sv<sharedPosition.connections.Count; ++sv)
								{
									adjacentV.Add(vertices[ sharedPosition.connections[sv] ]);
								}
							}

							marker = false;
						}
					}
				}
			
			//Debug.Log("Faces Found = " + facecount);
			
			return adjacentV;
		}

	}
}
