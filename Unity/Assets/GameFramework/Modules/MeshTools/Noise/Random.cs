using UnityEngine;
using System.Collections;

namespace MeshTools.Noise{
	public class Random{
		static float[] RandomArray1D;
		static float[,] RandomArray2D;
		static float[,,] RandomArray3D;
		
		private static int arraySize = 256;
		
//		public RandomNoise()
//		{
//			arraySize = 256; // Default Value
//		}
		
		public static int ArraySize
		{
			get { return arraySize; }
			set { arraySize = value; }
		}
		
		
		private static void InitArray1D()
		{
			RandomArray1D = new float[ArraySize];
			System.Random r = new System.Random();
			for (int i = 0; i < ArraySize; i++)
			{
				RandomArray1D[i] = (float)r.NextDouble();
			}
		}
		
		private static void InitArray2D()
		{
			RandomArray2D = new float[ArraySize, ArraySize];
			System.Random r = new System.Random();
			for (int i = 0; i < ArraySize; i++)
			{
				for (int j = 0; j < ArraySize; j++)
				{
					RandomArray2D[i, j] = (float)r.NextDouble();
				}
			}
		}

		private static void InitArray3D()
		{
			RandomArray3D = new float[ArraySize, ArraySize, ArraySize];
			System.Random r = new System.Random();
			for (int i = 0; i < ArraySize; i++)
			{
				for (int j = 0; j < ArraySize; j++)
				{
					for (int k = 0; k < ArraySize; k++)
					{
						RandomArray3D[i, j, k] = (float)r.NextDouble();
					}
				}
			}
		}
		
		public static float Get1D(float x)
		{
			if (RandomArray1D == null) InitArray1D();

			//put in range -arrySize-arraySize
			x -= arraySize * (int)(x/arraySize);
			//put in range 0-arraySize
			if(x < 0)
				x = arraySize-x;

			int ix = (int)x;
			return RandomArray1D[ix % (arraySize - 1)];
		}
		
		public static float Get2D(float x, float y)
		{
			if (RandomArray2D == null) InitArray2D();

			//put in range -arrySize-arraySize
			x -= arraySize * (int)(x/arraySize);
			y -= arraySize * (int)(y/arraySize);
			//put in range 0-arraySize
			if(x < 0)
				x = arraySize-x;
			if(y < 0)
				y = arraySize-y;

			int ix = (int)x;
			int iy = (int)y;
			return RandomArray2D[ix % (arraySize - 1), iy % (arraySize - 1)];
			
		}
		
		public static float Get3D(float x, float y, float z)
		{
			if (RandomArray3D == null) InitArray3D();


			//put in range -arrySize-arraySize
			x -= arraySize * (int)(x/arraySize);
			y -= arraySize * (int)(y/arraySize);
			z -= arraySize * (int)(z/arraySize);
			//put in range 0-arraySize
			if(x < 0)
				x = arraySize-x;
			if(y < 0)
				y = arraySize-y;
			if(z < 0)
				z = arraySize-z;

			int ix = (int)x;
			int iy = (int)y;
			int iz = (int)z;

			return RandomArray3D[ix % (arraySize - 1), iy % (arraySize - 1), iz % (arraySize - 1)];
		}
	}
}
