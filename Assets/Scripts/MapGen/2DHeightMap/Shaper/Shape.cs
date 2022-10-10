using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Shape class, holds methods for shape creation, movement, rotation, and filling.
public class Shape
{
    public int Id;
    public Dot[] Nodes;
    public Line[] BaseLines;
    public Box OuterBox;
    public List<Dot> PerlinContour;
    //Individual heightmap for each shape is used to allow multithreaded shape filling.
    public float[,] HeightMap;

    //Main method for shape creation.
    public Shape(int id, Box box, float fillValue, float rotation)
    {
        //Assigning basic data needed to create a shape.
        Id = id;
        List<Dot> nodes = new List<Dot>();
        List<Line> lines = new List<Line>();
        OuterBox = box;

        float fillTarget = fillValue;
        float fillThreshold = Options.FillThreshold;
        float currentFill;

        //Creating an inner limiter to control shape creation along with the OuterBox.
        ShapeLimiter shapeLimiter = new ShapeLimiter(OuterBox, Options.InnerBoxShrinkPercent);

        //Deciding on the start side based on the box orientation, 0 - left side, 1 - top side.
        int startSide = 0;
        if (shapeLimiter.IsHorizontal)
        {
            startSide = 1;
        }
        bool returnedToStartSide = false;

        //Selecting the starting point for shape creation. Based on the starting side, point will be placed
        //approximately at 65% of the starting side length for one axis, and at a random point within
        //the created limitations for the other axis.
        Dot startDot = new Dot(0, 0);
        switch (startSide)
        {
            case 0:
                if (shapeLimiter.XRange > 8) startDot.X = Seed.seed.Next(4, shapeLimiter.XRange / 2) + OuterBox.Start.X;
                else startDot.X = 4 + OuterBox.Start.X;
                startDot.Y = shapeLimiter.Start.Y + (int)(shapeLimiter.Axis.Length * 0.65f);
                break;
            case 1:
                startDot.X = shapeLimiter.Start.X + (int)(shapeLimiter.Axis.Length * 0.65f);
                if (shapeLimiter.YRange > 8) startDot.Y = OuterBox.End.Y - (int)Seed.seed.Next(4, shapeLimiter.YRange / 2);
                else startDot.Y = OuterBox.End.Y - 4;
                break;
        }

        //Getting the fill value (distance between inner and outer limit) for the starting dot.
        currentFill = shapeLimiter.GetFill(startDot, startSide);

        nodes.Add(startDot);
        //Collecting additional data needed for the shape creation loop.
        bool closed = false;
        int iterations = 1;
        int curSide = startSide; //Sides: 0 - left, 1 - top, 2 - right, 3 - bottom.
        Angle lastAngle = new Angle(curSide * 90);
        float percDone = 0;

        //Creating finished shape size variables, in order to resize the outer box after the shape creation.
        int shapeMinX = Options.MapSize;
        int shapeMinY = Options.MapSize;
        int shapeMaxX = 0;
        int shapeMaxY = 0;

        //Starting point for the shape creation loop, which will create new dots at fixed distance and selected angles until the shape will be closed.
        while (!closed)
        {
            //Collecting data on the previous dot.
            Dot prevDot = nodes[iterations - 1];
            Angle thisAngle = new Angle(lastAngle.Euler);
            Angle baseAngle = new Angle(90 * curSide);
            int distToInnerLim = shapeLimiter.GetDistToInnerLim(prevDot, curSide);

            //Start of the angle selection process, if the shape is close to finishing, angle will be selected based on the starting dot
            //position with minimum variety.

            if (percDone > 0.9f && returnedToStartSide)
            {
                lastAngle.Euler = prevDot.AngleTo(nodes[0]);
                thisAngle = lastAngle + Seed.seed.Next(-Options.MaxShapeClosingAngle, Options.MaxShapeClosingAngle);
            }
            else
            {
                //Calculating the precise angle to reach the fill target.
                float targetAngle = shapeLimiter.GetAngleToOptimalFill(prevDot, curSide, fillTarget);

                //Calculating how hard the ideal angle will affect the randomly selected one,
                //In cases where the current fill is within the threshold range,
                //or the previous dot was too close to the inner limitations, it should be ignored completely.
                float targetStrength = Mathf.Abs(fillTarget - currentFill);
                if (distToInnerLim < 0)
                {
                    targetStrength = 0;
                }
                if (currentFill < fillTarget + fillThreshold && currentFill > fillTarget - fillThreshold)
                {
                    targetStrength = 0;
                }

                //Setting up a range of angles from the previous dot that will be checked for collisions.
                Angle checkStart = new Angle(baseAngle.Euler - (Options.BaseAngleRangeForDots / 2));
                Angle checkEnd = new Angle(baseAngle.Euler + (Options.BaseAngleRangeForDots / 2));
                if (distToInnerLim < 4)
                {
                    checkEnd = baseAngle;
                }
                Angle checkedAngle = new Angle(checkStart.Euler);

                //Setting up a list of angle ranges that are free from collisions. Every even angle opens a range, and every uneven angle closes it.
                List<Angle> ranges = new List<Angle>();
                bool rangeOpened = false;

                //Starting to check the created range, with a predefined step.
                for (int i = 0; i <= (Options.BaseAngleRangeForDots / Options.CheckStepForAngleRanges); i++)
                {
                    //Creating a line and checking if it collides with anything.
                    Dot checkDot = new Dot(prevDot, checkedAngle.Euler, Options.LineBaseLength, Options.MapSize);
                    Line checkLine = new Line(prevDot, checkDot);
                    bool result = checkLine.CheckIfCrossesLand(OuterBox);

                    //If there is no collision, a new angle range is opened.
                    if (result == false && !rangeOpened)
                    {
                        ranges.Add(new Angle(checkedAngle.Euler));
                        rangeOpened = true;
                    }
                    //If the collision is detected and range was opened, it will be closed with the previous step angle.
                    else if (result != false && rangeOpened)
                    {
                        ranges.Add(new Angle(checkedAngle.Euler - Options.CheckStepForAngleRanges));
                        rangeOpened = false;
                    }

                    //At the end of the check, opened range will be closed, if there was no range opened and the last angle was free,
                    //a single angle range will be created.
                    if (checkedAngle == checkEnd)
                    {
                        if (rangeOpened)
                        {
                            if (result == false)
                            {
                                ranges.Add(new Angle(checkedAngle.Euler));
                            }
                            else
                            {
                                ranges.Add(new Angle(checkedAngle.Euler - Options.CheckStepForAngleRanges));
                            }
                        }
                        else if (result == false)
                        {
                            ranges.Add(checkedAngle);
                            ranges.Add(checkedAngle);
                        }
                    }
                    checkedAngle += Options.CheckStepForAngleRanges;
                }

                //Failsafe measure to make sure that a single angle will form a range.
                if (ranges.Count == 1)
                {
                    ranges.Add(ranges[0]);
                }

                Angle min = new Angle();
                Angle max = new Angle();

                //Handling the case with a single range, range will be checked against the last used angle and shrinked to
                //the preferred max difference between new and previous angle.
                if (ranges.Count == 2)
                {
                    if (ranges[0] == checkStart && ranges[1] == checkEnd)
                    {
                        if (checkStart < lastAngle - Options.PrefferableMaxAngleChange)
                        {
                            if (checkEnd > lastAngle + Options.PrefferableMaxAngleChange)
                            {
                                min = lastAngle - Options.PrefferableMaxAngleChange;
                                max = lastAngle + Options.PrefferableMaxAngleChange;
                            }
                            else
                            {
                                max = checkEnd;
                                min = max - (Options.PrefferableMaxAngleChange*2);
                            }
                        }
                        else
                        {
                            min = checkStart;
                            max = min + Options.PrefferableMaxAngleChange*2;
                        }
                    }
                    else
                    {
                        bool gotMin = false;
                        bool gotMax = false;
                        if (ranges[0] < lastAngle - Options.PrefferableMaxAngleChange && lastAngle - Options.PrefferableMaxAngleChange < ranges[1])
                        {
                            min = lastAngle - Options.PrefferableMaxAngleChange;
                            gotMin = true;
                        }
                        if (ranges[0] < lastAngle + Options.PrefferableMaxAngleChange && lastAngle + Options.PrefferableMaxAngleChange < ranges[1])
                        {
                            max = lastAngle + Options.PrefferableMaxAngleChange;
                            gotMax = true;
                        }
                        if (!gotMax && !gotMin)
                        {
                            float minDist = Mathf.Abs(ranges[0].GetDiff((lastAngle + Options.PrefferableMaxAngleChange)));
                            float maxDist = Mathf.Abs(ranges[1].GetDiff((lastAngle - Options.PrefferableMaxAngleChange)));
                            if (minDist < maxDist)
                            {
                                min = ranges[0];
                                max = ranges[0];
                            }
                            else
                            {
                                min = ranges[1];
                                max = ranges[1];
                            }
                        }
                        else if (gotMax && !gotMin)
                        {
                            if (max - Options.PrefferableMaxAngleChange*2 > ranges[0])
                            {
                                min = max - Options.PrefferableMaxAngleChange*2;
                            }
                            else
                            {
                                min = ranges[0];
                            }
                        }
                        else if (gotMin && !gotMax)
                        {
                            if (min + Options.PrefferableMaxAngleChange*2 < ranges[1])
                            {
                                max = min + Options.PrefferableMaxAngleChange*2;
                            }
                            else
                            {
                                max = ranges[1];
                            }
                        }
                    }
                }

                //Displaying the error in case there is something wrong with ranges.
                else if (ranges.Count == 0 || ranges.Count == 1)
                {
                    Debug.LogError("FAILED TO GET RANGES!");
                    min = baseAngle;
                    max = baseAngle;
                }

                //Handling the case with multiple ranges, closest to the base angle for the current side will be selected.
                else
                {
                    if (ranges.Count % 2 != 0)
                    {
                        ranges.RemoveAt(ranges.Count - 1);
                    }

                    float[] diffs = new float[ranges.Count / 2];
                    float largest = Options.BaseAngleRangeForDots; 
                    int ind = 0;

                    for (int i = 0; i < ranges.Count; i += 2)
                    {
                        diffs[i / 2] = Mathf.Abs(ranges[i].GetDiff(baseAngle));
                        bool baseInRange = ranges[i] <= baseAngle && baseAngle <= ranges[i + 1];
                        if (baseInRange)
                        {
                            ind = i;
                            min = baseAngle;
                            max = baseAngle;
                            break;
                        }
                        if (diffs[i / 2] <= largest)
                        {
                            largest = diffs[i / 2];
                            ind = i;
                            min = ranges[ind];
                            max = ranges[ind + 1];
                        }
                    }
                    bool gotMin = false;
                    bool gotMax = false;
                    if (ranges[0] < lastAngle - 45 && lastAngle - 45 < ranges[1])
                    {
                        min = lastAngle - 45;
                        gotMin = true;
                    }
                    if (ranges[0] < lastAngle + 45 && lastAngle + 45 < ranges[1])
                    {
                        max = lastAngle + 45;
                        gotMax = true;
                    }
                    if (!gotMax && !gotMin)
                    {
                        float minDist = Mathf.Abs(min.GetDiff((lastAngle + 45)));
                        float maxDist = Mathf.Abs(max.GetDiff((lastAngle - 45)));
                        if (minDist < maxDist)
                        {
                            min = ranges[ind];
                            max = ranges[ind];
                        }
                        else
                        {
                            min = ranges[ind + 1];
                            max = ranges[ind + 1];
                        }
                    }
                    else if (gotMax && !gotMin)
                    {
                        if (max - 90 > ranges[ind])
                        {
                            min = max - 90;
                        }
                        else
                        {
                            min = ranges[ind];
                        }
                    }
                    else if (gotMin && !gotMax)
                    {
                        if (min + 90 < ranges[ind + 1])
                        {
                            max = min + 90;
                        }
                        else
                        {
                            max = ranges[ind + 1];
                        }
                    }
                }

                //Creating a random angle within the selected range. Making sure that all angles at that point are in range from 0 to 360.
                float freeAngle = Seed.seed.Next((int)min.Euler, (int)(min.Euler + Mathf.Abs(max.GetDiff(min))));
                if (freeAngle < 0)
                {
                    freeAngle += 360;
                }
                float targetDiff = targetAngle - freeAngle;
                if (targetDiff > 180)
                {
                    targetDiff = (360 - targetAngle) + freeAngle;
                }
                else if (targetDiff < -180)
                {
                    targetDiff = (360 - freeAngle) + targetAngle;
                }

                //Calculating finished angle for the next dot based on the collected data.
                thisAngle.Euler = (freeAngle + targetDiff * targetStrength) % 360;
                if (thisAngle > max)
                {
                    thisAngle.Euler = max.Euler;
                }
                else if (thisAngle < min)
                {
                    thisAngle.Euler = min.Euler;
                }
            }

            //Creating the next dot, creating a line between a new and a previous dot, and writing collision info for it.
            Dot dot = new Dot(nodes[iterations - 1], thisAngle.Euler, Options.LineBaseLength, Options.MapSize);
            Line tempLine = new Line(nodes[iterations - 1], dot);
            tempLine.WriteToBox(OuterBox, Id);
            lines.Add(tempLine);
            lastAngle = thisAngle;
            nodes.Add(dot);

            //Updating finished shape size values.
            if (dot.X < shapeMinX)
            {
                shapeMinX = dot.X;
            }
            else if (dot.X > shapeMaxX)
            {
                shapeMaxX = dot.X;
            }
            if (dot.Y < shapeMinY)
            {
                shapeMinY = dot.Y;
            }
            else if (dot.Y > shapeMaxY)
            {
                shapeMaxY = dot.Y;
            }

            //Checking if the new dot has crossed the inner limitation and the cycle should switch the current shape side. 
            if (!returnedToStartSide)
            {
                bool shouldSwitch = false;
                switch (curSide)
                {
                    case 0:
                        shouldSwitch = dot.Y > shapeLimiter.End.Y;
                        break;
                    case 1:
                        shouldSwitch = dot.X > shapeLimiter.End.X;
                        break;
                    case 2:
                        shouldSwitch = dot.Y < shapeLimiter.Start.Y;
                        break;
                    case 3:
                        shouldSwitch = dot.X < shapeLimiter.Start.X;
                        break;
                }
                if (shouldSwitch)
                {
                    curSide++; if (curSide == 4) curSide = 0;
                    if (curSide == startSide) returnedToStartSide = true;
                }
            }

            //Updating fill values and info on current progress.
            float FillAtCurrentDot = shapeLimiter.GetFill(dot, curSide);
            currentFill = ((currentFill * iterations) + FillAtCurrentDot) / (iterations + 1);
            percDone = shapeLimiter.PercentageTravelled(prevDot, curSide, startSide, returnedToStartSide);

            //Checking if the shape can be closed.
            if (returnedToStartSide && nodes[iterations].GetSteps(startDot) < Options.DotsToCloseShape)
            {
                closed = true;
            }

            //Safeguard from an endless loop.
            iterations++;
            if (iterations >= Options.MaxDotsForShape)
            {
                Debug.LogError("Failed to create shape");
                closed = true;
            }
        }

        //Closing the last line, storing lines and nodes into arrays, resizing the outer box and rotating the shape for the given angle.
        Line lastLine = new Line(nodes[nodes.Count - 1], nodes[0]);
        lastLine.WriteToBox(OuterBox, Id);
        lines.Add(lastLine);
        Nodes = nodes.ToArray();
        BaseLines = lines.ToArray();
        OuterBox = new Box(new Dot(shapeMinX, shapeMinY), new Dot(shapeMaxX, shapeMaxY));
        Rotate(rotation);
    }

