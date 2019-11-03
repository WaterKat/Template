using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
using System;

namespace WaterKat.MathW
{
    public class Constants
    {
        public static double DegToRad
        {
            get
            {
                return (Math.PI / 180d);
            }
        }
        public static double RadToDeg
        {
            get
            {
                return (180d / Math.PI);
            }
        }
    }
}