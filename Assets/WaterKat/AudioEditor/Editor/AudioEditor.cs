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

        Texture2D[] TextureBuffer = new Texture2D[5];

        Slider PlayBar = new Slider();
        MinMaxSlider ZoomSlider = new MinMaxSlider() { maxLimit = 500 };

        [MenuItem("WaterKat/AudioEditor", false, 0)]
        static void Init()
        {
            AudioEditor window = (AudioEditor)EditorWindow.GetWindow(typeof(AudioEditor));
            window.Show();
        }

        TimeTexture test = new TimeTexture();

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("WaterKat Audio Editor", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold });
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            sourceClip = EditorGUILayout.ObjectField("Source Audio", sourceClip, typeof(AudioClip), false) as AudioClip;
            if (sourceClip == null) { return; }
            PlayBar.maxLimit = (float)sourceClip.samples*(float)sourceClip.channels / (float)sourceClip.frequency;

            GUILayout.Space(10);

            GUILayout.Label("Audio Preview", EditorStyles.boldLabel);
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("00:00.00");
                GUILayout.Label(Mathf.FloorToInt(PlayBar.value / 60).ToString("00") + ":" + ((PlayBar.value) % 60).ToString("00.00"), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                GUILayout.Label(Mathf.FloorToInt(PlayBar.maxLimit / 60).ToString("00") + ":" + ((PlayBar.maxLimit) % 60).ToString("00.00"), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight });
                GUILayout.EndHorizontal();
                GUILayout.HorizontalSlider(PlayBar.value, PlayBar.minLimit, PlayBar.maxLimit);
            }//PlayBar
            {
                GUILayout.BeginHorizontal();
                GUILayout.Button("|◀");
                GUILayout.Button("▮▮");
                GUILayout.Button("▶");
                GUILayout.Button("|✖|", new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold});
                GUILayout.Button("►|");
                GUILayout.EndHorizontal();
            }//AudioControls

            GUILayout.Space(20);

            Texture2D testtexture = test.getTextureTile(ZoomSlider.minValue,ZoomSlider.maxValue,(int)position.width-14, 20);


            GUILayout.Box(test.SpectrographIcon(testtexture.width,testtexture.height,sourceClip), new GUIStyle() { margin = new RectOffset(7, 7, 0, 0), fixedHeight = testtexture.height, fixedWidth = testtexture.width }, new GUILayoutOption[] { });

            PlayBar.maxLimit = ZoomSlider.maxValue;
            EditorGUILayout.MinMaxSlider(ref ZoomSlider.minValue, ref ZoomSlider.maxValue, ZoomSlider.minLimit, ZoomSlider.maxLimit);
            GUILayout.Box(testtexture, new GUIStyle() { margin = new RectOffset(7, 7, 0, 0), fixedHeight = testtexture.height, fixedWidth = testtexture.width }, new GUILayoutOption[] { });

        }
    }



    class TimeTexture
    {
        public float TotalLength;
        public float startLength;
        public float endLength;
        public Texture2D lastTexture;

        float relativePosition(float point, float minLimit,float maxLimit)
        {
            float relativePoint = point - minLimit;
            float relativeSize = maxLimit - minLimit;
            return (relativePoint / relativeSize);
        }

        void normalizeData(float[] table)
        {
            float largestValue = 0;
            foreach (float currentFloat in table)
            {
                if (Mathf.Abs(currentFloat) > largestValue)
                {
                    largestValue = Mathf.Abs(currentFloat);
                }
            }
            Debug.Log(largestValue);
            for (int i = 0; i < table.Length; i++)
            {
                table[i] = table[i] / largestValue;
            }
        }

        public Texture2D SpectrographIcon(int sizeX,int sizeY,AudioClip audioClip)
        {
            int originOffset = sizeY / 2;
            Texture2D audioTexture = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);

            if (audioClip == null) { return audioTexture; }

            float[] audioData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(audioData, 0);
            normalizeData(audioData);

            for (int x = 0; x < sizeX; x++)
            {
                int sampleIndex = Mathf.FloorToInt(relativePosition(x,0,sizeX)*audioData.Length);
                float scaledSample = audioData[sampleIndex] * originOffset;

                for (int i = 0; Mathf.Abs(i) < Mathf.Abs(scaledSample); i += (int)Mathf.Sign(scaledSample))
                {
                    audioTexture.SetPixel(x, Mathf.Clamp(i + originOffset, 0, sizeY), new Color(0, 0.5f, 0.5f,1));
                }

                audioTexture.SetPixel(x, (int)scaledSample + originOffset, new Color(0, 0.5f, 0.5f, 1));
            }
                       
            audioTexture.Apply();
            return audioTexture;
        }

        public Texture2D getTextureTile(float startTime,float clipLength,int sizex, int sizey)
        {
            Texture2D tempTexture = new Texture2D(sizex, sizey, TextureFormat.ARGB32, false);
            tempTexture.wrapMode = TextureWrapMode.Clamp;

            float[] Divisions = new float[] { 60, 30, 15, 1, 0.5f };
            float[] BarLengths = new float[] { 1, .8f, .6f, .4f, .2f };
            for (int i = 0; i < Divisions.Length; i++)
            {
                int Bars = Mathf.FloorToInt(clipLength / Divisions[i]);
                for (int x = 1; x <= Bars; x++)
                {
                    int relativeX = Mathf.FloorToInt(relativePosition(Divisions[i] * x, startTime, clipLength)*sizex);
                    for (int y = sizey-Mathf.FloorToInt(sizey*BarLengths[i]); y < sizey; y++)
                    {
                        tempTexture.SetPixel(relativeX, y, new Color(1, 0, 0, 1));
                    }
                }
            }

            for (int x = 0; x < sizex; x++)
            {
                tempTexture.SetPixel(x, sizey-1, new Color(1, 0, 0, 1));
            }
            for (int y = 0; y < sizey; y++)
            {
                tempTexture.SetPixel(0, y, new Color(1, 0, 0, 1));
            }

            tempTexture.Apply();
            lastTexture = tempTexture;
            return tempTexture;
            
        }

        float TimefromSamples(int samples,int samplerate)
        {
            if (samplerate == 0)
            {
                samplerate=44100;
            }
            return ((float)samples) / samplerate;
        }
    }

    class Slider
    {
        public float minLimit = 0;
        public float maxLimit = 100;
        public float value;
        public float originalMinLimit;
        public float originalMaxLimit;

        public Slider()
        {
            originalMinLimit = minLimit;
            originalMaxLimit = maxLimit;
        }
        public Slider(float _minLimit,float _maxLimit)
        {
            originalMinLimit = minLimit;
            originalMaxLimit = maxLimit;
            minLimit = _minLimit;
            maxLimit = _maxLimit;
        }
    }

    class MinMaxSlider
    {
        public float minValue = 0;
        public float minLimit = 0;
        public float maxValue = 100;
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