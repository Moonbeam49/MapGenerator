using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class used to represent boxes used in shape creation as outer limitations.
public class Box
{
    public Dot Start, End;
    public int XSize { get => End.X - Start.X; }
    public int YSize { get => End.Y - Start.Y; }

    public int BiggestSide
    {
        get
        {
            if (XSize > YSize)
            {
                return XSize;
            }
            else
            {
                return YSize;
            }
        }
    }

    public int SmallestSide
    {
        get
        {
            if (XSize > YSize)
            {
                return YSize;
            }
            else
            {
                return XSize;
            }
        }
    }

    public int Perimeter
    {
        get => 2 * (XSize + YSize);
    }

    public int Area
    {
        get => XSize * YSize;
    }

    //Array used for collision checks.
    public int[,] TempLand;

    public Dot Center
    {
        get => new Dot(Start.X + XSize / 2, Start.Y + YSize / 2);
    }

    public Box(Dot start, Dot end)
    {
        Start = start;
        End = end;
    }

    public Box(Dot start, Dot end, bool createTempLand)
    {
        Start = start;
        End = end;
        if (createTempLand)
        {
            //Creates the collision array and writes box edges into it.
            TempLand = new int[XSize + 1, YSize + 1];
            for (int x = 0; x < XSize; x++)
            {
                for (int y = 0; y < YSize; y++)
                {
                    if (x == 1 || x == XSize - 1 || y == 1 || y == YSize - 1) TempLand[x, y] = 2;

                    else if (ShapeCreator.TempLand[x + Start.X, y + Start.Y] != 0)
                    {
                        TempLand[x, y] = 2;
                    }
                }
            }
        }
    }

    public bool isHorisontal
    {
        get => XSize > YSize;
    }
}
