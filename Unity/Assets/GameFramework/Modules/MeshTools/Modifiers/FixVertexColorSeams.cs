using UnityEngine;
using System.Collections;

namespace MeshTools{
	public partial class Modifier{
		//fix seams in vertex colors for a non-watertight mesh / mesh with seams
		public static void FixVertexColorSeams(Mesh mesh)
		{
			Vector3[] vertices = mesh.vertices;
			Helper.VertexConnection[] connections = Helper.GetSharedVertices(vertices);
			
			Color[] colors = mesh.colors;
			
			//Cache
			Helper.VertexConnection connection;
			Color color;
			
			for(int i=0; i<connections.Length; ++i)
			{
				connection = connections[i];
				if(connection == null)
					continue;
				
				color = Color.black;
				for(int sv=0; sv<connection.connections.Count; ++sv)
				{
					color += colors[connection.connections[sv]];
				}
				colors[i] = color/connection.connections.Count;
			}
			
			mesh.colors = colors;
		}

	}
}
