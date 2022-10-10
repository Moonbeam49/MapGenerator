using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Class that represents a line as a collection of Dots.
public class Line
{
    public Dot[] Dots;
    public Dot Origin { get => Dots[0]; }
    public Dot End { get => Dots[Dots.Length - 1]; }

    public int Length { get => Dots.Length; }

    public Line(Dot origin, Dot end)
    {
        int xSteps = end.X - origin.X;
        int ySteps = end.Y - origin.Y;
        int totalLength = Math.Abs(xSteps) + Math.Abs(ySteps) + 1;
        Dots = new Dot[totalLength];
        Dots[0] = origin;
        Dots[Dots.Length - 1] = end;

        //Ensuring that a horizontal or vertical line will have its second and second to last dots align with a general direction of a line,
        //In order to provide a bigger range of angles to choose from for the next line in shape.
        if (totalLength > 2)
        {
            if (Mathf.Abs(xSteps) > Mathf.Abs(ySteps))
            {
                if (xSteps > 0)
                {
                    Dots[1] = new Dot(Origin.X + 1, origin.Y);
                    Dots[Dots.Length - 2] = new Dot(End.X - 1, End.Y);
                    xSteps -= 2;
                }
                else
                {
                    Dots[1] = new Dot(Origin.X - 1, origin.Y);
                    Dots[Dots.Length - 2] = new Dot(End.X + 1, End.Y);
                    xSteps += 2;
                }
            }
            else
            {
                if (ySteps > 0)
                {
                    Dots[1] = new Dot(Origin.X, origin.Y + 1);
                    Dots[Dots.Length - 2] = new Dot(End.X, End.Y - 1);
                    ySteps -= 2;
                }
                else
                {
                    Dots[1] = new Dot(Origin.X, origin.Y - 1);
                    Dots[Dots.Length - 2] = new Dot(End.X, End.Y + 1);
                    ySteps += 2;
                }
            }
        }

        //Building a line step by step.
        float ratio = Math.Abs((float)xSteps / ySteps);
        float offset = ratio;
        for (int i = 2; i < totalLength - 2; i++)
        {
            int newX = Dots[i - 1].X, newY = Dots[i - 1].Y;
            if (offset >= 1)
            {
                if (xSteps > 0) newX++; else if (xSteps < 0) newX--;
                offset--;
            }
            else
            {
                if (ySteps > 0) newY++; else if (ySteps < 0) newY--;
                offset = ratio + offset;
            }
            Dots[i] = new Dot(newX, newY);

        }
    }

    //Writes line to a given box templand array for further collision checks.
    public void WriteToBox(Box box, int id)
    {
        for (int i = 0; i < Dots.Length; i++)
        {
            Dot dotOnBox = new Dot(Dots[i].X - box.Start.X, Dots[i].Y - box.Start.Y);
            box.TempLand[dotOnBox.X, dotOnBox.Y] = id;
        }
    }

    //Checks every dot in line against a given box's templand array for collisions.
    public bool CheckIfCrossesLand(Box box)
    {
        for (int i = 1; i < Dots.Length; i++)
        {
            Dot dotOnBox = new Dot(Dots[i].X - box.Start.X, Dots[i].Y - box.Start.Y);
            if (box.TempLand[dotOnBox.X, dotOnBox.Y] != 0)
            {
                return true;
            }
        }

        return false;
    }
}
