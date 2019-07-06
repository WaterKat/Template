using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace WaterKat.AudioEditor
{
    public class AudioEditor : EditorWindow
    {
        AudioClip sourceClip;

        [MenuItem("WaterKat/AudioEditor", false, 0)]
        static void Init()
        {
            AudioEditor window = (AudioEditor)EditorWindow.GetWindow(typeof(AudioEditor));
            window.Show();
        }
    }

    class MinMaxSlider
    {
        public float minVal = 0;
        public float minLimit = 0;
        public float maxVal = 100;
        public float maxLimit = 100;
        public float originalminLimit;
        public float originalmaxLimit;

        public MinMaxSlider()
        {
            originalminLimit = minLimit;
            originalmaxLimit = maxLimit;
        }
    }
}