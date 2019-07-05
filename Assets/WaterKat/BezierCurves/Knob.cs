using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterKat.Benzier
{
    public class Node2D : MonoBehaviour
    {
        Vector3 position;
        public Vector2 Position { get; set; }
        Vector2 _inKnob;
        public Vector2 inKnob
        {
            get
            {
                return _inKnob;
            }
            set
            {
                Vector2 vector2 = value;
                vector2.x = Mathf.Min(vector2.x, position.x);
                _inKnob = vector2;
            }
        }
        Vector2 _outKnob;
        public Vector2 outKnob
        {
            get
            {
                return outKnob;
            }
            set
            {
                Vector2 vector2 = value;
                vector2.x = Mathf.Max(vector2.x, position.x);
                outKnob = vector2;
            }
        }
        bool Smooth;
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

    public class BezierCurve
    {
        Node2D nodeA;
        Node2D nodeB;

        float evaluateFloat(float t,float x0, float x1,float x2,float x3)
        {
           return (Mathf.Pow((1 - t), 3) * x0) + (3 * Mathf.Pow((1 - t), 2) * t * x1) + (3 * (1 - t) * Mathf.Pow(t, 2) * x2) + (Mathf.Pow(t, 3) * x3);
        }
        public float Evaluate(float t)
        {
            //float xf = evaluateFloat(t, nodeA.Position.x, nodeA.outKnob.x, nodeB.inKnob.x, nodeB.Position.x);
            float yf = evaluateFloat(t, nodeA.Position.y, nodeA.outKnob.y, nodeB.inKnob.y, nodeB.Position.y);
            return yf;
        }
    }
}