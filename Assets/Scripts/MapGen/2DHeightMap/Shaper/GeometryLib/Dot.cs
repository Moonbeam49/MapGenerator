using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//2d dot that uses integers as coordinates.
public class Dot
{
    public int X;
    public int Y;

    public Dot(int x, int y)
    {
        X = x;
        Y = y;
    }

    //Creates a new dot at a given distance and angle from the origin dot.
    public Dot(Dot origin, float angle, int length, int sideSize)
    {
        float radAngle = angle * Mathf.Deg2Rad;
        int xStep = (int)(Math.Sin(radAngle) * length);
        int yStep = (int)(Math.Cos(radAngle) * length);
        X = origin.X + xStep;
        Y = origin.Y + yStep;
        if (X < 0)
        {
            X = 0;
        }
        else if (X >= sideSize)
        {
            X = sideSize - 1;
        }
        if (Y < 0)
        {
            Y = 0;
        }
        else if (Y >= sideSize)
        {
            Y = sideSize - 1;
        }
    }

    public static bool operator ==(Dot dot1, Dot dot2)
    {
        if (dot1 is null)
        {
            if (dot2 is null)
            {
                return true;
            }
            return false;
        }
        if (dot2 is null) return false;
        return dot1.X == dot2.X && dot1.Y == dot2.Y;
    }

    public static bool operator !=(Dot dot1, Dot dot2) => !(dot1 == dot2);

    public static Dot operator -(Dot a, Dot b)
    {
        return new Dot(a.X - b.X, a.Y - b.Y);
    }

    public static Dot operator +(Dot a, Dot b)
    {
        return new Dot(a.X + b.X, a.Y + b.Y);
    }

    public float AngleTo(Dot b)
    {
        
        if (b.Y - Y == 0)
        {
            if (b.X > X) return 90;
            else return 270;
        }

        float angle = (float)Math.Atan(Math.Abs(b.X - X) / (double)Math.Abs(b.Y - Y));
        angle *= Mathf.Rad2Deg;

        if (b.X > X)
        {
            if (!(b.Y > Y))
            {
                angle = 90 - angle;
                angle += 90;
            }
        }
        else
        {
            if (b.Y > Y)
            {
                angle = 90 - angle;
                angle += 270;
            }
            else
            {
                angle += 180;
            }
        }
        return angle;
    }

    //Returns the distance from this to a given dot in steps. Used in most cases.
    public int GetSteps(Dot b)
    {
        if (b != null)
        {
            return Math.Abs(X - b.X) + Math.Abs(Y - b.Y);
        }
        else
        {
            return 0;
        }
    }

    //Returns approximation of an actual distance from this to a given dot. Used for shape rotation.
    public int GetDistance(Dot b)
    {
        if (b != null)
        {
            Dot vec = new Dot(X - b.X, Y - b.Y);
            return (int)Mathf.Sqrt(Mathf.Pow(vec.X, 2) + Mathf.Pow(vec.Y, 2));
        }
        else
        {
            return 0;
        }
    }

    public override bool Equals(object obj)
    {
        var dot = obj as Dot;
        return dot != null &&
               X == dot.X &&
               Y == dot.Y;
    }

    public override int GetHashCode()
    {
        var hashCode = 1502939027;
        hashCode = hashCode * -1521134295 + X.GetHashCode();
        hashCode = hashCode * -1521134295 + Y.GetHashCode();
        return hashCode;
    }
}
