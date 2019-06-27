using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WK.Lerps
{
    public struct Vector3Lerp
    {
        public Vector3 startVector3;
        public Vector3 endVector3;
        public AnimationCurve animationCurve;

        Vector3Lerp(Vector3 _startVector3, Vector3 _endVector3)
        {
            startVector3 = _startVector3;
            endVector3 = _endVector3;
            animationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        }
        Vector3Lerp(Vector3 _startVector3, Vector3 _endVector3, AnimationCurve _animationCurve)
        {
            startVector3 = _startVector3;
            endVector3 = _endVector3;
            animationCurve = _animationCurve;
        }

        public Vector3 Evaluate(float input)
        {
            if (input == 0)
            {
                return startVector3;
            }
            if (input == 1)
            {
                return endVector3;
            }
            Vector3 difference = (endVector3 - startVector3);
            float curvedInput = animationCurve.Evaluate(Mathf.Clamp(input, 0, 1));
            return ((startVector3 + (difference * curvedInput)) + (endVector3 - (difference * (1 - curvedInput)))) / 2;
        }

        public static Vector3 Evaluate(Vector3 _startVector3, Vector3 _endVector3, float input)
        {
            return new Vector3Lerp(_startVector3, _endVector3).Evaluate(input);
        }
        public static Vector3 Evaluate(Vector3 _startVector3, Vector3 _endVector3, AnimationCurve _animationCurve, float input)
        {
            return new Vector3Lerp(_startVector3, _endVector3, _animationCurve).Evaluate(input);
        }
    }
    public struct FloatLerp
    {
        public float startFloat;
        public float endFloat;
        public AnimationCurve animationCurve;

        FloatLerp(float _startFloat, float _endFloat)
        {
            startFloat = _startFloat;
            endFloat = _endFloat;
            animationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        }
        FloatLerp(float _startFloat, float _endFloat, AnimationCurve _animationCurve)
        {
            startFloat = _startFloat;
            endFloat = _endFloat;
            animationCurve = _animationCurve;
        }

        public float Evaluate(float input)
        {
            if (input == 0)
            {
                return startFloat;
            }
            if (input == 1)
            {
                return endFloat;
            }
            float difference = (endFloat - startFloat);
            float curvedInput = animationCurve.Evaluate(Mathf.Clamp(input, 0, 1));
            return ((startFloat + (difference * curvedInput)) + (endFloat - (difference * (1 - curvedInput)))) / 2;
        }


        public static float Evaluate(float _startFloat,float _endFloat, float _input)
        {
            return new FloatLerp(_startFloat, _endFloat).Evaluate(_input);
        }
        public static float Evaluate(float _startFloat, float _endFloat, AnimationCurve _animationCurve,float _input)
        {
            return new FloatLerp(_startFloat, _endFloat, _animationCurve).Evaluate(_input);
        }
    }

    public class WKLerpManager : MonoBehaviour
    {
        public AnimationCurve testCurve = new AnimationCurve();
        // Start is called before the first frame update
        void Start()
        {
            Keyframe startKeyframe = new Keyframe() { time = 0, value = 0, outTangent = 0.5f, outWeight = 10f };

            Keyframe endKeyframe = new Keyframe() { time = 1f, value = 1f, inTangent = 0 };

            testCurve.AddKey(startKeyframe);
            testCurve.AddKey(endKeyframe);

        }


        
        // Update is called once per frame
        void Update()
        {

        }
    }
}