using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
using System;

namespace WaterKat.MathW
{
    [System.Serializable]
    public struct ComplexF
    {
        public float real;
        public float imaginary;

        public ComplexF (float _real)
        {
            this.real = _real;
            this.imaginary = 0;
        }

        public ComplexF(float _real,float _imaginary)
        {
            this.real = _real;
            this.imaginary = _imaginary;
        }

        public static ComplexF From_Polar(float radius, float theta)
        {
            ComplexF data = new ComplexF(radius * (float)Math.Cos(theta*Constants.DegToRad), radius * (float)Math.Sin(theta * Constants.DegToRad));
            return data;
        }
    }
}