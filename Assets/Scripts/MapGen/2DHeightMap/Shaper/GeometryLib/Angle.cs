using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class used to represent angles in eulers.
public class Angle
{
    //Make sure the angle stays within the 0 to 360 range.
    public float Euler
    {
        get
        {
            return _euler;
        }
        set
        {
            _euler = value;
            _euler %= 360;
            if (_euler < 0) _euler += 360;
        }
    }

    private float _euler;

    public Angle()
    {
        Euler = 0;
    }

    public Angle(float angle)
    {
        Euler = angle;
    }

    public static Angle operator +(Angle a, Angle b)
    {
        float res = a._euler + b._euler;
        return new Angle(res);
    }

    public static Angle operator -(Angle a, Angle b)
    {
        float res = a._euler - b._euler;
        return new Angle(res);
    }

    public static Angle operator +(Angle a, float b)
    {
        return new Angle(a.Euler + b);
    }

    public static Angle operator -(Angle a, float b)
    {
        return new Angle(a.Euler - b);
    }

    //Checks if the compared angle lies to the right of the given Angle. Used for angle range checks.
    public static bool operator >(Angle a, Angle b)
    {
        float diff = a.GetDiff(b);
        if (diff < 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    //Checks if the compared angle lies to the left of the given Angle. Used for angle range checks.
    public static bool operator <(Angle a, Angle b)
    {
        float diff = a.GetDiff(b);
        if (diff > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator >=(Angle a, Angle b)
    {
        float diff = a.GetDiff(b);
        if (diff == 0)
        {
            return true;
        }
        else if (diff < 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator <=(Angle a, Angle b)
    {
        float diff = a.GetDiff(b);
        if (diff == 0)
        {
            return true;
        }
        else if (diff > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator ==(Angle a, Angle b)
    {
        if (a._euler == b._euler)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator !=(Angle a, Angle b)
    {
        if (a._euler != b._euler)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Returns formatted difference (clockwise) in eulers.
    public float GetDiff(Angle to)
    {
        float diff = to._euler - _euler;
        if (diff > 180)
        {
            diff = -1 * ((360 - to._euler) + _euler);
        }
        else if (diff < -180)
        {
            diff = (360 - _euler) + to._euler;
        }
        return diff;
    }

    public override bool Equals(object obj)
    {
        var angle = obj as Angle;
        return angle != null &&
               Euler == angle.Euler &&
               _euler == angle._euler;
    }

    public override int GetHashCode()
    {
        var hashCode = 87950164;
        hashCode = hashCode * -1521134295 + Euler.GetHashCode();
        hashCode = hashCode * -1521134295 + _euler.GetHashCode();
        return hashCode;
    }
}
