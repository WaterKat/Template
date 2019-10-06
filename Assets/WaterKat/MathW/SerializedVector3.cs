using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializedVector3 : MonoBehaviour
{
    public float x;
    public float y;
    public float z;
    /*
    public  SerializedVector3(Vector3 vector3) 
    {
        if (digit > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(digit), "Digit cannot be greater than nine.");
        }
        this.digit = digit;
    }

    public static implicit operator (Digit d) => d.digit;
    public static explicit operator Digit(byte b) => new Digit(b);
    */
}
