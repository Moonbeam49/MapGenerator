using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PreMapGrid
{
    int gridSideSize;
    List<Box> createdBoxes = new List<Box>();
    int boxesRequested = 0;

    //Class that creates and handles a grid of boxes used for shape creation.
    public PreMapGrid()
    {
        //Checking the amount of boxes current map size can support.
        gridSideSize = Options.MapSize-Options.GridBorderSize*2;
        int mapArea = Options.MapSize * Options.MapSize;
        int maxShapeCount = mapArea / (Options.MinimumPrefferedBoxSize * Options.MinimumPrefferedBoxSize);
        if (Options.ShapeCount > maxShapeCount)
        {
            Options.ShapeCount = maxShapeCount;
        }

        //Splitting map into basic grid based on the shape count, determining which rows will get extra boxes.
        Box[] initBoxes = new Box[Options.ShapeCount];
        float boxesPerSide = Mathf.Sqrt(Options.ShapeCount);
        int xBoxes = Mathf.CeilToInt(boxesPerSide);
        int yBoxes = Mathf.FloorToInt(boxesPerSide);
        int extrasPerXRow = 0;
        int extraRows = 0;
        if (xBoxes*yBoxes != Options.ShapeCount)
        {
            int extraBoxCount = Options.ShapeCount - xBoxes * yBoxes;
            if (extraBoxCount > 0)
            {
                extrasPerXRow = Mathf.CeilToInt((float)extraBoxCount / yBoxes);
            }
            else
            {
                extrasPerXRow = Mathf.FloorToInt((float)extraBoxCount / yBoxes);
            }
            extraRows = extraBoxCount / extrasPerXRow;
        }
        int extraBoxesStartingRow = Mathf.CeilToInt((yBoxes - extraRows) / 2f);
        int extraRowsDone = 0;
        int ySize = gridSideSize / yBoxes;
        int currentBox = 0;
        for (int y = 0; y < yBoxes; y++)
        {
            int thisX = xBoxes;
            if (y >= extraBoxesStartingRow - 1 && extraRowsDone < extraRows)
            {
                thisX += extrasPerXRow;
            }
            int xSize = (gridSideSize / thisX);
            for (int x = 0; x < thisX; x++)
            {
                Dot start = new Dot(Options.GridBorderSize + (xSize * x), Options.GridBorderSize + (ySize * y));
                Dot end = new Dot(Options.GridBorderSize + (xSize * (x + 1)), Options.GridBorderSize + (ySize * (y + 1)));
                initBoxes[currentBox] = new Box(start, end, true);
                currentBox++;
            }
            if (thisX != xBoxes)
            {
                extraRowsDone++;
            }
        }


        //Resizing and moving basic boxes based on the currently selected land percentage.
        int totalArea = gridSideSize * gridSideSize;
        int landArea = Mathf.CeilToInt(totalArea * Options.ShapeSize);
        int shapesLeft = Options.ShapeCount;
        int averageShapeArea = landArea / shapesLeft;
        for (int i = 0; i < initBoxes.Length; i++)
        {
            //Calculating new shape total area, and size ratio between sides.
            int newArea = (int)(averageShapeArea * Seed.seed.Next(8, 12) / 10f);
            float sizeRatio = Seed.seed.Next((int)((1/Options.MaxShapeSizeRatio)*100), (int)(Options.MaxShapeSizeRatio*100)) / 100f;
            int Xsize =(int)Mathf.Sqrt(newArea/sizeRatio);
            int Ysize = (int)(Xsize * sizeRatio);
            newArea = Xsize * Ysize;

            //Calculating movement limitations within the basic box for the finished shape box.
            int maxXOffset = initBoxes[i].XSize - Xsize;
            int maxYOffset = initBoxes[i].YSize - Ysize;
            if (maxXOffset < 0) 
            {
                Xsize += maxXOffset; 
                maxXOffset = 0; 
            }
            if (maxYOffset < 0) 
            {
                Ysize += maxYOffset; 
                maxYOffset = 0; 
            }

            //Calculating finalized position for the new box.
            int xOffset = Seed.seed.Next(0, maxXOffset);
            int yOffset = Seed.seed.Next(0, maxYOffset);
            Dot start = new Dot(initBoxes[i].Start.X + xOffset, initBoxes[i].Start.Y + yOffset);
            Dot end = new Dot(start.X + Xsize, start.Y + Ysize);

            //Creating new box and updating area values.
            landArea -= newArea;
            shapesLeft--;
            if (shapesLeft == 0)
            {
                shapesLeft++;
            }
            averageShapeArea = landArea / shapesLeft;
            createdBoxes.Add(new Box(start,end,true));
        }
    }

    //Returns generated boxes in their generation order.
    public Box GetNextBox()
    {
        if (boxesRequested >= createdBoxes.Count)
        {
            return null;
        }
        Box retBox = createdBoxes[boxesRequested];
        boxesRequested++;
        return retBox;
    }
}
