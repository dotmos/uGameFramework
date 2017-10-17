using UnityEngine;
using System.Collections;

namespace MeshTools{
public partial class Modifier{

	//Spherify a mesh
	public static void Spherify(Mesh mesh, float radius)
	{
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = new Vector3[vertices.Length];
		Vector3 normal;
		for(int i=0; i<vertices.Length; ++i)
		{
			normal = vertices[i].normalized;
			normals[i] = normal;
			vertices[i] = normal * radius;
		}
		mesh.vertices = vertices;
		mesh.normals = normals;
	}
}
}