using UnityEngine;
using System.Collections;

namespace MeshTools{
	public partial class Modifier
	{
		//Adds noise to the mesh, based on vertex positions
		public static void Noise(Mesh mesh, System.Func<float,float,float,float> noiseFunction, float frequency = 1, float amplitude = 1, bool onlyPositive = false, Vector3 offset = default(Vector3), bool invertNoise = false)
		{
			Vector3[] vertices = mesh.vertices;
			Vector3[] normals = mesh.normals;
			Vector3 vertex;
			if(!invertNoise)
			{
				if(onlyPositive)
				{
					for(int i=0; i<vertices.Length; ++i)
					{
						vertex = vertices[i];
						vertices[i] += normals[i] * noiseFunction(vertex.x*frequency + offset.x, vertex.y*frequency + offset.y, vertex.z*frequency + offset.z) * amplitude;
					}
				}
				else
				{
					for(int i=0; i<vertices.Length; ++i)
					{
						vertex = vertices[i];
						vertices[i] += normals[i] * (noiseFunction(vertex.x*frequency + offset.x, vertex.y*frequency + offset.y, vertex.z*frequency + offset.z)*2-1) * amplitude;
					}
				}
			}
			else
			{
				if(onlyPositive)
				{
					for(int i=0; i<vertices.Length; ++i)
					{
						vertex = vertices[i];
						vertices[i] += normals[i] * (1-noiseFunction(vertex.x*frequency + offset.x, vertex.y*frequency + offset.y, vertex.z*frequency + offset.z)) * amplitude;
					}
				}
				else
				{
					for(int i=0; i<vertices.Length; ++i)
					{
						vertex = vertices[i];
						vertices[i] += normals[i] * ((1-noiseFunction(vertex.x*frequency + offset.x, vertex.y*frequency + offset.y, vertex.z*frequency + offset.z))*2-1) * amplitude;
					}
				}
			}
			
			mesh.vertices = vertices;
		}

		//Adds noise to the mesh, based on vertex positions
		public static void Noise2D(Mesh mesh, System.Func<float,float,float,float> noiseFunction, float frequency = 1, float amplitude = 1, bool onlyPositive = false, Vector3 offset = default(Vector3), bool invertNoise = false)
		{
			Vector3[] vertices = mesh.vertices;
			Vector3[] normals = mesh.normals;
			Vector3 vertex;
			if(!invertNoise)
			{
				if(onlyPositive)
				{
					for(int i=0; i<vertices.Length; ++i)
					{
						vertex = vertices[i];
						vertices[i] += normals[i] * noiseFunction(vertex.x*frequency + offset.x, vertex.y*frequency + offset.y, offset.z) * amplitude;
					}
				}
				else
				{
					for(int i=0; i<vertices.Length; ++i)
					{
						vertex = vertices[i];
						vertices[i] += normals[i] * (noiseFunction(vertex.x*frequency + offset.x, vertex.y*frequency + offset.y, offset.z)*2-1) * amplitude;
					}
				}
			}
			else
			{
				if(onlyPositive)
				{
					for(int i=0; i<vertices.Length; ++i)
					{
						vertex = vertices[i];
						vertices[i] += normals[i] * (1-noiseFunction(vertex.x*frequency + offset.x, vertex.y*frequency + offset.y, offset.z)) * amplitude;
					}
				}
				else
				{
					for(int i=0; i<vertices.Length; ++i)
					{
						vertex = vertices[i];
						vertices[i] += normals[i] * ((1-noiseFunction(vertex.x*frequency + offset.x, vertex.y*frequency + offset.y, offset.z))*2-1) * amplitude;
					}
				}
			}
			
			mesh.vertices = vertices;
		}

	}
}