    //Rotates the shape before the finalized contour is created.
    public void Rotate(float angle)
    {
        float randRotation = angle;
        int newminX = Options.MapSize;
        int newmaxX = 0;
        int newminY = Options.MapSize;
        int newmaxY = 0;
        //Rotating every node for the given angle around the box center.
        for (int i = 0; i < Nodes.Length; i++)
        {
            Nodes[i] = new Dot(OuterBox.Center, OuterBox.Center.AngleTo(Nodes[i]) + randRotation, OuterBox.Center.GetDistance(Nodes[i]), Options.MapSize);
            if (Nodes[i].X < newminX)
            {
                newminX = Nodes[i].X;
            }
            if (Nodes[i].X > newmaxX)
            {
                newmaxX = Nodes[i].X;
            }
            if (Nodes[i].Y < newminY)
            {
                newminY = Nodes[i].Y;
            }
            if (Nodes[i].Y > newmaxY)
            {
                newmaxY = Nodes[i].Y;
            }
        }
        if (newminX != 0)
        {
            newminX--;
        }
        if (newminY != 0)
        {
            newminY--;
        }
        if (newmaxX < Options.MapSize - 1)
        {
            newmaxX++;
        }
        if (newmaxY < Options.MapSize - 1)
        {
            newmaxY++;
        }

        //Resizing the box after rotation, redrawing lines and writing new collision info.
        OuterBox = new Box(new Dot(newminX, newminY), new Dot(newmaxX, newmaxY));
        OuterBox.TempLand = new int[OuterBox.XSize + 1, OuterBox.YSize + 1];
        for (int i = 0; i < BaseLines.Length; i++)
        {
            int nextNode = i + 1;
            if (nextNode == BaseLines.Length) nextNode = 0;
            BaseLines[i] = new Line(Nodes[i], Nodes[nextNode]);
            BaseLines[i].WriteToBox(OuterBox, Id);
        }
    }

