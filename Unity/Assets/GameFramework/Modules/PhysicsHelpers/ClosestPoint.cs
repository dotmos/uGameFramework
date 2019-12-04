﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sphere collider extensions.
/// Uses code from https://github.com/IronWarrior/SuperCharacterController/blob/master/Assets/SuperCharacterController/Core/SuperCollider.cs
/// </summary>
namespace PhysicsHelper{
    public class ClosestPoint{
        /// <summary>
        /// Closest point on sphere.
        /// </summary>
        /// <returns>The point on sphere.</returns>
        /// <param name="sphere">Sphere.</param>
        /// <param name="point">Point.</param>
        public static Vector3 ClosestPointOnSphere(SphereCollider sphere, Vector3 point)
        {
            Vector3 p;

            p = point - (sphere.transform.position + sphere.center);
            p.Normalize();

            p *= sphere.radius * sphere.transform.localScale.x;
            p += sphere.transform.position + sphere.center;

            return p;
        }

        /// <summary>
        /// Closest point on box.
        /// </summary>
        /// <returns>The point on box.</returns>
        /// <param name="box">Box.</param>
        /// <param name="point">Point.</param>
        public static Vector3 ClosestPointOnBox(BoxCollider box, Vector3 point)
        {
            // Cache the collider transform
            Transform ct = box.transform;

            // Firstly, transform the point into the space of the collider
            Vector3 local = ct.InverseTransformPoint(point);

            // Now, shift it to be in the center of the box
            local -= box.center;

            //Pre multiply to save operations.
            Vector3 halfSize = box.size * 0.5f;

            // Clamp the points to the collider's extents
            Vector3 localNorm = new Vector3(
                Mathf.Clamp(local.x, -halfSize.x, halfSize.x),
                Mathf.Clamp(local.y, -halfSize.y, halfSize.y),
                Mathf.Clamp(local.z, -halfSize.z, halfSize.z)
            );

            //Calculate distances from each edge
            float dx = Mathf.Min(Mathf.Abs(halfSize.x - localNorm.x), Mathf.Abs(-halfSize.x - localNorm.x));
            float dy = Mathf.Min(Mathf.Abs(halfSize.y - localNorm.y), Mathf.Abs(-halfSize.y - localNorm.y));
            float dz = Mathf.Min(Mathf.Abs(halfSize.z - localNorm.z), Mathf.Abs(-halfSize.z - localNorm.z));

            // Select a face to project on
            if (dx < dy && dx < dz)
            {
                localNorm.x = Mathf.Sign(localNorm.x) * halfSize.x;
            }
            else if (dy < dx && dy < dz)
            {
                localNorm.y = Mathf.Sign(localNorm.y) * halfSize.y;
            }
            else if (dz < dx && dz < dy)
            {
                localNorm.z = Mathf.Sign(localNorm.z) * halfSize.z;
            }

            // Now we undo our transformations
            localNorm += box.center;

            // Return resulting point
            return ct.TransformPoint(localNorm);
        }

        /// <summary>
        /// Closest point on capsule.
        /// </summary>
        /// <returns>The point on capsule.</returns>
        /// <param name="capsule">Capsule.</param>
        /// <param name="point">Point.</param>
        public static Vector3 ClosestPointOnCapsule(CapsuleCollider capsule, Vector3 point)
        {
            Transform ct = capsule.transform; // Transform of the collider

            float lineLength = capsule.height - capsule.radius * 2; // The length of the line connecting the center of both sphere
            Vector3 dir = Vector3.up;

            Vector3 upperSphere = dir * lineLength * 0.5f + capsule.center; // The position of the radius of the upper sphere in local coordinates
            Vector3 lowerSphere = -dir * lineLength * 0.5f + capsule.center; // The position of the radius of the lower sphere in local coordinates

            Vector3 local = ct.InverseTransformPoint(point); // The position of the controller in local coordinates

            Vector3 p = Vector3.zero; // Contact point
            Vector3 pt = Vector3.zero; // The point we need to use to get a direction vector with the controller to calculate contact point

            if (local.y < lineLength * 0.5f && local.y > -lineLength * 0.5f) // Controller is contacting with cylinder, not spheres
                pt = dir * local.y + capsule.center;
            else if (local.y > lineLength * 0.5f) // Controller is contacting with the upper sphere 
                pt = upperSphere;
            else if (local.y < -lineLength * 0.5f) // Controller is contacting with lower sphere
                pt = lowerSphere;

            //Calculate contact point in local coordinates and return it in world coordinates
            p = local - pt;
            p.Normalize();
            p = p * capsule.radius + pt;
            return ct.TransformPoint(p);

        }

