using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MeshTools{
	public partial class Modifier{

//		//Create new normals based on hardness 
//		public static void Normals(Mesh mesh, float hardness)
//		{
//			int[] triangles = mesh.triangles;
//			Vector3[] vertices = mesh.vertices;
//			Vector3[] normals = new Vector3[vertices.Length];
//
//			//caches
//			Vector3 normal;
//			Vector3 vertex;
//			int vi0, vi1, vi2; //vertex indices
//			vi0 = vi1 = vi2 = 0;
//
//			//generate normals
//			for(int i=0; i<triangles.Length; i+=3)
//			{
//				vi0 = triangles[i];
//				vi1 = triangles[i+1];
//				vi2 = triangles[i+2];
//				vertex = vertices[vi0]; //cache double used vertex for faster access
//				normal = Vector3.Cross(vertices[vi1] - vertex, vertices[vi2] - vertex); //normal vector
//				normal /= normal.magnitude; //fast normalize
//				normals[vi0] += normal;
//				normals[vi1] += normal;
//				normals[vi2] += normal;
//			}
//
//			//Normalize normals
//			for(int i=0; i<normals.Length; ++i)
//			{
//				normals[i] = normals[i].normalized;
//			}
//
//			//Assign normals
//			mesh.normals = normals;
//		}
	   

		//Creates smooth normals for non-watertight meshes or meshes with seams. i.e. a "hard" cube gets smooth normals.
		//Ignores smoothing groups
		public static void SmoothNormals(Mesh mesh, float hardness)
		{
			Vector3[] vertices = mesh.vertices;
			Helper.VertexConnection[] sharedVertices = Helper.GetSharedVertices(vertices);

			int[] triangles = mesh.triangles;
			Vector3[] normals = new Vector3[vertices.Length];
			
			//caches
			Vector3 normal;
			Vector3 vertex;
			Helper.VertexConnection sharedVertex;
			int vi0, vi1, vi2; //vertex indices
			vi0 = vi1 = vi2 = 0;
			
			//generate normals
			for(int i=0; i<triangles.Length; i+=3)
			{
				vi0 = triangles[i];
				vi1 = triangles[i+1];
				vi2 = triangles[i+2];
				vertex = vertices[vi0]; //cache double used vertex for faster access
				normal = Vector3.Cross(vertices[vi1] - vertex, vertices[vi2] - vertex); //normal vector
				normal /= normal.magnitude; //fast normalize
				normals[vi0] += normal;
				normals[vi1] += normal;
				normals[vi2] += normal;
				
				//Assign normals to shared vertices
				sharedVertex = sharedVertices[vi0];
				if(sharedVertex != null)
				{
					for(int sv=0; sv<sharedVertex.connections.Count; ++sv)
					{
						normals[sharedVertex.connections[sv]] += normal;
					}
				}
				sharedVertex = sharedVertices[vi1];
				if(sharedVertex != null)
				{
					for(int sv=0; sv<sharedVertex.connections.Count; ++sv)
					{
						normals[sharedVertex.connections[sv]] += normal;
					}
				}
				sharedVertex = sharedVertices[vi2];
				if(sharedVertex != null)
				{
					for(int sv=0; sv<sharedVertex.connections.Count; ++sv)
					{
						normals[sharedVertex.connections[sv]] += normal;
					}
				}
			}
			
			//Normalize normals
			for(int i=0; i<normals.Length; ++i)
			{
				normals[i] = normals[i].normalized;
			}
			
			//Assign new normals
			mesh.normals = normals;
		}


		//Fix normal seams between mesh seams. i.e. vertices that share the same position but are not connected.
		//For generating overall smooth normals, use SmoothNormals() instead!
		public static void FixNormalsSeam(Mesh mesh)
		{
			//Get vertices with same position
			Vector3[] vertices = mesh.vertices;
			Helper.VertexConnection[] connections = Helper.GetSharedVertices(vertices);

			//Create interpolated normals for all vertices with same position, removing hard edges/seams
			Vector3[] normals = mesh.normals;
			Helper.VertexConnection vConnection;
			Vector3 normal;
			for(int i=0; i<connections.Length; ++i)
			{
				vConnection = connections[i];
				if(vConnection == null)
					continue;

				normal = Vector3.zero;
				for(int j=0; j<vConnection.connections.Count; ++j)
				{
					normal += normals[vConnection.connections[j]];
				}
				normals[i] = normal.normalized;
			}

			//Assign new normals to mesh
			mesh.normals = normals;
        }
	}
}
