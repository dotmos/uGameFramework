using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MeshTools{
	public partial class Generator{

		public class CubeData{
			//Cube with smooth edges
			public static readonly Vector3[] vertices = new Vector3[]{
				//Top plane
				new Vector3(0.5f,0.5f,0.5f), //0
				new Vector3(0.5f,0.5f,-0.5f), //1
				new Vector3(-0.5f,0.5f,-0.5f), //2
				new Vector3(-0.5f,0.5f,0.5f), //3
				
				//Bottom plane
				new Vector3(0.5f,-0.5f,0.5f), //4
				new Vector3(0.5f,-0.5f,-0.5f), //5
				new Vector3(-0.5f,-0.5f,-0.5f), //6
				new Vector3(-0.5f,-0.5f,0.5f), //7
			};
			public static readonly int[] triangles = new int[]{
				/*
				3----0
				|\   |\
				| 7- |-4
				2----1 |
				 \|   \|
				  6----5
		       */
				
				//Top
				0,1,2, 2,3,0,
				//Bottom
				4,7,6, 6,5,4,
				//Front
				1,5,6, 6,2,1,
				//Back
				0,3,7, 7,4,0,
				//Right
				0,4,5, 5,1,0,
				//Left
				3,2,6, 6,7,3
			};
			public static readonly Vector2[] uvs = new Vector2[]{
				new Vector2(1,0),
				new Vector2(1,1),
				new Vector2(0,1),
				new Vector2(0,0),

				new Vector2(1,0),
				new Vector2(1,1),
				new Vector2(0,1),
				new Vector2(0,0),
			};


			//Cube with Hard edges
			public static readonly Vector3[] verticesHard = new Vector3[]{
				//Top plane
				new Vector3(0.5f,0.5f,0.5f), //0
				new Vector3(0.5f,0.5f,0.5f), //1
				new Vector3(0.5f,0.5f,0.5f), //2

				new Vector3(0.5f,0.5f,-0.5f), //3
				new Vector3(0.5f,0.5f,-0.5f), //4
				new Vector3(0.5f,0.5f,-0.5f), //5

				new Vector3(-0.5f,0.5f,-0.5f), //6
				new Vector3(-0.5f,0.5f,-0.5f), //7
				new Vector3(-0.5f,0.5f,-0.5f), //8

				new Vector3(-0.5f,0.5f,0.5f), //9
				new Vector3(-0.5f,0.5f,0.5f), //10
				new Vector3(-0.5f,0.5f,0.5f), //11

				//Bottom plane
				new Vector3(0.5f,-0.5f,0.5f), //12
				new Vector3(0.5f,-0.5f,0.5f), //13
				new Vector3(0.5f,-0.5f,0.5f), //14

				new Vector3(0.5f,-0.5f,-0.5f), //15
				new Vector3(0.5f,-0.5f,-0.5f), //16
				new Vector3(0.5f,-0.5f,-0.5f), //17

				new Vector3(-0.5f,-0.5f,-0.5f), //18
				new Vector3(-0.5f,-0.5f,-0.5f), //19
				new Vector3(-0.5f,-0.5f,-0.5f), //20

				new Vector3(-0.5f,-0.5f,0.5f), //21
				new Vector3(-0.5f,-0.5f,0.5f), //22
				new Vector3(-0.5f,-0.5f,0.5f), //23
			};
			public static readonly Vector2[] uvsHard = new Vector2[]{
				new Vector2(1,0),
				new Vector2(1,0),
				new Vector2(1,0),

				new Vector2(1,1), //3
				new Vector2(1,0),
				new Vector2(0,0),

				new Vector2(0,1), //6
				new Vector2(0,0),
				new Vector2(1,1),

				new Vector2(0,0), //9
				new Vector2(1,1),
				new Vector2(1,0),

				//Bottom
				new Vector2(1,0), //12
				new Vector2(0,0),
				new Vector2(1,1),
				
				new Vector2(1,1), //15
				new Vector2(1,1),
				new Vector2(0,1),
				
				new Vector2(0,1), //18
				new Vector2(0,1),
				new Vector2(0,1),
				
				new Vector2(0,0), //21
				new Vector2(0,1),
				new Vector2(0,0),
			};
			public static readonly int[] trianglesHard = new int[]{
				/*
				91011-----012
				|\        |\
				| 212223---121314
				678------345
				 \|       \|
				  181920---151617
		       */
				
				//Top
				0,3,6, 6,9,0,
				//Bottom
				12,21,18, 18,15,12,
				//Back
				4,16,7, 7,16,19,
				//Front
				1,10,22, 22,13,1,
				//Right
				2,14,17, 17,5,2,
				//Left
				11,8,20, 20,23,11

			};
		}

        /// <summary>
        /// Creates a cube mesh with welded edges. UVs are only valid for 2 sides. The other sides will be strechted
        /// </summary>
		public static Mesh Cube()
		{
			Mesh _mesh = new Mesh();
			_mesh.vertices = CubeData.vertices;
			_mesh.triangles = CubeData.triangles;
			_mesh.uv = CubeData.uvs;
			_mesh.uv2 = CubeData.uvs;
			_mesh.uv3 = CubeData.uvs;
			_mesh.uv4 = CubeData.uvs;
			_mesh.RecalculateNormals();
			_mesh.RecalculateBounds();
			_mesh.name = "Cube";
			return _mesh;
		}

        /// <summary>
        /// Creates a cube mesh with splitted edges, resulting in hard shading and valid UVs for each cube side.
        /// </summary>
        /// <returns>The hard.</returns>
		public static Mesh CubeHard()
		{
			Mesh _mesh = new Mesh();
			_mesh.vertices = CubeData.verticesHard;
			_mesh.triangles = CubeData.trianglesHard;
			_mesh.uv = CubeData.uvsHard;
			_mesh.uv2 = CubeData.uvsHard;
			_mesh.uv3 = CubeData.uvsHard;
			_mesh.uv4 = CubeData.uvsHard;
			_mesh.RecalculateNormals();
			_mesh.RecalculateBounds();
			_mesh.name = "CubeHard";
			return _mesh;
		}

	}
}