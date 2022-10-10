using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

//Class that stores all publicly available options.
public static class Options 
{
    //Options that can be changed at runtime.
    public static string Seed;
    public static int MapSize;
    public static bool IsShaped;
    public static bool MeshRequested;
    public static float NoiseScale;
    public static int NoiseOctaves;
    public static float NoisePersistance;
    public static float NoiseLacunarity;
    public static int ShapeCount;
    public static float ShapeSize;
    public static int DensityRadius;
    public static int PContourDist;
    public static float FillThreshold;

    //Multiple constants that are used by the map generator.
    public const float MinFill = 0.5f;
    public const float MaxFill = 0.9f;
    public const int MaxShapeRotationAngle = 30;
    public const int InnerBoxShrinkPercent = 40;
    public const int BaseAngleRangeForDots = 180;
    public const int MaxShapeClosingAngle = 15;
    public const int PrefferableMaxAngleChange = 45;
    public const int CheckStepForAngleRanges = 5;
    public const int LineBaseLength = 4;
    public const int DotsToCloseShape = 16;
    public const int MaxDotsForShape = 10000;
    public const int MaxContourDots = 100000;
    public const int MaxDotsToCheckForContour = 20;
    public const int MaxSurfaceDotsForShape = 5000000;
    public const float WaterHeightCutoff = 0.1f;
    public const int GridBorderSize = 32;
    public const int MinimumPrefferedBoxSize = 64;
    public const float MaxShapeSizeRatio = 2f;
    public const int NoiseMapMaxOffset = 500;
    public const float WaterMeshHeight = 2.18f;
    public const int MeshChunkMaxSize = 128;

    //Gets a byte value of every char in string, then sums them up and returns an integer used by the seed object.
    public static int SeedToInt()
    {
        byte[] bytesFromString = Encoding.UTF8.GetBytes(Seed);
        int newSeed = 0;
        for (int i = 0; i < bytesFromString.Length; i++)
        {
            newSeed += bytesFromString[i];
        }
        return newSeed;
    }

    //Converts current options into a single string for storage.
    public static string ConvertOptionsToString()
    {
        string options = "";
        options += Seed + "\n";
        options += MapSize + "\n";
        options += IsShaped + "\n";
        options += MeshRequested + "\n";
        options += NoiseScale + "\n";
        options += NoiseOctaves + "\n";
        options += NoisePersistance + "\n";
        options += NoiseLacunarity + "\n";
        options += ShapeCount + "\n";
        options += ShapeSize + "\n";
        options += DensityRadius + "\n";
        options += PContourDist + "\n";
        options+= FillThreshold + "\n";
        return options;
    }

    //Reads a loaded string and applies all of the loaded data.
    public static void GetOptionsFromString(string options)
    {
        string[] optionsArr = options.Split('\n');
        Seed = optionsArr[0];
        MapSize = int.Parse(optionsArr[1]);
        IsShaped = bool.Parse(optionsArr[2]);
        MeshRequested = bool.Parse(optionsArr[3]);
        NoiseScale = float.Parse(optionsArr[4]);
        NoiseOctaves = int.Parse(optionsArr[5]);
        NoisePersistance = float.Parse(optionsArr[6]);
        NoiseLacunarity = float.Parse(optionsArr[7]);
        ShapeCount = int.Parse(optionsArr[8]);
        ShapeSize = float.Parse(optionsArr[9]);
        DensityRadius = int.Parse(optionsArr[10]);
        PContourDist = int.Parse(optionsArr[11]);
        FillThreshold = float.Parse(optionsArr[12]);
    }
}
