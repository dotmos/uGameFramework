/*MIT License

Copyright(c) 2018 Vili Volčini / viliwonka

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace DataStructures.ViliWonka.KDTree{

    public interface IPositionable {
        Vector3 Position { get; set; }
        object UserObject { get; }
    }

    public class KDGameObject : IPositionable {
        public GameObject gobj;

        public KDGameObject(GameObject gobj) {
            this.gobj = gobj;
        }

        public Vector3 Position {
            get {
                return gobj.transform.position;
            }
            set {
                gobj.transform.position = value;
            }
        }

        public object UserObject {
            get { return gobj; }
        }
    }

        public class KDTree<T> : IList<T> where T : IPositionable {
            public KDNode RootNode { get; private set; }

            public List<T> Points { get { return points; } } // points on which kd-tree will build on. This array will stay unchanged when re/building kdtree!
            private List<T> points;

            public int[] Permutation { get { return permutation; } } // index aray, that will be permuted
            private int[] permutation;

            public bool IsReadOnly { get { return ((IList<T>)points).IsReadOnly; } }

            public int Count {
                get {
                    return ((IList<T>)points).Count;
                }
            }

            public T this[int index] {
                get { return ((List<T>)points)[index]; }
                set { ((IList<T>)points)[index] = value; }
            }

            private int maxPointsPerLeafNode = 32;

            private KDNode[] kdNodesStack;
            private int kdNodesCount = 0;

            public KDTree(int maxPointsPerLeafNode = 16) {
                points = new List<T>();
                permutation = new int[0];

                kdNodesStack = new KDNode[64];

                this.maxPointsPerLeafNode = maxPointsPerLeafNode;
            }

            public KDTree(List<T> points, int maxPointsPerLeafNode = 16) {

                this.points = points;
                this.permutation = new int[points.Count];

                kdNodesStack = new KDNode[64];

                this.maxPointsPerLeafNode = maxPointsPerLeafNode;

                Rebuild();
            }

            public void Rebuild(int maxPointsPerLeafNode = -1) {

                for (int i = 0; i < Count; i++) {
                    permutation[i] = i;
                }

                if (maxPointsPerLeafNode > 0) {
                    this.maxPointsPerLeafNode = maxPointsPerLeafNode;
                }
                BuildTree();
            }

            void SetData(List<T> data) {
                points.Clear();
                foreach (var d in data) {
                    Add(d, false);
                }
                Rebuild();
            }

            void BuildTree() {

                ResetKDNodeStack();

                RootNode = GetKDNode();
                RootNode.bounds = MakeBounds();
                RootNode.start = 0;
                RootNode.end = Count;

                SplitNode(RootNode);
            }

            KDNode GetKDNode() {

                KDNode node = null;

                if (kdNodesCount < kdNodesStack.Length) {

                    if (kdNodesStack[kdNodesCount] == null) {
                        kdNodesStack[kdNodesCount] = node = new KDNode();
                    } else {
                        node = kdNodesStack[kdNodesCount];
                        node.partitionAxis = -1;
                    }
                } else {

                    // automatic resize of KDNode pool array
                    Array.Resize(ref kdNodesStack, kdNodesStack.Length * 2);
                    node = kdNodesStack[kdNodesCount] = new KDNode();
                }

                kdNodesCount++;

                return node;
            }

            void ResetKDNodeStack() {
                kdNodesCount = 0;
            }

            /// <summary>
            /// For calculating root node bounds
            /// </summary>
            /// <returns>Boundary of all Vector3 points</returns>
            KDBounds MakeBounds() {

                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

                int even = Count & ~1; // calculate even Length

                // min, max calculations
                // 3n/2 calculations instead of 2n
                for (int i0 = 0; i0 < even; i0 += 2) {

                    int i1 = i0 + 1;

                    // X Coords
                    if (points[i0].Position.x > points[i1].Position.x) {
                        // i0 is bigger, i1 is smaller
                        if (points[i1].Position.x < min.x)
                            min.x = points[i1].Position.x;

                        if (points[i0].Position.x > max.x)
                            max.x = points[i0].Position.x;
                    } else {
                        // i1 is smaller, i0 is bigger
                        if (points[i0].Position.x < min.x)
                            min.x = points[i0].Position.x;

                        if (points[i1].Position.x > max.x)
                            max.x = points[i1].Position.x;
                    }

                    // Y Coords
                    if (points[i0].Position.y > points[i1].Position.y) {
                        // i0 is bigger, i1 is smaller
                        if (points[i1].Position.y < min.y)
                            min.y = points[i1].Position.y;

                        if (points[i0].Position.y > max.y)
                            max.y = points[i0].Position.y;
                    } else {
                        // i1 is smaller, i0 is bigger
                        if (points[i0].Position.y < min.y)
                            min.y = points[i0].Position.y;

                        if (points[i1].Position.y > max.y)
                            max.y = points[i1].Position.y;
                    }

                    // Z Coords
                    if (points[i0].Position.z > points[i1].Position.z) {
                        // i0 is bigger, i1 is smaller
                        if (points[i1].Position.z < min.z)
                            min.z = points[i1].Position.z;

                        if (points[i0].Position.z > max.z)
                            max.z = points[i0].Position.z;
                    } else {
                        // i1 is smaller, i0 is bigger
                        if (points[i0].Position.z < min.z)
                            min.z = points[i0].Position.z;

                        if (points[i1].Position.z > max.z)
                            max.z = points[i1].Position.z;
                    }
                }

                // if array was odd, calculate also min/max for the last element
                if (even != Count) {
                    // X
                    if (min.x > points[even].Position.x)
                        min.x = points[even].Position.x;

                    if (max.x < points[even].Position.x)
                        max.x = points[even].Position.x;
                    // Y
                    if (min.y > points[even].Position.y)
                        min.y = points[even].Position.y;

                    if (max.y < points[even].Position.y)
                        max.y = points[even].Position.y;
                    // Z
                    if (min.z > points[even].Position.z)
                        min.z = points[even].Position.z;

                    if (max.z < points[even].Position.z)
                        max.z = points[even].Position.z;
                }

                KDBounds b = new KDBounds();
                b.min = min;
                b.max = max;

                return b;
            }

            /// <summary>
            /// Recursive splitting procedure
            /// </summary>
            /// <param name="parent">This is where root node goes</param>
            /// <param name="depth"></param>
            ///
            void SplitNode(KDNode parent) {

                // center of bounding box
                KDBounds parentBounds = parent.bounds;
                Vector3 parentBoundsSize = parentBounds.size;

                // Find axis where bounds are largest
                int splitAxis = 0;
                float axisSize = parentBoundsSize.x;

                if (axisSize < parentBoundsSize.y) {
                    splitAxis = 1;
                    axisSize = parentBoundsSize.y;
                }

                if (axisSize < parentBoundsSize.z) {
                    splitAxis = 2;
                }

                // Our axis min-max bounds
                float boundsStart = parentBounds.min[splitAxis];
                float boundsEnd = parentBounds.max[splitAxis];

                // Calculate the spliting coords
                float splitPivot = CalculatePivot(parent.start, parent.end, boundsStart, boundsEnd, splitAxis);

                parent.partitionAxis = splitAxis;
                parent.partitionCoordinate = splitPivot;

                // 'Spliting' array to two subarrays
                int splittingIndex = Partition(parent.start, parent.end, splitPivot, splitAxis);

                // Negative / Left node
                Vector3 negMax = parentBounds.max;
                negMax[splitAxis] = splitPivot;

                KDNode negNode = GetKDNode();
                negNode.bounds = parentBounds;
                negNode.bounds.max = negMax;
                negNode.start = parent.start;
                negNode.end = splittingIndex;
                parent.negativeChild = negNode;

                // Positive / Right node
                Vector3 posMin = parentBounds.min;
                posMin[splitAxis] = splitPivot;

                KDNode posNode = GetKDNode();
                posNode.bounds = parentBounds;
                posNode.bounds.min = posMin;
                posNode.start = splittingIndex;
                posNode.end = parent.end;
                parent.positiveChild = posNode;

                // Constraint function deciding if split should be continued
                if (ContinueSplit(negNode))
                    SplitNode(negNode);

                if (ContinueSplit(posNode))
                    SplitNode(posNode);
            }

            /// <summary>
            /// Sliding midpoint splitting pivot calculation
            /// 1. First splits node to two equal parts (midPoint)
            /// 2. Checks if elements are in both sides of splitted bounds
            /// 3a. If they are, just return midPoint
            /// 3b. If they are not, then points are only on left or right bound.
            /// 4. Move the splitting pivot so that it shrinks part with points completely (calculate min or max dependent) and return.
            /// </summary>
            /// <param name="start"></param>
            /// <param name="end"></param>
            /// <param name="boundsStart"></param>
            /// <param name="boundsEnd"></param>
            /// <param name="axis"></param>
            /// <returns></returns>
            float CalculatePivot(int start, int end, float boundsStart, float boundsEnd, int axis) {

                //! sliding midpoint rule
                float midPoint = (boundsStart + boundsEnd) / 2f;

                bool negative = false;
                bool positive = false;

                float negMax = Single.MinValue;
                float posMin = Single.MaxValue;

                // this for loop section is used both for sorted and unsorted data
                for (int i = start; i < end; i++) {

                    if (points[permutation[i]].Position[axis] < midPoint)
                        negative = true;
                    else
                        positive = true;

                    if (negative == true && positive == true)
                        return midPoint;
                }

                if (negative) {

                    for (int i = start; i < end; i++)
                        if (negMax < points[permutation[i]].Position[axis])
                            negMax = points[permutation[i]].Position[axis];

                    return negMax;
                } else {

                    for (int i = start; i < end; i++)
                        if (posMin > points[permutation[i]].Position[axis])
                            posMin = points[permutation[i]].Position[axis];

                    return posMin;
                }
            }

            /// <summary>
            /// Similar to Hoare partitioning algorithm (used in Quick Sort)
            /// Modification: pivot is not left-most element but is instead argument of function
            /// Calculates splitting index and partially sorts elements (swaps them until they are on correct side - depending on pivot)
            /// Complexity: O(n)
            /// </summary>
            /// <param name="start">Start index</param>
            /// <param name="end">End index</param>
            /// <param name="partitionPivot">Pivot that decides boundary between left and right</param>
            /// <param name="axis">Axis of this pivoting</param>
            /// <returns>
            /// Returns splitting index that subdivides array into 2 smaller arrays
            /// left = [start, pivot),
            /// right = [pivot, end)
            /// </returns>
            int Partition(int start, int end, float partitionPivot, int axis) {

                // note: increasing right pointer is actually decreasing!
                int LP = start - 1; // left pointer (negative side)
                int RP = end;       // right pointer (positive side)

                int temp;           // temporary var for swapping permutation indexes

                while (true) {

                    do {
                        // move from left to the right until "out of bounds" value is found
                        LP++;
                    }
                    while (LP < RP && points[permutation[LP]].Position[axis] < partitionPivot);

                    do {
                        // move from right to the left until "out of bounds" value found
                        RP--;
                    }
                    while (LP < RP && points[permutation[RP]].Position[axis] >= partitionPivot);

                    if (LP < RP) {
                        // swap
                        temp = permutation[LP];
                        permutation[LP] = permutation[RP];
                        permutation[RP] = temp;
                    } else {

                        return LP;
                    }
                }
            }

            /// <summary>
            /// Constraint function. You can add custom constraints here - if you have some other data/classes binded to Vector3 points
            /// Can hardcode it into
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            bool ContinueSplit(KDNode node) {

                return (node.Count > maxPointsPerLeafNode);
            }

            public int IndexOf(T item) {
                return ((IList<T>)points).IndexOf(item);
            }

            public void Insert(int index, T item) {
                ((IList<T>)points).Insert(index, item);
                Rebuild();
            }

            public void RemoveAt(int index) {
                ((IList<T>)points).RemoveAt(index);
                Rebuild();
            }

            public void Add(T item, bool rebuild = true) {
                points.Add(item);
                if (rebuild) {
                    Rebuild();
                }
            }

            public void Clear() {
                ((IList<T>)points).Clear();
                Rebuild();
            }

            public bool Contains(T item) {
                return ((IList<T>)points).Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex) {
                ((IList<T>)points).CopyTo(array, arrayIndex);
                Rebuild();
            }

            public bool Remove(T item) {
                var result = ((IList<T>)points).Remove(item);
                Rebuild();
                return result;

            }

            public IEnumerator<T> GetEnumerator() {
                return ((IList<T>)points).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return ((IList<T>)points).GetEnumerator();
            }

            public void Add(T item) {
                Add(item, true);
            }
        }
    
}