    //Moving the shape before the finalized contour is created.
    public void MoveBy(Dot moveDot)
    {
        int newminX = Options.MapSize;
        int newmaxX = 0;
        int newminY = Options.MapSize;
        int newmaxY = 0;
        //Moving every node in shape for the given value.
        for (int i = 0; i < Nodes.Length; i++)
        {
            Nodes[i] = Nodes[i] + moveDot;
            if (Nodes[i].X < newminX)
            {
                newminX = Nodes[i].X;
            }
            if (Nodes[i].X > newmaxX)
            {
                newmaxX = Nodes[i].X;
            }
            if (Nodes[i].Y < newminY)
            {
                newminY = Nodes[i].Y;
            }
            if (Nodes[i].Y > newmaxY)
            {
                newmaxY = Nodes[i].Y;
            }
        }
        if (newminX != 0)
        {
            newminX--;
        }
        if (newminY != 0)
        {
            newminY--;
        }
        if (newmaxX < Options.MapSize - 1)
        {
            newmaxX++;
        }
        if (newmaxY < Options.MapSize - 1)
        {
            newmaxY++;
        }

        //Resizing the box after movement, redrawing lines and writing new collision info.
        OuterBox = new Box(new Dot(newminX, newminY), new Dot(newmaxX, newmaxY));
        OuterBox.TempLand = new int[OuterBox.XSize + 1, OuterBox.YSize + 1];
        for (int i = 0; i < BaseLines.Length; i++)
        {
            int nextNode = i + 1;
            if (nextNode == BaseLines.Length) nextNode = 0;
            BaseLines[i] = new Line(Nodes[i], Nodes[nextNode]);
            BaseLines[i].WriteToBox(OuterBox, Id);
        }
    }

