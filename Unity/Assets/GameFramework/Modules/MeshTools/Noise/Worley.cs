using UnityEngine;
using System.Collections;

namespace MeshTools.Noise{
	public class Worley{
			
		private static float[] DistanceArray = new float[3];
		
		public static int DistanceFunction = 0;
		public static int CombineDistanceFunction = 0;
		public static int Seed = 3221;
		
		static uint OFFSET_BASIS = 2166136261;
		static uint FNV_PRIME = 16777619;

		delegate float CombinerFunc(float[] array);

		static float CombinerFunc1(float[] array)
		{
			return array[0];
		}
		
		static float CombinerFunc2(float[] array)
		{
			return array[1] - array[0];
		}
		
		static float CombinerFunc3(float[] array)
		{
			return array[2] - array[0];
		}

		delegate float DistanceFunc(Vector3 p1, Vector3 p2);

		static float EuclidianDistanceFunc(Vector3 p1, Vector3 p2)
		{
			return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y)
				+ (p1.z - p2.z) * (p1.z - p2.z);
		}
		
		static float ManhattanDistanceFunc(Vector3 p1, Vector3 p2)
		{
			return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y) + Mathf.Abs(p1.z - p2.z);
		}
		
		static float ChebyshevDistanceFunc(Vector3 p1, Vector3 p2)
		{
			Vector3 diff = new Vector3(p1.x - p2.x, p1.y - p2.y,p1.z - p2.z);
			return Mathf.Max(Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y)), Mathf.Abs(diff.z));
		}

		static long probLookup(long value)
		{
			value = value & 0xffffffff;
			if (value < 393325350) return 1 & 0xffffffff;
			if (value < 1022645910) return 2 & 0xffffffff;
			if (value < 1861739990) return 3 & 0xffffffff;
			if (value < 2700834071) return 4 & 0xffffffff;
			if (value < 3372109335) return 5 & 0xffffffff;
			if (value < 3819626178) return 6 & 0xffffffff;
			if (value < 4075350088) return 7 & 0xffffffff;
			if (value < 4203212043) return 8 & 0xffffffff;
			return 9 & 0xffffffff;
		}
		
		static void insert(float[] arr, float value)
		{
			float temp;
			for (var i = arr.Length - 1; i >= 0; i--)
			{
				if (value > arr[i]) break;
				temp = arr[i];
				arr[i] = value;
				if (i + 1 < arr.Length) arr[i + 1] = temp;
			}
		}
		
		static uint lcgRandom(uint lastValue)
		{
			return (uint)((((1103515245 & 0xffffffff) * lastValue + (12345 & 0xffffffff)) % 0x100000000) & 0xffffffff);
		}
		
		
		static uint hash(long i, long j, long k)
		{
			return (uint)((((((OFFSET_BASIS ^ (i & 0xffffffff)) * FNV_PRIME) ^ (j & 0xffffffff)) * FNV_PRIME)
			         ^ (k & 0xffffffff)) * FNV_PRIME) & 0xffffffff;
		}

		static uint hash(long i, long j)
		{
			return (uint)((((OFFSET_BASIS ^ (i & 0xffffffff)) * FNV_PRIME) ^ (j & 0xffffffff)) * FNV_PRIME);
		}
		
		static float noise3d(Vector3 input, CombinerFunc combinerFunc, DistanceFunc distanceFunc )
		{
//			var value = 0;
			
			uint lastRandom;
			uint numberFeaturePoints;
			Vector3 randomDiff = new Vector3(0,0,0);
			Vector3 featurePoint = new Vector3(0,0,0);
			
			int cubeX, cubeY, cubeZ;
			
			for (var i = 0; i < DistanceArray.Length; i++)
			{
				DistanceArray[i] = 6666;
			}
			
			var evalCubeX = (int)(Mathf.Floor(input.x));
			var evalCubeY = (int)(Mathf.Floor(input.y));
			var evalCubeZ = (int)(Mathf.Floor(input.z));
			
			for (var i = -1; i < 2; ++i)
				for (var j = -1; j < 2; ++j)
					for (var k = -1; k < 2; ++k)
				{
					cubeX = evalCubeX + i;
					cubeY = evalCubeY + j;
					cubeZ = evalCubeZ + k;
					
					//2. Generate a reproducible random number generator for the cube
					lastRandom = lcgRandom(hash((cubeX + Seed) & 0xffffffff, (cubeY) & 0xffffffff, (cubeZ) & 0xffffffff));
					//3. Determine how many feature points are in the cube
					numberFeaturePoints = (uint)probLookup(lastRandom);
					//4. Randomly place the feature points in the cube
					for (var l = 0; l < numberFeaturePoints; ++l)
					{
						lastRandom = lcgRandom(lastRandom);
						randomDiff.x = (float)lastRandom / 0x100000000;
						
						lastRandom = lcgRandom(lastRandom);
						randomDiff.y = (float)lastRandom / 0x100000000;
						
						lastRandom = lcgRandom(lastRandom);
						randomDiff.z = (float)lastRandom / 0x100000000;
						
						featurePoint = new Vector3 ( randomDiff.x + cubeX, randomDiff.y + cubeY, randomDiff.z + cubeZ );
						
						//5. Find the feature point closest to the evaluation point. 
						//This is done by inserting the distances to the feature points into a sorted list
						float v = distanceFunc(input, featurePoint);
						insert(DistanceArray, v);
					}
					//6. Check the neighboring cubes to ensure their are no closer evaluation points.
					// This is done by repeating steps 1 through 5 above for each neighboring cube
				}
			
			var color = combinerFunc(DistanceArray);
			if (color < 0) color = 0;
			if (color > 1) color = 1;
			
			return color;
		}

		static float noise2d(Vector2 input, CombinerFunc combinerFunc, DistanceFunc distanceFunc )
		{
			uint lastRandom;
			uint numberFeaturePoints;
			Vector2 randomDiff = new Vector2(0,0);
			Vector2 featurePoint = new Vector2(0,0);
			
			int cubeX, cubeY;
			
			for (var i = 0; i < DistanceArray.Length; i++)
			{
				DistanceArray[i] = 6666;
			}
			
			var evalCubeX = (int)(Mathf.Floor(input.x));
			var evalCubeY = (int)(Mathf.Floor(input.y));
			
			for (var i = -1; i < 2; ++i)
				for (var j = -1; j < 2; ++j)
				{
					cubeX = evalCubeX + i;
					cubeY = evalCubeY + j;
					
					//2. Generate a reproducible random number generator for the cube
					lastRandom = lcgRandom(hash((cubeX + Seed) & 0xffffffff, (cubeY) & 0xffffffff));
					//3. Determine how many feature points are in the cube
					numberFeaturePoints = (uint)probLookup(lastRandom);
					//4. Randomly place the feature points in the cube
					for (var l = 0; l < numberFeaturePoints; ++l)
					{
						lastRandom = lcgRandom(lastRandom);
						randomDiff.x = (float)lastRandom / 0x100000000;
						
						lastRandom = lcgRandom(lastRandom);
						randomDiff.y = (float)lastRandom / 0x100000000;
						
						featurePoint = new Vector2 ( randomDiff.x + cubeX, randomDiff.y + cubeY);
						
						//5. Find the feature point closest to the evaluation point. 
						//This is done by inserting the distances to the feature points into a sorted list
						float v = distanceFunc(input, featurePoint);
						insert(DistanceArray, v);
					}
					//6. Check the neighboring cubes to ensure their are no closer evaluation points.
					// This is done by repeating steps 1 through 5 above for each neighboring cube
				}
			
			var color = combinerFunc(DistanceArray);
			if (color < 0) color = 0;
			if (color > 1) color = 1;
			
			return color;





			/*
			//Declare some values for later use
			uint lastRandom, numberFeaturePoints;
			Vector2 randomDiff, featurePoint;
			int cubeX, cubeY;
			
			float[] distanceArray = new float[3];
			
			//Initialize values in distance array to large values
			for (int i = 0; i < distanceArray.Length; i++)
				distanceArray[i] = 6666;
			
			//1. Determine which cube the evaluation point is in
			int evalCubeX = (int)Mathf.Floor(input.x);
			int evalCubeY = (int)Mathf.Floor(input.y);
			
			for (int i = -1; i < 2; ++i)
			{
				for (int j = -1; j < 2; ++j)
				{
					cubeX = evalCubeX + i;
					cubeY = evalCubeY + j;
					
					//2. Generate a reproducible random number generator for the cube
					lastRandom = lcgRandom(hash((uint)(cubeX + inSeed), (uint)(cubeY)));
					
					//3. Determine how many feature points are in the cube
					numberFeaturePoints = probLookup(lastRandom);
					//4. Randomly place the feature points in the cube
					for (uint l = 0; l < numberFeaturePoints; ++l)
					{
						lastRandom = lcgRandom(lastRandom);
						randomDiff.x = (float)lastRandom / 0x100000000;
						
						lastRandom = lcgRandom(lastRandom);
						randomDiff.y = (float)lastRandom / 0x100000000;
						
						featurePoint = new Vector2(randomDiff.x + (float)cubeX, randomDiff.y + (float)cubeY);
						
						//5. Find the feature point closest to the evaluation point. 
						//This is done by inserting the distances to the feature points into a sorted list
						insert(distanceArray, DistanceFunc2(input, featurePoint));
					}
					//6. Check the neighboring cubes to ensure their are no closer evaluation points.
					// This is done by repeating steps 1 through 5 above for each neighboring cube
				}
			}
			
			return Mathf.Clamp01(CombineFunc(distanceArray));
			*/
		}









		//Access functions


		public enum CombinerFuncEnum{
			D1,
			D2MinusD1,
			D3MinusD1
		}
		static CombinerFuncEnum combinerFuncEnum = CombinerFuncEnum.D1;
		static CombinerFunc cFunction = CombinerFunc1;
		public static CombinerFuncEnum combinerFunction {
			get{
				return combinerFuncEnum;
			}
			
			set{
				if(value == combinerFuncEnum)
					return;
				
				switch (value)
				{
				case CombinerFuncEnum.D1:
					cFunction = CombinerFunc1;// i => i[0];
					break;
				case CombinerFuncEnum.D2MinusD1:
					cFunction = CombinerFunc2;//i => i[1] - i[0];
					break;
				case CombinerFuncEnum.D3MinusD1:
					cFunction = CombinerFunc3;//i => i[2] - i[0];
					break;
				}
				combinerFuncEnum = value;
			}
		}
		
		public enum DistanceFuncEnum{
			Euclidean,
			Manhattan,
			Chebyshev
		}
		static DistanceFuncEnum distanceFuncEnum = DistanceFuncEnum.Euclidean;
		static DistanceFunc dFunction = EuclidianDistanceFunc;
		public static DistanceFuncEnum distanceFunction {
			get{
				return distanceFuncEnum;
			}
			set{
				if(value == distanceFuncEnum)
					return;
				
				switch (value)
				{
				case DistanceFuncEnum.Euclidean:
					dFunction = EuclidianDistanceFunc;
					break;
				case DistanceFuncEnum.Manhattan:
					dFunction = ManhattanDistanceFunc;
					break;
				case DistanceFuncEnum.Chebyshev:
					dFunction = ChebyshevDistanceFunc;
					break;
				}
				distanceFuncEnum = value;
			}
		}


		public static float Get1D(float x, CombinerFuncEnum combinerFunc, DistanceFuncEnum distanceFunc)
		{
			//return noise(new Vector3(x,0,0));
			return Get3D(x, 0, 0, combinerFunc, distanceFunc);
		}

		//distance function and combiner function can be set by Worley.distanceFunction & Worley.distanceFunction.combinerFunction
		//Or use Get1D(float x CombinerFuncEnum combinerFunc, DistanceFuncEnum distanceFunc)
		public static float Get1D(float x)
		{
			//return noise(new Vector3(x,0,0));
			return Get3D(x, 0, 0, combinerFunction, distanceFunction);
		}
		
		public static float Get2D(float x, float y, CombinerFuncEnum combinerFunc, DistanceFuncEnum distanceFunc)
		{
			//return noise(new Vector3(x,y,0));
			return Get3D(x, y, 0, combinerFunc, distanceFunc);
		}

		//distance function and combiner function can be set by Worley.distanceFunction & Worley.distanceFunction.combinerFunction
		//Or use Get2D(float x, float y, CombinerFuncEnum combinerFunc, DistanceFuncEnum distanceFunc)
		public static float Get2D(float x, float y)
		{
			//return noise(new Vector3(x,y,0));
			return Get3D(x, y, 0, combinerFunction, distanceFunction);
		}
		
		public static float Get3D(float x, float y, float z, CombinerFuncEnum combinerFunc, DistanceFuncEnum distanceFunc)
		{
			DistanceFunc _dFunc = null;
			switch (distanceFunc)
			{
			case DistanceFuncEnum.Euclidean:
				_dFunc = EuclidianDistanceFunc;
				break;
			case DistanceFuncEnum.Manhattan:
				_dFunc = ManhattanDistanceFunc;
				break;
			case DistanceFuncEnum.Chebyshev:
				_dFunc = ChebyshevDistanceFunc;
				break;
			}

			CombinerFunc _cFunc = null;
			switch (combinerFunc)
			{
			case CombinerFuncEnum.D1:
				_cFunc = CombinerFunc1;// i => i[0];
				break;
			case CombinerFuncEnum.D2MinusD1:
				_cFunc = CombinerFunc2;//i => i[1] - i[0];
				break;
			case CombinerFuncEnum.D3MinusD1:
				_cFunc = CombinerFunc3;//i => i[2] - i[0];
				break;
			}

			return noise3d(new Vector3(x,y,z), _cFunc, _dFunc);
		}

		//distance function and combiner function can be set by Worley.distanceFunction & Worley.distanceFunction.combinerFunction
		//Or use Get3D(float x, float y, float z, CombinerFuncEnum combinerFunc, DistanceFuncEnum distanceFunc)
		public static float Get3D(float x, float y, float z)
		{
			return noise3d(new Vector3(x,y,z), cFunction, dFunction);
		}
	}
}
