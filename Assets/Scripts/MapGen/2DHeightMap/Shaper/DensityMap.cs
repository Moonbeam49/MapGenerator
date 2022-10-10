using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class that handles density values (amount of land in the given radius).
public class DensityMap
{
    public int[,] DensityArray;
    int densityRadiusPlusOne;
    public int MaxDensity = 0;
    public int LandValue = 0;

    //Creates a new density map, calculates land values.
    public DensityMap ()
    {
        DensityArray = new int[Options.MapSize, Options.MapSize];
        densityRadiusPlusOne = Options.DensityRadius + 1;
        MaxDensity = densityRadiusPlusOne * densityRadiusPlusOne;
        LandValue = MaxDensity + 1;
        PrepareDensityMap();
    }

    //Writing map borders as land to density data with the given radius.
    void PrepareDensityMap()
    {
        for (int x = 0, y = 0; x < Options.MapSize; x++)
        {
            AddLandAtPoint(new Dot(x, y));

            if (x == Options.MapSize - 1 && y != Options.MapSize - 1)
            {
                x = -1;
                y = Options.MapSize - 1;
            }
        }
        for (int x = 0, y = 0; y < Options.MapSize; y++)
        {
            AddLandAtPoint(new Dot(x, y));

            if (y == Options.MapSize - 1 && x != Options.MapSize - 1)
            {
                y = -1;
                x = Options.MapSize - 1;
            }
        }

    }

    //Method used to update density data with land generated at the exact point and given radius.
    public void AddLandAtPoint (Dot dot)
    {
        //Marking dot as land.
        DensityArray[dot.X, dot.Y] = LandValue;
        //Creating a box with a given size which will have its density values increased.
        Dot boxStart = new Dot(dot.X - Options.DensityRadius, dot.Y - Options.DensityRadius);
        Dot boxEnd = new Dot(dot.X + densityRadiusPlusOne, dot.Y + densityRadiusPlusOne);
        if (boxStart.X < 0)
        {
            boxStart.X = 0;
        }
        if (boxStart.Y < 0)
        {
            boxStart.Y = 0;
        }
        if (boxEnd.X > Options.MapSize)
        {
            boxEnd.X = Options.MapSize;
        }
        if (boxEnd.Y > Options.MapSize)
        {
            boxEnd.Y = Options.MapSize;
        }

        //Increasing density values.
        for (int densityX = boxStart.X; densityX < boxEnd.X; densityX++)
        {
            for (int densityY = boxStart.Y; densityY < boxEnd.Y; densityY++)
            {
                if (DensityArray[densityX, densityY] != LandValue)
                {
                    DensityArray[densityX, densityY]++;
                }
            }
        }
    }
}
