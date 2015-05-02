using UnityEngine;
using System.Collections;

public class TerrainCell {

	public float Altitude;
}

public class World {

	public int Width { get; private set; }
	public int Height { get; private set; }

	public int Seed { get; private set; }

	public TerrainCell[][] terrain;

	public World(int width, int height, int seed) {
	
		Width = width;
		Height = height;
		Seed = seed;

		terrain = new TerrainCell[width][];

		for (int i = 0; i < width; i++)
		{
			TerrainCell[] column = new TerrainCell[height];

			for (int j = 0; j < height; j++)
			{
				column[j] = new TerrainCell();
			}

			terrain[i] = column;
		}
	}

	public void Generate () {

		Random.seed = Seed;
		
		Vector3 offset = Random.insideUnitSphere * 1000;
		
		int sizeX = Width;
		int sizeY = Height;
		
		float radius = 2f;
		
		for (int i = 0; i < sizeX; i++)
		{
			float beta = (i / (float)sizeX) * Mathf.PI * 2;
			
			for (int j = 0; j < sizeY; j++)
			{
				float alpha = (j / (float)sizeY) * Mathf.PI;
				
				Vector3 pos = MathUtility.GetCartesianCoordinates(alpha,beta,radius) + offset;
				
				float noise = PerlinNoise.GetValue(pos.x, pos.y, pos.z);

				terrain[i][j].Altitude = noise;
			}
		}
	}
}