    //Standalone method that can be called from different threads.
    public void CreateCountour(DensityMap densityMap)
    {
        PerlinContour = CreatePerlinContour(densityMap);
    }

    //Creates finalized contour along the previously created baseline.
    List<Dot> CreatePerlinContour(DensityMap densityMap)
    {

        //Preparing storage for finalized shape dimensions.
        int shapeMaxX = 0;
        int shapeMaxY = 0;
        int shapeMinX = Options.MapSize;
        int shapeMinY = Options.MapSize;

        List<Dot> contour = new List<Dot>();

        //Array of available movement options for each dot.
        Dot[] options = { new Dot(0, 1), new Dot(1, 1), new Dot(1, 0), new Dot(1, -1), new Dot(0, -1), new Dot(-1, -1), new Dot(-1, 0), new Dot(-1, 1) };

        //Preparing data on the closest base line dot and the dot behind it. Setting up the first perlin contour dot.
        int curLine = 0;
        int lastLine = BaseLines.Length - 1;
        int curDotInd = 0;
        int lastDotInd = BaseLines[lastLine].Length - 2;

        Dot curDot = BaseLines[curLine].Dots[curDotInd];
        Dot lastDot = BaseLines[lastLine].Dots[lastDotInd];
        Dot lastDirection;

        Dot startDot = Nodes[0];
        contour.Add(startDot);

        //Updating shape size data.
        if (startDot.X > shapeMaxX)
        {
            shapeMaxX = startDot.X;
        }
        else if (startDot.X < shapeMinX)
        {
            shapeMinX = startDot.X;
        }
        if (startDot.Y > shapeMaxY)
        {
            shapeMaxY = startDot.Y;
        }
        else if (startDot.Y < shapeMinY)
        {
            shapeMinY = startDot.Y;
        }

        //Preparing data for the contour creation loop.
        int tries = 0;
        bool closed = false;
        Dot perlinDot = startDot;
        bool lookingToClose = false;
        bool closing = false;

        while (!closed && tries < Options.MaxContourDots)
        {
            //Collecting info on the current closest base line dot.
            int distanceChecked = 0;
            int distanceToCurrentDot = int.MaxValue;
            int startLine = curLine;
            int startDotIndex = curDotInd;
            int changedDirection = 0;
            int endDotIndex = 0;
            int endLineIndex = 0;

            //At first, loop checks the distance from the current contour dot to the base line dot, and the dots ahead of it,
            //if the distance is growing or the limit is reached, the loop will start to check the distance to the dots behind the baseline dot.
            while (distanceChecked < Options.MaxDotsToCheckForContour)
            {
                int thisDist = perlinDot.GetSteps(curDot);
                if (thisDist <= distanceToCurrentDot)
                {
                    distanceToCurrentDot = thisDist;
                    endDotIndex = curDotInd;
                    endLineIndex = curLine;
                }
                else
                {
                    changedDirection++;
                    if (changedDirection == 1)
                    {
                        curDotInd = endDotIndex;
                        curLine = endLineIndex;
                        break;
                    }
                    else
                    {
                        curLine = startLine;
                        curDotInd = startDotIndex;
                    }
                }
                if (changedDirection == 0)
                {
                    curDotInd++;
                    if (curDotInd >= BaseLines[curLine].Length)
                    {
                        curDotInd = 1;
                        curLine++;
                        if (curLine >= BaseLines.Length)
                        {
                            curLine = 0;
                        }
                    }
                }
                else
                {
                    curDotInd--;
                    if (curDotInd < 0)
                    {
                        curLine--;
                        if (curLine < 0)
                        {
                            curLine = BaseLines.Length - 1;
                        }
                        curDotInd = BaseLines[curLine].Dots.Length - 1;
                    }
                }
                if (curDotInd == 1 && curDotInd == BaseLines[curLine].Dots.Length)
                {
                    curDotInd = 0;
                }
                curDot = BaseLines[curLine].Dots[curDotInd];

                distanceChecked++;
            }

            //Getting the dot behind the closest one.
            lastDotInd = curDotInd - 1;
            if (lastDotInd < 0)
            {
                lastLine = curLine - 1;
                if (lastLine < 0)
                {
                    lastLine = BaseLines.Length - 1;
                }
                lastDotInd = BaseLines[lastLine].Length - 1;
            }
            else
            {
                lastLine = curLine;
            }
            lastDot = BaseLines[lastLine].Dots[lastDotInd];

            //Getting the movement direction between the closest and the previous dot for the contour to follow.
            lastDirection = curDot - lastDot;

            //Checking if the contour covered half of the baseline.
            if (!lookingToClose)
            {
                lookingToClose = curLine > BaseLines.Length / 2;
            }
            //Checking if the contour is less than 3 lines (12 dots) away from the start.
            if (lookingToClose)
            {
                if (!closing)
                {
                    closing = curLine > BaseLines.Length - 3;
                }
            }

            //Collecting currently available movement options based on the base line direction.
            int[] curOpts = new int[3];
            if (lastDirection.X > 0)
            {
                if (lastDirection.Y == 0)
                {
                    curOpts[0] = 1;
                    curOpts[1] = 2;
                    curOpts[2] = 3;
                }
                else if (lastDirection.Y > 0)
                {
                    curOpts[0] = 0;
                    curOpts[1] = 1;
                    curOpts[2] = 2;
                }
                else
                {
                    curOpts[0] = 2;
                    curOpts[1] = 3;
                    curOpts[2] = 4;
                }
            }
            else if (lastDirection.X == 0)
            {
                if (lastDirection.Y > 0)
                {
                    curOpts[0] = 7;
                    curOpts[1] = 0;
                    curOpts[2] = 1;
                }
                else
                {
                    curOpts[0] = 3;
                    curOpts[1] = 4;
                    curOpts[2] = 5;
                }
            }
            else
            {
                if (lastDirection.Y == 0)
                {
                    curOpts[0] = 5;
                    curOpts[1] = 6;
                    curOpts[2] = 7;
                }
                else if (lastDirection.Y > 0)
                {
                    curOpts[0] = 6;
                    curOpts[1] = 7;
                    curOpts[2] = 0;
                }
                else
                {
                    curOpts[0] = 4;
                    curOpts[1] = 5;
                    curOpts[2] = 6;
                }
            }

            //Generating perlin value which will be used in option selection for the next contour dot.
            float pVal = PerlinNoise.DotPerlinValue(perlinDot.X, perlinDot.Y);
            int perlinOption = 0;
            if (pVal > 0.66f)
            {
                perlinOption = 2;
            }
            else if (pVal > 0.33f)
            {
                perlinOption = 1;
            }

            //Checking the amount of land at the available options.
            int[] densityVals = new int[3];
            int smallestDensity = int.MaxValue;
            int densityOption = 0;

            for (int i = 0; i < densityVals.Length; i++)
            {
                Dot densDot = perlinDot + options[curOpts[i]];
                if (densDot.X < 0 || densDot.X >= Options.MapSize || densDot.Y < 0 || densDot.Y >= Options.MapSize)
                {
                    densityVals[i] = densityMap.LandValue;
                }
                else
                {
                    densityVals[i] = densityMap.DensityArray[densDot.X, densDot.Y];
                }

                if (densityVals[i] < smallestDensity)
                {
                    smallestDensity = densityVals[i];
                    densityOption = i;
                }
            }
            if (densityVals[0] == densityVals[1] && densityVals[1] == densityVals[2])
            {
                densityOption = -1;
            }

            //Selecting the final option for the next contour dot with the priority given to the option with the lowest density,
            //in order to avoid shape collisions. If the density will be the same for all of the options, perlin value will be used.
            //In case the shape is in the closing state - the dot with the lowest distance towards the starting dot will be selected.
            if (closing)
            {
                int smallestDist = int.MaxValue; int ind = 0;
                for (int i = 0; i < options.Length; i++)
                {
                    int distToEnd = (perlinDot + options[i]).GetSteps(startDot);
                    if (distToEnd < smallestDist)
                    {
                        smallestDist = distToEnd;
                        ind = i;
                    }
                }
                perlinDot = perlinDot + options[ind];
            }
            else
            {
                if (densityOption == -1)
                {
                    perlinDot = perlinDot + options[curOpts[perlinOption]];
                }
                else
                {
                    perlinDot = perlinDot + options[curOpts[densityOption]];
                }
            }

            //Making sure that the new dot is located within the map.
            if (perlinDot.X < 0)
            {
                perlinDot.X = 0;
            }
            else if (perlinDot.X >= Options.MapSize)
            {
                perlinDot.X = Options.MapSize - 1;
            }
            if (perlinDot.Y < 0)
            {
                perlinDot.Y = 0;
            }
            else if (perlinDot.Y >= Options.MapSize)
            {
                perlinDot.Y = Options.MapSize - 1;
            }

            //Making sure that the new dot is not exceeding the maximum distance from base line.
            if (perlinDot.X < curDot.X - Options.PContourDist / 2)
            {
                perlinDot.X = curDot.X - Options.PContourDist / 2;
            }
            else if (perlinDot.X >= curDot.X + Options.PContourDist / 2)
            {
                perlinDot.X = curDot.X + Options.PContourDist / 2;
            }
            if (perlinDot.Y < curDot.Y - Options.PContourDist / 2)
            {
                perlinDot.Y = curDot.Y - Options.PContourDist / 2;
            }
            else if (perlinDot.Y >= curDot.Y + Options.PContourDist / 2)
            {
                perlinDot.Y = curDot.Y + Options.PContourDist / 2;
            }

            //Checking if the contour should be closed, if not adding the dot to the list and updating contour size.
            if (closing && perlinDot == startDot)
            {
                closed = true;
            }
            else
            {
                contour.Add(perlinDot);

                //Writing collision info for the added dot.
                densityMap.DensityArray[perlinDot.X, perlinDot.Y] = densityMap.LandValue;
                if (perlinDot.X > shapeMaxX) shapeMaxX = perlinDot.X;
                if (perlinDot.X < shapeMinX) shapeMinX = perlinDot.X;
                if (perlinDot.Y > shapeMaxY) shapeMaxY = perlinDot.Y;
                if (perlinDot.Y < shapeMinY) shapeMinY = perlinDot.Y;
            }
            tries++;
        }

        //Writing the whole contour to the density map with radius from the settings.
        for (int i = 0; i < contour.Count; i++)
        {
            densityMap.AddLandAtPoint(contour[i]);
        }

        //Updating outer box size to fit the new contour.
        if (shapeMaxX < Options.MapSize)
        {
            shapeMaxX++;
        }
        if (shapeMaxY < Options.MapSize)
        {
            shapeMaxY++;
        }
        if (shapeMinY > 0)
        {
            shapeMinY--;
        }
        if (shapeMinX > 0)
        {
            shapeMinX--;
        }
        OuterBox = new Box(new Dot(shapeMinX, shapeMinY), new Dot(shapeMaxX, shapeMaxY));

        return contour;
    }

