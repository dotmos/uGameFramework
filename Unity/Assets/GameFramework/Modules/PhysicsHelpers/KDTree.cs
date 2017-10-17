//Define this if you want to use multithreading. This will introduce garbage though.
//#define KDTree_Multithreading 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PhysicsHelper{

    /// <summary>
    /// KD tree. Taken from https://forum.unity3d.com/threads/get-the-collision-points-in-physics-overlapsphere.395176/#post-2581349 by HiddenMonk
    /// </summary>
    public class KDTree
    {
        public KDTree[] lr;
        public Vector3 pivot;
        public int pivotIndex;
        public int axis;

        //    Change this value to 2 if you only need two-dimensional X,Y points. The search will
        //    be quicker in two dimensions.
        const int numDims = 3;

        List<int> nearests = new List<int>();
        protected int[] tris;
        protected Vector3[] verts;
        protected VertTriList vt;

        public KDTree()
        {
            lr = new KDTree[2];
        }

        /// <summary>
        /// Creates a new KDTree from a mesh
        /// </summary>
        /// <returns>The from mesh.</returns>
        /// <param name="mesh">Mesh.</param>
        public static KDTree CreateFromMesh(Mesh mesh){
            Vector3[] verts = mesh.vertices;
            KDTree _tree = MakeFromPoints(verts);
            _tree.tris = mesh.triangles;
            _tree.verts = mesh.vertices;
            _tree.vt = new VertTriList(mesh);
            return _tree;
        }            

        /// <summary>
        /// Returns the nearest point on the kd mesh. meshTransform is the transform of the mesh that the point should be searched for. This is needed for correctly rotating orienting the mesh data.
        /// </summary>
        /// <returns>The point on mesh.</returns>
        /// <param name="pt">Point.</param>
        /// <param name="verts">Verts.</param>
        /// <param name="vertProx">Vert prox.</param>
        /// <param name="tri">Tri.</param>
        /// <param name="vt">Vt.</param>
        public Vector3 FindNearestPointOnMesh(Vector3 point, Transform meshTransform)
        {
            //transform point to mesh local space
            point = meshTransform.InverseTransformPoint(point);

            //    First, find the nearest vertex (the nearest point must be on one of the triangles
            //    that uses this vertex if the mesh is convex).
            //  Since there can be multiple vertices on a single spot, we need to find the correct vert and triangle.
            FindNearestEpsilon(point, nearests);

            Vector3 nearestPt = Vector3.zero;
            float nearestSqDist = float.MaxValue;
            Vector3 possNearestPt;

            for(int i = 0; i < nearests.Count; i++)
            {
                //    Get the list of triangles in which the nearest vert "participates".
                int[] nearTris = vt[nearests[i]];

                for(int j = 0; j < nearTris.Length; j++)
                {
                    int triOff = nearTris[j] * 3;
                    Vector3 a = verts[tris[triOff]];
                    Vector3 b = verts[tris[triOff + 1]];
                    Vector3 c = verts[tris[triOff + 2]];

                    ClosestPoint.ClosestPointOnTriangleToPoint(ref point, ref a, ref b, ref c, out possNearestPt);
                    float possNearestSqDist = (point - possNearestPt).sqrMagnitude;

                    if(possNearestSqDist < nearestSqDist)
                    {
                        nearestPt = possNearestPt;
                        nearestSqDist = possNearestSqDist;
                    }
                }
            }

            //Transform point to world space and return
            return meshTransform.TransformPoint(nearestPt);
        }

        /// <summary>
        /// Make a new tree from a list of points.
        /// </summary>
        /// <returns>The from points.</returns>
        /// <param name="points">Points.</param>
        static KDTree MakeFromPoints(params Vector3[] points)
        {
            int[] indices = Iota(points.Length);
            return MakeFromPointsInner(0, 0, points.Length - 1, points, indices);
        }

        /// <summary>
        /// Recursively build a tree by separating points at plane boundaries.
        /// </summary>
        /// <returns>The from points inner.</returns>
        /// <param name="depth">Depth.</param>
        /// <param name="stIndex">St index.</param>
        /// <param name="enIndex">En index.</param>
        /// <param name="points">Points.</param>
        /// <param name="inds">Inds.</param>
        static KDTree MakeFromPointsInner(int depth, int stIndex, int enIndex, Vector3[] points, int[] inds)
        {
            KDTree root = new KDTree();
            root.axis = depth % numDims;
            int splitPoint = FindPivotIndex(points, inds, stIndex, enIndex, root.axis);

            root.pivotIndex = inds[splitPoint];
            root.pivot = points[root.pivotIndex];

            int leftEndIndex = splitPoint - 1;

            if(leftEndIndex >= stIndex)
            {
                root.lr[0] = MakeFromPointsInner(depth + 1, stIndex, leftEndIndex, points, inds);
            }

            int rightStartIndex = splitPoint + 1;

            if(rightStartIndex <= enIndex)
            {
                root.lr[1] = MakeFromPointsInner(depth + 1, rightStartIndex, enIndex, points, inds);
            }

            return root;
        }

        static void SwapElements(int[] arr, int a, int b)
        {
            int temp = arr[a];
            arr[a] = arr[b];
            arr[b] = temp;
        }

        /// <summary>
        /// Simple "median of three" heuristic to find a reasonable splitting plane.
        /// </summary>
        /// <returns>The split point.</returns>
        /// <param name="points">Points.</param>
        /// <param name="inds">Inds.</param>
        /// <param name="stIndex">St index.</param>
        /// <param name="enIndex">En index.</param>
        /// <param name="axis">Axis.</param>
        static int FindSplitPoint(Vector3[] points, int[] inds, int stIndex, int enIndex, int axis)
        {
            float a = points[inds[stIndex]][axis];
            float b = points[inds[enIndex]][axis];
            int midIndex = (stIndex + enIndex) / 2;
            float m = points[inds[midIndex]][axis];

            if(a > b)
            {
                if(m > a)
                {
                    return stIndex;
                }

                if(b > m)
                {
                    return enIndex;
                }

                return midIndex;
            }
            else
            {
                if(a > m)
                {
                    return stIndex;
                }

                if(m > b)
                {
                    return enIndex;
                }

                return midIndex;
            }
        }

        /// <summary>
        /// Find a new pivot index from the range by splitting the points that fall either side
        //  of its plane.
        /// </summary>
        /// <returns>The pivot index.</returns>
        /// <param name="points">Points.</param>
        /// <param name="inds">Inds.</param>
        /// <param name="stIndex">St index.</param>
        /// <param name="enIndex">En index.</param>
        /// <param name="axis">Axis.</param>
        public static int FindPivotIndex(Vector3[] points, int[] inds, int stIndex, int enIndex, int axis)
        {
            int splitPoint = FindSplitPoint(points, inds, stIndex, enIndex, axis);
            // int splitPoint = Random.Range(stIndex, enIndex);

            Vector3 pivot = points[inds[splitPoint]];
            SwapElements(inds, stIndex, splitPoint);

            int currPt = stIndex + 1;
            int endPt = enIndex;

            while(currPt <= endPt)
            {
                Vector3 curr = points[inds[currPt]];

                if((curr[axis] > pivot[axis]))
                {
                    SwapElements(inds, currPt, endPt);
                    endPt--;
                }
                else
                {
                    SwapElements(inds, currPt - 1, currPt);
                    currPt++;
                }
            }

            return currPt - 1;
        }

        public static int[] Iota(int num)
        {
            int[] result = new int[num];

            for(int i = 0; i < num; i++)
            {
                result[i] = i;
            }

            return result;
        }

        /// <summary>
        /// Find the nearest point in the set to the supplied point.
        /// </summary>
        /// <returns>The nearest.</returns>
        /// <param name="pt">Point.</param>
        public int FindNearest(Vector3 pt)
        {
            float bestSqDist = float.MaxValue;
            int bestIndex = -1;

            Search(pt, ref bestSqDist, ref bestIndex);

            return bestIndex;
        }

        /// <summary>
        /// Recursively search the tree.
        /// </summary>
        /// <param name="pt">Point.</param>
        /// <param name="bestSqSoFar">Best sq so far.</param>
        /// <param name="bestIndex">Best index.</param>
        void Search(Vector3 pt, ref float bestSqSoFar, ref int bestIndex)
        {
            float mySqDist = (pivot - pt).sqrMagnitude;

            if(mySqDist < bestSqSoFar)
            {
                bestSqSoFar = mySqDist;
                bestIndex = pivotIndex;
            }

            float planeDist = pt[axis] - pivot[axis]; //DistFromSplitPlane(pt, pivot, axis);

            int selector = planeDist <= 0 ? 0 : 1;

            if(lr[selector] != null)
            {
                lr[selector].Search(pt, ref bestSqSoFar, ref bestIndex);
            }

            selector = (selector + 1) % 2;

            float sqPlaneDist = planeDist * planeDist;

            if((lr[selector] != null) && (bestSqSoFar > sqPlaneDist))
            {
                lr[selector].Search(pt, ref bestSqSoFar, ref bestIndex);
            }
        }

        /// <summary>
        /// Its possible for vertices the be in the exact same position, so we want to grab all of them.
        /// </summary>
        /// <returns>The nearest epsilon.</returns>
        /// <param name="pt">Point.</param>
        /// <param name="resultBuffer">Result buffer.</param>
        public IList<int> FindNearestEpsilon(Vector3 pt, IList<int> resultBuffer) //Use result buffer to avoid garbage collection.
        {
            resultBuffer.Clear();

            float bestSqDist = float.MaxValue;
            int bestIndex = -1;

            SearchEpsilon(pt, ref bestSqDist, ref bestIndex, resultBuffer);

            return resultBuffer;
        }

        void SearchEpsilon(Vector3 pt, ref float bestSqSoFar, ref int bestIndex, IList<int> resultBuffer)
        {
            float mySqDist = (pivot - pt).sqrMagnitude;

            if((mySqDist < bestSqSoFar || Mathf.Abs(mySqDist - bestSqSoFar) < Mathf.Epsilon))
            {
                if(mySqDist < bestSqSoFar + Mathf.Epsilon + Mathf.Epsilon) resultBuffer.Clear();

                bestSqSoFar = mySqDist;
                bestIndex = pivotIndex;
                resultBuffer.Add(pivotIndex);
            }

            float planeDist = pt[axis] - pivot[axis]; //DistFromSplitPlane(pt, pivot, axis);

            int selector = planeDist <= 0 ? 0 : 1;

            if(lr[selector] != null)
            {
                lr[selector].SearchEpsilon(pt, ref bestSqSoFar, ref bestIndex, resultBuffer);
            }

            selector = (selector + 1) % 2;

            float sqPlaneDist = planeDist * planeDist;

            if((lr[selector] != null) && (bestSqSoFar > sqPlaneDist))
            {
                lr[selector].SearchEpsilon(pt, ref bestSqSoFar, ref bestIndex, resultBuffer);
            }
        }

        /// <summary>
        /// Get a point's distance from an axis-aligned plane.
        /// </summary>
        /// <returns>The from split plane.</returns>
        /// <param name="pt">Point.</param>
        /// <param name="planePt">Plane point.</param>
        /// <param name="axis">Axis.</param>
        float DistFromSplitPlane(Vector3 pt, Vector3 planePt, int axis)
        {
            return pt[axis] - planePt[axis];
        }

        /// <summary>
        /// Simple output of tree structure - mainly useful for getting a rough
        //  idea of how deep the tree is (and therefore how well the splitting
        //  heuristic is performing).
        /// </summary>
        /// <param name="level">Level.</param>
        public string Dump(int level)
        {
            string result = pivotIndex.ToString().PadLeft(level) + "\n";

            if(lr[0] != null)
            {
                result += lr[0].Dump(level + 2);
            }

            if(lr[1] != null)
            {
                result += lr[1].Dump(level + 2);
            }

            return result;
        }
    }
}