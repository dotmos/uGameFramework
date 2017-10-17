using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MeshTools{
    public partial class Modifier {

        /// <summary>
        /// Extrude the specified mesh by amount.
        /// If hard edges is set to true, some vertices will be duplicated. If you want to create UVs, you also need to set hardEdges to true.
        /// If keepOriginalFaces is set to true, the original faces will NOT be deleted. 
        /// If inverseOriginalFaces is set to true, original faces will be inverted before extruding, creating a watertight mesh.
        /// </summary>
        /// <param name="mesh">Mesh.</param>
        /// <param name="amount">Amount.</param>
        /// <param name="uvSizePerUnity">Uv size per unity.</param>
        /// <param name="hardEdges">If set to <c>true</c> hard edges.</param>
        /// <param name="keepOriginalFaces">If set to <c>true</c> keep original faces.</param>
        /// <param name="inverseOriginalFaces">If set to <c>true</c> inverse original faces.</param>
        public static void Extrude(Mesh mesh, float amount, Vector2 uvSizePerUnit, bool hardEdges = true, bool keepOriginalFaces = false, bool inverseOriginalFaces = false){

            //Cache normals
            Vector3[] orgNormals = mesh.normals;

            //Cache original vertices.
            Vector3[] bottomVertices = mesh.vertices;
            //Store for "top" vertices
            Vector3[] topVertices = new Vector3[bottomVertices.Length];

            //UVs
            Vector2[] orgUVs = mesh.uv;
            List<Vector2> uvs = new List<Vector2>();

            //Create vertex array
            List<Vector3> vertices = new List<Vector3>();
            //Add bottom vertices of we want to keep original faces OR if mesh should not have hard edges
            if(keepOriginalFaces || !hardEdges)
            {
                vertices.AddRange(bottomVertices);
                uvs.AddRange(orgUVs);
            }

            int topVertexIndexOffset = vertices.Count;

            //Store original triangles
            int[] orgTriangles = new int[mesh.triangles.Length];
            mesh.triangles.CopyTo(orgTriangles, 0);

            //Inverse mesh normals?
            if(inverseOriginalFaces){
                InverseNormals(mesh);
            }

            //Create extruded face
            for(int i=0; i<bottomVertices.Length; ++i){
                //vertices.Add( topAndBottomVertices[i] + (orgNormals[i] * amount) ); // "top"
                topVertices[i] = bottomVertices[i] + (orgNormals[i] * amount);
                uvs.Add( orgUVs[i] );
            }
            vertices.AddRange(topVertices);

            //Calculate outer/boundary edges from triangles so we know where we have to create new triangles/walls, connecting the original(bottom) and extruded polygon(top)
            int[] tempTriangles;
            if(inverseOriginalFaces) tempTriangles = mesh.triangles;
            else tempTriangles = orgTriangles;
            List<Helper.Edge> boundaryEdges = Helper.Edge.CreateBoundaryEdgeArray(tempTriangles);

            //Create triangle list
            List<int> triangles = new List<int>();
            //Add original triangles
            if(keepOriginalFaces){
                for(int i=0; i<tempTriangles.Length; ++i)
                {
                    triangles.Add(tempTriangles[i]);
                }
            }
            //Add triangles for extruded face
            for(int i=0; i<orgTriangles.Length; ++i)
            {
                triangles.Add(orgTriangles[i]+topVertexIndexOffset);
            }

            //Add "walls"
            if(hardEdges){
                int _vCount = 0;
                if(inverseOriginalFaces)
                {
                    foreach(Helper.Edge e in boundaryEdges){
                        _vCount = vertices.Count;

                        //New vertices for first triangle
                        Vector3 v1,v2,v3;
                        v1 = topVertices[e._v2];
                        v2 = bottomVertices[e._v2];
                        v3 = bottomVertices[e._v1];
                        vertices.Add( v1 );
                        vertices.Add( v2 );
                        vertices.Add( v3 );
                        //First triangle
                        triangles.Add( _vCount);
                        triangles.Add( _vCount+1);
                        triangles.Add( _vCount+2);
                        //Create uvs
                        float d1 = Vector3.Distance(v1, v2);
                        float d2 = Vector3.Distance(v2, v3);
                        uvs.Add(new Vector2(d2/uvSizePerUnit.x, d1/uvSizePerUnit.y));
                        uvs.Add(new Vector2(d2/uvSizePerUnit.x, 0));
                        uvs.Add(new Vector2(0,0));

                        //New vertices for second triangle
                        Vector3 v4,v5,v6;
                        v4 = bottomVertices[e._v1];
                        v5 = topVertices[e._v1];
                        v6 = topVertices[e._v2];
                        vertices.Add( v4 );
                        vertices.Add( v5 );
                        vertices.Add( v6 );
                        //Second triangle
                        d1 = Vector3.Distance(v4, v5);
                        d2 = Vector3.Distance(v5, v6);
                        triangles.Add( _vCount+3 );
                        triangles.Add( _vCount+4 );
                        triangles.Add( _vCount+5 );
                        //Create uvs
                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(0, d1/uvSizePerUnit.y));
                        uvs.Add(new Vector2(d2/uvSizePerUnit.x, d1/uvSizePerUnit.y));
                    }
                } else {
                    foreach(Helper.Edge e in boundaryEdges){
                        _vCount = vertices.Count;

                        //New vertices for first triangle
                        vertices.Add( topVertices[e._v2] );
                        vertices.Add( bottomVertices[e._v2] );
                        vertices.Add( bottomVertices[e._v1] );
                        //First triangle
                        triangles.Add( _vCount+2);
                        triangles.Add( _vCount+1);
                        triangles.Add( _vCount);
                        //Create uvs
                        uvs.Add(new Vector2(0,1));
                        uvs.Add(new Vector2(0,0));
                        uvs.Add(new Vector2(1,0));

                        //New vertices for second triangle
                        vertices.Add( bottomVertices[e._v1] );
                        vertices.Add( topVertices[e._v1] );
                        vertices.Add( topVertices[e._v2] );
                        //Second triangle
                        triangles.Add( _vCount+5 );
                        triangles.Add( _vCount+4 );
                        triangles.Add( _vCount+3 );
                        //Create uvs
                        uvs.Add(new Vector2(1,0));
                        uvs.Add(new Vector2(1,1));
                        uvs.Add(new Vector2(0,1));
                    }
                }
            } else {
                if(inverseOriginalFaces)
                {
                    foreach(Helper.Edge e in boundaryEdges){
                        //First triangle
                        triangles.Add( e._v2+topVertexIndexOffset);
                        triangles.Add( e._v2);
                        triangles.Add( e._v1);
                        //Second triangle
                        triangles.Add( e._v1);
                        triangles.Add( e._v1+topVertexIndexOffset);
                        triangles.Add( e._v2+topVertexIndexOffset);
                    }
                } else {
                    foreach(Helper.Edge e in boundaryEdges){
                        //First triangle
                        triangles.Add( e._v1);
                        triangles.Add( e._v2);
                        triangles.Add( e._v2+topVertexIndexOffset);
                        //Second triangle
                        triangles.Add( e._v2+topVertexIndexOffset);
                        triangles.Add( e._v1+topVertexIndexOffset);
                        triangles.Add( e._v1);
                    }
                }
            }

           
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();

            //return newMesh;
        }

        public static void Extrude(Mesh mesh, float amount, bool hardEdges = true, bool keepOriginalFaces = false, bool inverseOriginalFaces = false){
            Extrude(mesh, amount, Vector2.one, hardEdges, keepOriginalFaces, inverseOriginalFaces);
        }
    }
}