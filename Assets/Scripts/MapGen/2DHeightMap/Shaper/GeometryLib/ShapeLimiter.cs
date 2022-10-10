using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Class that represents inner limiting box for shape creation, that also provides information on the current state of the shape.
class ShapeLimiter
{
    public Box OuterBox;
    public Dot Start, End;
    public bool IsHorizontal;
    public Line Axis;
    public int YRange, XRange;

    //Creates a new shape limiter as a shrunken version of a given outer box.
    public ShapeLimiter(Box box, float borderPercentage)
    {
        int middlePoint;
        int limit1;
        int limit2;
        OuterBox = box;
        //Calculating limiter dimensions based on the box parameters.
        if (box.isHorisontal)
        {
            middlePoint = box.YSize / 2 + box.Start.Y;
            int xOffset = (int)(box.XSize * (borderPercentage / 100) / 2);
            int yOffset = (int)(box.YSize * ((borderPercentage / 2) / 100)) / 2;
            if (box.XSize / 2 - xOffset < 10) xOffset = box.XSize / 2 - 10;
            if (box.YSize / 2 - yOffset < 10) yOffset = box.YSize / 2 - 10;
            limit1 = box.Start.X + xOffset;
            limit2 = box.End.X - xOffset;
            IsHorizontal = true;
            Start = new Dot(limit1, middlePoint - yOffset);
            End = new Dot(limit2, middlePoint + yOffset);
            //Ranges represent the amount of dots that are available for shape creation based on the current side.
            YRange = box.End.Y - End.Y;
            XRange = box.End.X - End.X;
            Axis = new Line(new Dot(Start.X, End.Y), End);
        }
        else
        {
            middlePoint = box.XSize / 2 + box.Start.X;
            int yOffset = (int)(box.YSize * (borderPercentage / 100) / 2);
            int xOffset = (int)(box.XSize * ((borderPercentage / 2) / 100)) / 2;
            if (box.XSize / 2 - xOffset < 10) xOffset = box.XSize / 2 - 10;
            if (box.YSize / 2 - yOffset < 10) yOffset = box.YSize / 2 - 10;
            limit1 = box.Start.Y + yOffset;
            limit2 = box.End.Y - yOffset;
            IsHorizontal = false;
            Start = new Dot(middlePoint - xOffset, limit1);
            End = new Dot(middlePoint + xOffset, limit2);
            YRange = box.End.Y - End.Y;
            XRange = box.End.X - End.X;
            Axis = new Line(new Dot(End.X, Start.Y), End);
        }

        //Writes created shape limiter to the collision array of the box.
        for (int x = XRange; x <= OuterBox.XSize - XRange; x++)
        {
            OuterBox.TempLand[x, YRange] = 2;
            OuterBox.TempLand[x, OuterBox.YSize - YRange] = 2;
        }
        for (int y = YRange; y <= OuterBox.YSize - YRange; y++)
        {
            OuterBox.TempLand[XRange, y] = 2;
            OuterBox.TempLand[OuterBox.XSize - XRange, y] = 2;
        }
    }

    //Returns distance to this limiter from the given dot.
    public int GetDistToInnerLim(Dot dot, int currentSide)
    {
        switch (currentSide)
        {
            case 0:
                return Start.X - dot.X;
            case 1:
                return dot.Y - End.Y;
            case 2:
                return dot.X - End.X;
            default:
                return Start.Y - dot.Y;
        }
    }

    //Returns the fill value (distance between the inner and the outer limits) as a float with values in range from 0 to 1.  
    public float GetFill(Dot dot, int currentSide)
    {
        switch (currentSide)
        {
            case 0:
                return (Start.X - dot.X) / (float)XRange;
            case 1:
                return (dot.Y - End.Y) / (float)YRange;
            case 2:
                return (dot.X - End.X) / (float)XRange;
            default:
                return (Start.Y - dot.Y) / (float)YRange;
        }
    }

    //Returns precise angle for the next dot based on the fill target.
    public float GetAngleToOptimalFill(Dot origin, int currentSide, float fillTarget)
    {
        int x = 0, y = 0;
        switch (currentSide)
        {
            case 0:
                y = origin.Y + 4;
                x = Start.X - (int)(XRange * fillTarget);
                return origin.AngleTo(new Dot(x, y));
            case 1:
                y = End.Y + (int)(YRange * fillTarget);
                x = origin.X + 4;
                return origin.AngleTo(new Dot(x, y));
            case 2:
                y = origin.Y - 4;
                x = End.X + (int)(XRange * fillTarget);
                return origin.AngleTo(new Dot(x, y));
            default:
                y = Start.Y - (int)(YRange * fillTarget);
                x = origin.X - 4;
                return origin.AngleTo(new Dot(x, y));
        }
    }

    //Returns approximate shape completion status as float with values from 0 to 1.
    public float PercentageTravelled(Dot dot, int currentSide, int startingSide, bool returnedToStart)
    {
        int totalDist = 0;
        int sidesTravelled = currentSide - startingSide;
        if (sidesTravelled == -1) sidesTravelled = 3;
        if (sidesTravelled == 0 && returnedToStart) sidesTravelled = 4;
        for (int i = 0; i < sidesTravelled; i++)
        {
            if (i == 0) totalDist += (int)(OuterBox.BiggestSide * 0.35f);
            else if (i % 2 == 1) totalDist += OuterBox.SmallestSide;
            else totalDist += OuterBox.BiggestSide;
        }
        int curSideDist = 0;
        if (currentSide == startingSide && !returnedToStart)
        {
            switch (currentSide)
            {
                case 0:
                    curSideDist = dot.Y - (OuterBox.Start.Y + (int)(OuterBox.YSize * 0.65f));
                    break;
                case 1:
                    curSideDist = dot.X - (OuterBox.Start.X + (int)(OuterBox.XSize * 0.65f));
                    break;
            }
        }
        else
        {
            switch (currentSide)
            {
                case 0:
                    curSideDist = dot.Y - OuterBox.Start.Y;
                    break;
                case 1:
                    curSideDist = dot.X - OuterBox.Start.X;
                    break;
                case 2:
                    curSideDist = OuterBox.End.Y - dot.Y;
                    break;
                case 3:
                    curSideDist = OuterBox.End.X - dot.X;
                    break;
            }
        }
        totalDist += curSideDist;
        if (totalDist == 0)
        {
            return 0;
        }
        else
        {
            return (float)totalDist / OuterBox.Perimeter;
        }
    }
}
