using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class used to generate perlin noise heightmap.
public static class NoiseHeightMap
{
    public static float[,] CreateHeightMap(int size, float scale)
    {
        float[,] HeightMap = new float[size, size];
        //Offset is used to create unique maps based on seed values.
        int xOffset = Seed.seed.Next(-Options.NoiseMapMaxOffset, Options.NoiseMapMaxOffset);
        int yOffset = Seed.seed.Next(-Options.NoiseMapMaxOffset, Options.NoiseMapMaxOffset);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                HeightMap[x, y] = PerlinNoise.TerrainPerlinValue(x+xOffset,y+yOffset);
            }
        }
        return HeightMap;
    }
}