    //Fills the finished contour with noise data, based on the distance from the edge of the shape.
    //Takes in a dot from which noise sampling will begin, and a custom start dot if the function has to be rerun.
    public void FillShapeWithLand(Dot randomStartDot, Dot customStartDot)
    {

        HeightMap = new float[OuterBox.XSize + 1, OuterBox.YSize + 1];
        //Creating a separate map for collision detection, and writing current contour into that.
        float[,] tempHeightMap = new float[OuterBox.XSize + 1, OuterBox.YSize + 1];
        for (int i = 0; i < PerlinContour.Count; i++)
        {
            tempHeightMap[PerlinContour[i].X - OuterBox.Start.X, PerlinContour[i].Y - OuterBox.Start.Y] = 1f;
        }

        List<Dot> dotsToCheck = new List<Dot>();

        //Starting dot for the algorithm is taken at the center of the shape, if that approach
        //already failed once, it takes a given dot.
        Dot startDot = new Dot(OuterBox.XSize / 2, OuterBox.YSize / 2);
        if (customStartDot != null)
        {
            startDot = customStartDot;
        }

        //Checking if the given dot is surrounded by a contour dot from all sides.
        int lDist = 0, rDist = 0, tDist = 0, bDist = 0;
        bool lLand = false, rLand = false, tLand = false, bLand = false;
        for (int x2 = startDot.X + 1; x2 < HeightMap.GetLength(0); x2++)
        {
            if (tempHeightMap[x2, startDot.Y] != 0)
            {
                rLand = true;
                break;
            }
            else
            {
                rDist++;
            }
        }
        for (int x2 = startDot.X - 1; x2 >= 0; x2--)
        {
            if (tempHeightMap[x2, startDot.Y] != 0)
            {
                lLand = true;
                break;
            }
            else
            {
                lDist++;
            }
        }
        for (int y2 = startDot.Y + 1; y2 < HeightMap.GetLength(1); y2++)
        {
            if (tempHeightMap[startDot.X, y2] != 0)
            {
                tLand = true;
                break;
            }
            else
            {
                tDist++;
            }
        }
        for (int y2 = startDot.Y - 1; y2 >= 0; y2--)
        {
            if (tempHeightMap[startDot.X, y2] != 0)
            {
                bLand = true;
                break;
            }
            else
            {
                bDist++;
            }
        }
        if (!(rLand && lLand && tLand && bLand))
        {
            if (!rLand)
            {
                startDot.X -= lDist - 1;
            }
            else if (!lLand)
            {
                startDot.X += rDist + 1;
            }
            else if (!tLand)
            {
                startDot.Y -= bDist - 1;
            }
            else if (!bLand)
            {
                startDot.Y += tDist + 1;
            }
        }

        //Starting shape fill loop with all the required data.
        fillDot(startDot);
        checkNeighbourDots(startDot);
        bool shouldBreak = false;
        int tries = 0;
        while (dotsToCheck.Count > 0 && tries < Options.MaxSurfaceDotsForShape && !shouldBreak)
        {
            checkNeighbourDots(dotsToCheck[0]);
            dotsToCheck.RemoveAt(0);
            tries++;
        }

        //Fills the dot with finished height value, based on the distance from the shore and noise value.
        void fillDot(Dot dot)
        {
            //Measuring distance to the contour from the given dot.
            int topContourDistance = 0;
            int botContourDistance = 0;
            int leftContourDistance = 0;
            int rightContourDistance = 0;
            for (int x2 = dot.X + 1; x2 < HeightMap.GetLength(0); x2++)
            {
                if (tempHeightMap[x2, dot.Y] != 0)
                {
                    break;
                }
                else
                {
                    rightContourDistance++;
                }
            }
            for (int x2 = dot.X - 1; x2 >= 0; x2--)
            {
                if (tempHeightMap[x2, dot.Y] != 0)
                {
                    break;
                }
                else
                {
                    leftContourDistance++;
                }
            }
            for (int y2 = dot.Y + 1; y2 < HeightMap.GetLength(1); y2++)
            {
                if (tempHeightMap[dot.X, y2] != 0)
                {
                    break;
                }
                else
                {
                    topContourDistance++;
                }
            }
            for (int y2 = dot.Y - 1; y2 >= 0; y2--)
            {
                if (tempHeightMap[dot.X, y2] != 0)
                {
                    break;
                }
                else
                {
                    botContourDistance++;
                }
            }

            //Calculating the smallest distance on both axes.
            int minHorisontalDistance = leftContourDistance;
            if (minHorisontalDistance > rightContourDistance)
            {
                minHorisontalDistance = rightContourDistance;
            }
            int minVerticalDistance = topContourDistance;
            if (minVerticalDistance > botContourDistance)
            {
                minVerticalDistance = botContourDistance;
            }

            //Calculating maximum possible distance on both axes for the current dot.
            int maxHorizontalDistance = leftContourDistance + rightContourDistance;
            if (maxHorizontalDistance < 4)
            {
                maxHorizontalDistance = 4;
            }
            int maxVerticalDistance = topContourDistance + botContourDistance;
            if (maxVerticalDistance < 4)
            {
                maxVerticalDistance = 4;
            }

            //Getting finished distance value based on previous measurements.
            float horizontalDistanceValue = (float)minHorisontalDistance / maxHorizontalDistance;
            float verticalDistanceValue = (float)minVerticalDistance / maxVerticalDistance;
            float distanceValue = horizontalDistanceValue * verticalDistanceValue;

            //Sampling noise.
            float perlinValue = PerlinNoise.TerrainPerlinValue(dot.X + randomStartDot.X, dot.Y + randomStartDot.Y);

            //Calculating finished height value for tile, based on distance and noise value.
            HeightMap[dot.X, dot.Y] = Options.WaterHeightCutoff + (1 - Options.WaterHeightCutoff) * perlinValue * distanceValue;
        }

        //Check if the 4 neighboring dots are suitable to be filled with noise.
        void checkNeighbourDots(Dot dot)
        {
            //Checking if the provided dot touches the edge of the shape's height map. If that's the case it means that filling process
            //started outside of the created contour, and should be restarted with the new starting point.
            if (dot.X == 0 || dot.X == HeightMap.GetLength(0) || dot.Y == 0 || dot.Y == HeightMap.GetLength(1))
            {
                int minX = OuterBox.XSize / 10;
                int maxX = OuterBox.XSize - minX;
                int minY = OuterBox.YSize / 10;
                int maxY = OuterBox.YSize - minY;
                shouldBreak = true;
                FillShapeWithLand(randomStartDot, new Dot(Seed.seed.Next(minX, maxX), Seed.seed.Next(minY, maxY)));
            }
            else
            {
                //Going through 4 neighboring dots and filling them if they are not placed on the contour, and weren't previously filled.
                //If the dot was filled, it is also added to the list of dots whose neighbors will be checked.
                if (dot.X < HeightMap.GetLength(0) - 1)
                {
                    Dot newDot = new Dot(dot.X + 1, dot.Y);
                    if (tempHeightMap[newDot.X, newDot.Y] == 0 && HeightMap[newDot.X, newDot.Y] == 0)
                    {
                        dotsToCheck.Add(newDot);
                        fillDot(newDot);
                    }
                }
                if (dot.X > 0)
                {
                    Dot newDot = new Dot(dot.X - 1, dot.Y);
                    if (tempHeightMap[newDot.X, newDot.Y] == 0 && HeightMap[newDot.X, newDot.Y] == 0)
                    {
                        dotsToCheck.Add(newDot);
                        fillDot(newDot);
                    }
                }
                if (dot.Y < HeightMap.GetLength(1) - 1)
                {
                    Dot newDot = new Dot(dot.X, dot.Y + 1);
                    if (tempHeightMap[newDot.X, newDot.Y] == 0 && HeightMap[newDot.X, newDot.Y] == 0)
                    {
                        dotsToCheck.Add(newDot);
                        fillDot(newDot);
                    }
                }
                if (dot.Y > 0)
                {
                    Dot newDot = new Dot(dot.X, dot.Y - 1);
                    if (tempHeightMap[newDot.X, newDot.Y] == 0 && HeightMap[newDot.X, newDot.Y] == 0)
                    {
                        dotsToCheck.Add(newDot);
                        fillDot(newDot);
                    }
                }
            }
        }
    }
}












