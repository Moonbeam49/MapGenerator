using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class used for noise sampling.
public static class PerlinNoise
{
    //Scale at which the noise will be sampled.
    public static float Scale;
    //Amount of passes the sampler will make.
    public static int Octaves;
    //Defines how hard each pass after the first one will affect the end result.
    public static float Persisatnce;
    //Defines distance between points during sampling, higher values return more uneven terrain.
    public static float Lacunarity;

    //Returns a perlin noise value using all of the above parameters. Used for terrain generation.
    public static float TerrainPerlinValue(int x, int y)
    {
        //Defines the strength of each individual pass.
        float amplitude = 1;
        //Defines the distance passed with each sampled dot.
        float frequency = 1;
        float endValue = 0;

        for (int i = 0; i < Octaves; i++)
        {
            float sampleX = x / Scale * frequency;
            float sampleY = y / Scale * frequency;
            float pval = Mathf.PerlinNoise(sampleX, sampleY);
            endValue += pval * amplitude;

            amplitude *= Persisatnce;
            frequency *= Lacunarity;
        }

        return endValue;
    }

    //Returns a perlin noise value only affected by scale. Used in finished contour creation.
    public static float DotPerlinValue(int x, int y)
    {
        float sampleX = x / Scale;
        float sampleY = y / Scale;
        return Mathf.PerlinNoise(sampleX, sampleY);
    }
}
