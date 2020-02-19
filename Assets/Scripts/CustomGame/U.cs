using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class U
{
    public static float WrapAngle(float value, float target)
    {
        if (target - value >= 180) return target - 360;
        else if (target - value <= -180) return target + 360;
        else return target;
    }
}