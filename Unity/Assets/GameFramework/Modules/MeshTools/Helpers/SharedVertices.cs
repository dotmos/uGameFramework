using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MeshTools{
	public partial class Helper
	{
		public class VertexConnection
		{
			public List<int> connections = new List<int>();
		}
		
		public static VertexConnection[] GetSharedVertices(Vector3[] vertices)
		{
			VertexConnection[] connections = new VertexConnection[vertices.Length];
			
			for(int i = 0; i < vertices.Length; i++)
			{
				var P1 = vertices[i];
				var VC1 = connections[i];
				for(int n = i+1; n < vertices.Length; n++)
				{
					if (P1 == vertices[n])
					{
						var VC2 = connections[n];
						if (VC2 == null)
							VC2 = connections[n] = new VertexConnection();
						if (VC1 == null)
							VC1 = connections[i] = new VertexConnection();
						VC1.connections.Add(n);
						VC2.connections.Add(i);
					}
				}
			}
			
			return connections;
		}
	}
}
