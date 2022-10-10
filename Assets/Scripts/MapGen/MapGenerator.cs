using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//General class that handles map generation.
public class MapGenerator : MonoBehaviour
{
    public MapDrawer Drawer;

    //Settings used to convert basic 0 to 1 height to an actual mesh height.
    public float MeshHeightMultiplier;
    public AnimationCurve MeshHeightCurve;

    public float[,] CurrentHeightMap;

    public void GenerateMap()
    {
        //If seed value returns empty, default seedless random object will be used.
        if (!string.IsNullOrEmpty(Options.Seed))
        {
            Seed.seed = new System.Random(Options.SeedToInt());
        }

        //Passing settings values to the Noise class.
        PerlinNoise.Scale = Options.NoiseScale;
        PerlinNoise.Persisatnce = Options.NoisePersistance;
        PerlinNoise.Octaves = Options.NoiseOctaves;
        PerlinNoise.Lacunarity = Options.NoiseLacunarity;

        //Deciding which algorithm will be used for generation, and passing the required options. 
        if (Options.IsShaped)
        {
            CurrentHeightMap = ShapeCreator.CreateShapes();
        } 
        else
        {
            CurrentHeightMap = NoiseHeightMap.CreateHeightMap(Options.MapSize, Options.NoiseScale);
        }

        //Post processing for the newly generated map.
        CurrentHeightMap = NormalizeHeightMap(CurrentHeightMap);
        CurrentHeightMap = SmoothHeightMap(CurrentHeightMap,3);

        //Creating a 2d picture for the new map.
        Drawer.drawHeightMapAs2dTexture(CurrentHeightMap);

        //Creating a 3d mesh for the new map.
        if (Options.MeshRequested)
        {
            Drawer.UpdateMeshShader(MinMeshHeight, MaxMeshHeight);
            Drawer.CreateMesh(MeshGenerator.GenerateTerrainMesh(CurrentHeightMap, MeshHeightMultiplier, MeshHeightCurve));
        }
    }

    //Function used to display heightmaps loaded from storage.
    public void LoadHeightMap(float[,] hMap)
    {
        CurrentHeightMap = hMap;
        Options.MapSize = CurrentHeightMap.GetLength(0);
        Drawer.drawHeightMapAs2dTexture(CurrentHeightMap);
        if (Options.MeshRequested)
        {
            Drawer.UpdateMeshShader(MinMeshHeight, MaxMeshHeight);
            Drawer.CreateMesh(MeshGenerator.GenerateTerrainMesh(CurrentHeightMap, MeshHeightMultiplier, MeshHeightCurve));
        }
    }

    //Function used to make sure that all of the height values are within range from 0 to 1.
    float[,] NormalizeHeightMap(float[,] hMap)
    {
        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        for (int y = 0; y < hMap.GetLength(1); y++)
        {
            for (int x = 0; x < hMap.GetLength(0); x++)
            {
                if (hMap[x, y] > maxHeight)
                {
                    maxHeight = hMap[x, y];
                }
                if (hMap[x, y] < minHeight)
                {
                    minHeight = hMap[x, y];
                }
            }
        }

        for (int y = 0; y < hMap.GetLength(1); y++)
        {
            for (int x = 0; x < hMap.GetLength(0); x++)
            {
                hMap[x, y] = Mathf.InverseLerp(minHeight, maxHeight, hMap[x, y]);
            }
        }

        return hMap;
    }

    //Equalizing height values between neighboring tiles to make sure we remove all of the weird height changes.
    float[,] SmoothHeightMap(float[,] hMap, int passes)
    {
        for (int i = 0; i < passes; i++)
        {
            for (int y = 0; y < hMap.GetLength(1); y++)
            {
                for (int x = 0; x < hMap.GetLength(0); x++)
                {
                    int tilesAdded = 1;
                    if (x + 1 != hMap.GetLength(0))
                    {
                        hMap[x, y] += hMap[x + 1, y];
                        tilesAdded++;
                    }
                    if (y + 1 != hMap.GetLength(1))
                    {
                        hMap[x, y] += hMap[x, y + 1];
                        tilesAdded++;
                    }
                    if (x > 0)
                    {
                        hMap[x, y] += hMap[x - 1, y];
                        tilesAdded++;
                    }
                    if (y > 0)
                    {
                        hMap[x, y] += hMap[x, y - 1];
                        tilesAdded++;
                    }
                    hMap[x, y] /= tilesAdded;
                }
            }
        }
        return hMap;
    }

    //Returns theoretical minimum vertice height for the generated mesh with the current settings.
    public float MinMeshHeight
    {
        get
        {
            return MeshHeightMultiplier * MeshHeightCurve.Evaluate(0);
        }
    }

    //Returns theoretical maximum vertice height for the generated mesh with the current settings.
    public float MaxMeshHeight
    {
        get
        {
            return MeshHeightMultiplier * MeshHeightCurve.Evaluate(1);
        }
    }
}