        /// <summary>
        /// Returns the closest point  on a triangle
        /// </summary>
        /// <param name="point">Point.</param>
        /// <param name="vertex1">Vertex1.</param>
        /// <param name="vertex2">Vertex2.</param>
        /// <param name="vertex3">Vertex3.</param>
        /// <param name="result">Result.</param>
        public static void ClosestPointOnTriangleToPoint(ref Vector3 point, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out Vector3 result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 136

            //Check if P in vertex region outside A
            Vector3 ab = vertex2 - vertex1;
            Vector3 ac = vertex3 - vertex1;
            Vector3 ap = point - vertex1;

            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f)
            {
                result = vertex1; //Barycentric coordinates (1,0,0)
                return;
            }

            //Check if P in vertex region outside B
            Vector3 bp = point - vertex2;
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3)
            {
                result = vertex2; // barycentric coordinates (0,1,0)
                return;
            }

            //Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                float v = d1 / (d1 - d3);
                result = vertex1 + v * ab; //Barycentric coordinates (1-v,v,0)
                return;
            }

            //Check if P in vertex region outside C
            Vector3 cp = point - vertex3;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6)
            {
                result = vertex3; //Barycentric coordinates (0,0,1)
                return;
            }

            //Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                float w = d2 / (d2 - d6);
                result = vertex1 + w * ac; //Barycentric coordinates (1-w,0,w)
                return;
            }

            //Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                result = vertex2 + w * (vertex3 - vertex2); //Barycentric coordinates (0,1-w,w)
                return;
            }

            //P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float v2 = vb * denom;
            float w2 = vc * denom;
            result = vertex1 + ab * v2 + ac * w2; //= u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0f - v - w
        }

        /// <summary>
        /// Returns the closest point on a terrain
        /// </summary>
        /// <returns>The point on terrain.</returns>
        /// <param name="collider">Collider.</param>
        /// <param name="to">To.</param>
        /// <param name="radius">Radius.</param>
        public static Vector3 ClosestPointOnTerrain(TerrainCollider terrain, Vector3 point, float radius = 0.01f)
        {
            TerrainData terrainData = terrain.terrainData;

            Vector3 local = terrain.transform.InverseTransformPoint(point);

            // Calculate the size of each tile on the terrain horizontally and vertically
            float pixelSizeX = terrainData.size.x / (terrainData.heightmapResolution - 1);
            float pixelSizeZ = terrainData.size.z / (terrainData.heightmapResolution - 1);

            float percentZ = Mathf.Clamp01(local.z / terrainData.size.z);
            float percentX = Mathf.Clamp01(local.x / terrainData.size.x);

            float positionX = percentX * (terrainData.heightmapResolution - 1);
            float positionZ = percentZ * (terrainData.heightmapResolution - 1);

            // Calculate our position, in tiles, on the terrain
            int pixelX = Mathf.FloorToInt(positionX);
            int pixelZ = Mathf.FloorToInt(positionZ);

            // Calculate the distance from our point to the edge of the tile we are in
            float distanceX = (positionX - pixelX) * pixelSizeX;
            float distanceZ = (positionZ - pixelZ) * pixelSizeZ;

            // Find out how many tiles we are overlapping on the X plane
            float radiusExtentsLeftX = radius - distanceX;
            float radiusExtentsRightX = radius - (pixelSizeX - distanceX);

            int overlappedTilesXLeft = radiusExtentsLeftX > 0 ? Mathf.FloorToInt(radiusExtentsLeftX / pixelSizeX) + 1 : 0;
            int overlappedTilesXRight = radiusExtentsRightX > 0 ? Mathf.FloorToInt(radiusExtentsRightX / pixelSizeX) + 1 : 0;

            // Find out how many tiles we are overlapping on the Z plane
            float radiusExtentsLeftZ = radius - distanceZ;
            float radiusExtentsRightZ = radius - (pixelSizeZ - distanceZ);

            int overlappedTilesZLeft = radiusExtentsLeftZ > 0 ? Mathf.FloorToInt(radiusExtentsLeftZ / pixelSizeZ) + 1 : 0;
            int overlappedTilesZRight = radiusExtentsRightZ > 0 ? Mathf.FloorToInt(radiusExtentsRightZ / pixelSizeZ) + 1 : 0;

            // Retrieve the heights of the pixels we are testing against
            int startPositionX = pixelX - overlappedTilesXLeft;
            int startPositionZ = pixelZ - overlappedTilesZLeft;

            int numberOfXPixels = overlappedTilesXRight + overlappedTilesXLeft + 1;
            int numberOfZPixels = overlappedTilesZRight + overlappedTilesZLeft + 1;

            // Account for if we are off the terrain
            if (startPositionX < 0)
            {
                numberOfXPixels -= Mathf.Abs(startPositionX);
                startPositionX = 0;
            }

            if (startPositionZ < 0)
            {
                numberOfZPixels -= Mathf.Abs(startPositionZ);
                startPositionZ = 0;
            }

            if (startPositionX + numberOfXPixels + 1 > terrainData.heightmapResolution)
            {
                numberOfXPixels = terrainData.heightmapResolution - startPositionX - 1;
            }

            if (startPositionZ + numberOfZPixels + 1 > terrainData.heightmapResolution)
            {
                numberOfZPixels = terrainData.heightmapResolution - startPositionZ - 1;
            }

            // Retrieve the heights of the tile we are in and all overlapped tiles
            float[,] heights = terrainData.GetHeights(startPositionX, startPositionZ, numberOfXPixels + 1, numberOfZPixels + 1);

            // Pre-scale the heights data to be world-scale instead of 0...1
            for (int i = 0; i < numberOfXPixels + 1; i++)
            {
                for (int j = 0; j < numberOfZPixels + 1; j++)
                {
                    heights[j, i] *= terrainData.size.y;
                }
            }

            // Find the shortest distance to any triangle in the set gathered
            float shortestDistance = float.MaxValue;

            Vector3 shortestPoint = Vector3.zero;

            for (int x = 0; x < numberOfXPixels; x++)
            {
                for (int z = 0; z < numberOfZPixels; z++)
                {
                    // Build the set of points that creates the two triangles that form this tile
                    Vector3 a = new Vector3((startPositionX + x) * pixelSizeX, heights[z, x], (startPositionZ + z) * pixelSizeZ);
                    Vector3 b = new Vector3((startPositionX + x + 1) * pixelSizeX, heights[z, x + 1], (startPositionZ + z) * pixelSizeZ);
                    Vector3 c = new Vector3((startPositionX + x) * pixelSizeX, heights[z + 1, x], (startPositionZ + z + 1) * pixelSizeZ);
                    Vector3 d = new Vector3((startPositionX + x + 1) * pixelSizeX, heights[z + 1, x + 1], (startPositionZ + z + 1) * pixelSizeZ);

                    Vector3 nearest;

                    ClosestPointOnTriangleToPoint(ref a, ref d, ref c, ref local, out nearest);

                    float distance = (local - nearest).sqrMagnitude;

                    if (distance <= shortestDistance)
                    {
                        shortestDistance = distance;
                        shortestPoint = nearest;
                    }

                    ClosestPointOnTriangleToPoint(ref a, ref b, ref d, ref local, out nearest);

                    distance = (local - nearest).sqrMagnitude;

                    if (distance <= shortestDistance)
                    {
                        shortestDistance = distance;
                        shortestPoint = nearest;
                    }
                }
            }

            return terrain.transform.TransformPoint(shortestPoint);
        }


        static Dictionary<Mesh, KDTree> kdTreePool = new Dictionary<Mesh, KDTree>();
        /// <summary>
        /// Returns the closest point an a mesh. If cacheKDTree is set to true (default), the kdtree will be cached
        /// so it can be reused later which will greatly improve performance for the sake of RAM usage.
        /// </summary>
        /// <returns>The point on mesh.</returns>
        /// <param name="mesh">Mesh.</param>
        /// <param name="point">Point.</param>
        /// <param name="cacheKDTree">If set to <c>true</c> cache KD tree.</param>
        public static Vector3 ClosestPointOnMesh(MeshCollider mesh, Vector3 point, bool cacheKDTree = true){

            //If object has a mesh KDTree, use it
            MeshKDTree meshKDTree = mesh.GetComponent<MeshKDTree>();
            if(meshKDTree != null) return ClosestPointOnMeshKDTree(meshKDTree, point);

            //Check if we have a KDTree for the supplied mesh
            if(kdTreePool.ContainsKey(mesh.sharedMesh)){
                return kdTreePool[mesh.sharedMesh].FindNearestPointOnMesh(point, mesh.transform);
            }
            else {
                //If not, create new KDTree and find closest point
                KDTree kdTree = KDTree.CreateFromMesh(mesh.sharedMesh);
                Vector3 result = kdTree.FindNearestPointOnMesh(point, mesh.transform);
                //Cache kd tree for later use is user wants to cache it
                if(cacheKDTree){
                    kdTreePool.Add(mesh.sharedMesh, kdTree);
                } else {
                    kdTree = null;
                }
                return result;
            }

        }

        /// <summary>
        /// Returns the closest point on a mesh KDTree.
        /// </summary>
        /// <returns>The vertex on KD mesh.</returns>
        /// <param name="mesh">Mesh.</param>
        /// <param name="point">Point.</param>
        public static Vector3 ClosestPointOnMeshKDTree(MeshKDTree mesh, Vector3 point){
            return mesh.ClosestPointOnSurface(point);
        }

    }
}