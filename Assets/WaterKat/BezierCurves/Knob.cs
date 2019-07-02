using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterKat.Benzier
{
    public class Node2D : MonoBehaviour
    {
        Vector3 position;
        public Vector2 Position { get; set; }
        Vector2 knobA;
        public Vector2 KnobA
        {
            get
            {
                return knobA;
            }
            set
            {
                Vector2 vector2 = value;
                vector2.x = Mathf.Min(vector2.x, position.x);
                knobA = vector2;
            }
        }
        Vector2 knobB;
        Vector2 KnobB
        {
            get
            {
                return knobB;
            }
            set
            {
                Vector2 vector2 = value;
                vector2.x = Mathf.Max(vector2.x, position.x);
                knobB = vector2;
            }
        }
        bool Symmetric;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}