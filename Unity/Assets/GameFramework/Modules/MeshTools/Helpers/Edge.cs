using System.Collections;
using System.Collections.Generic;

namespace MeshTools{
    public partial class Helper{
        public class Edge{
            public int _v1;
            public int _v2;

            public Edge(int v1, int v2){
                _v1 = v1;
                _v2 = v2;
            }

            /// <summary>
            /// Checks if this edge is a boundary edge. Returns true if edge is boundary.
            /// </summary>
            /// <returns><c>true</c>, if if boundary was checked, <c>false</c> otherwise.</returns>
            /// <param name="edges">Edges.</param>
            public bool CheckIfBoundary(List<Edge> edges){
                for(int i=0; i<edges.Count; ++i){
                    if(edges[i]._v1 == _v2 && edges[i]._v2 == _v1) return false;
                }
                return true;
            }

            /// <summary>
            /// Create an array of edges from a triangle list
            /// </summary>
            /// <returns>The edge array.</returns>
            /// <param name="triangles">Triangles.</param>
            public static List<Edge> CreateEdgeArray(int[] triangles){
                List<Edge> _edges = new List<Edge>();
                for(int i=0; i<triangles.Length; i+=3){
                    _edges.Add(new Edge(triangles[i], triangles[i+1]));
                    _edges.Add(new Edge(triangles[i+1], triangles[i+2]));
                    _edges.Add(new Edge(triangles[i+2], triangles[i]));
                }

                return _edges;
            }

            /// <summary>
            /// Create an array of edges from a triangle list, only creating an edge if its a boundary edge
            /// </summary>
            /// <returns>The boundary edge array.</returns>
            /// <param name="triangles">Triangles.</param>
            public static List<Edge> CreateBoundaryEdgeArray(int[] triangles){
                List<Edge> _allEdges = CreateEdgeArray(triangles);
                List<Edge> _boundaryEdges = new List<Edge>();
                foreach(Edge e in _allEdges){
                    if(e.CheckIfBoundary(_allEdges)) _boundaryEdges.Add(e);
                }

                _allEdges.Clear();

                return _boundaryEdges;
            }
        }
    }
}