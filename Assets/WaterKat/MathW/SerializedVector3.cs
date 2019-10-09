using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterKat.MathW
{
    [System.Serializable]
    public class SerializedVector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3 vector3
        {
            get
            {
                return new Vector3(x, y, z);
            }
        }

        public SerializedVector3(Vector3 vector3)
        {
            this.x = vector3.x;
            this.y = vector3.y;
            this.z = vector3.z;
        }

        public static explicit operator Vector3(SerializedVector3 sVector3)
        {
            return sVector3.vector3;
        }
    }
}