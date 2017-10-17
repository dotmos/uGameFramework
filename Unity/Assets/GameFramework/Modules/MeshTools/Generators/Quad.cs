using UnityEngine;

namespace MeshTools
{
    public partial class Generator
    {
        public class Quad
        {
            /// <summary>
            /// Create XZ based plane off a given bounds object.
            /// Subtract the given vector3 position by worldPosition
            /// Scale the uvs by a given factor: uvSizeFactor
            /// </summary>
            /// <param name="mesh">Reference to the mesh object. Used to avoid creating new Mesh() data [better GC scenario]</param>
            /// <param name="bounds">Bounds object to use for creating the quad mesh.</param>
            /// <param name="worldPosition">Subtract the given vector3 position. (Used to fix wrong positioning when bounds is world position oriented) </param>
            /// <param name="uvSizeFactor">Scale the uvs by a given factor.</param>
            public static void CreateXZMesh(Mesh mesh, Bounds bounds, Vector3 worldPosition, float uvSizeFactor = 1f)
            {
                if (mesh == null)
                {
                    mesh = new Mesh();
                }
                else
                {
                    mesh.Clear();
                }

                mesh.vertices = new Vector3[]
                {
                new Vector3(bounds.min.x - worldPosition.x, 0, bounds.min.z - worldPosition.z),
                new Vector3(bounds.min.x - worldPosition.x, 0, bounds.max.z - worldPosition.z),
                new Vector3(bounds.max.x - worldPosition.x, 0, bounds.max.z - worldPosition.z),
                new Vector3(bounds.max.x - worldPosition.x, 0, bounds.min.z - worldPosition.z)
                };

                mesh.uv = new Vector2[]
                {
                new Vector2(0,0),
                new Vector2(0, 1f*uvSizeFactor*bounds.size.z),
                new Vector2(1f*uvSizeFactor*bounds.size.x, 1f*uvSizeFactor*bounds.size.z),
                new Vector2(1f*uvSizeFactor*bounds.size.x, 0),
                };

                mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();
                mesh.RecalculateNormals();

            }

            /// <summary>
            /// Create XZ based plane off a given bounds object.
            /// Scale the uvs by a given factor.
            /// </summary>
            /// <param name="mesh">Reference to the mesh object. Used to avoid creating new Mesh() data [better GC scenario]</param>
            /// <param name="bounds">Bounds object to use for creating the quad mesh.</param>
            /// <param name="uvSizeFactor">Scale the uvs by a given factor.</param>
            public static void CreateXZMesh(Mesh mesh, Bounds bounds, float uvSizeFactor = 1f)
            {
                CreateXZMesh(mesh, bounds, Vector3.zero, uvSizeFactor);
            }
        }
    }
}

