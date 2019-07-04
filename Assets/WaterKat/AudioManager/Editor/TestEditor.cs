using UnityEngine;
using UnityEditor;

namespace WaterKat.Audio
{
    public class AudioEditor : EditorWindow
    {
        AudioClip sourceClip;
        AudioClip SourceClip
        {
            get { return sourceClip; }
            set { if (value == sourceClip) { return; } sourceClip = value; UpdateAudioTexture(); }
        }
        AudioClip ExportClip;

        Texture CurrentAudioTexture;

        string myString = "Hello World";
        bool groupEnabled;
        bool myBool = true;
        float myFloat = 1.23f;

        // Add menu named "My Window" to the Window menu
        [MenuItem("WaterKat/AudioEditor", false, 50)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            AudioEditor window = (AudioEditor)EditorWindow.GetWindow(typeof(AudioEditor));
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("General", EditorStyles.boldLabel);
            SourceClip = EditorGUILayout.ObjectField("Source Audio", SourceClip, typeof(AudioClip), false) as AudioClip;

            GUILayout.Label(CurrentAudioTexture,new GUILayoutOption[] { GUILayout.ExpandHeight(true),GUILayout.ExpandWidth(true)});

            ExportClip = EditorGUILayout.ObjectField("Export Audio", ExportClip, typeof(AudioClip), false) as AudioClip;

            myString = EditorGUILayout.TextField("Text Field", myString);

            groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            myBool = EditorGUILayout.Toggle("Toggle", myBool);
            myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
            EditorGUILayout.EndToggleGroup();
        }

        void UpdateAudioTexture()
        {
            CurrentAudioTexture = SpectrographTexture(SourceClip);
        }

        Texture SpectrographTexture(AudioClip audioClip)
        {
            int width = 320;
            int height = 200;
            int originOffset = height / 2;
            Texture2D audioTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);

            if (audioClip == null) { return audioTexture; }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (y == originOffset)
                    {
                        audioTexture.SetPixel(x, y, new Color(0.70f, 0.35f, 0.07f));
                        continue;
                    }
                    if (Mathf.Repeat(x,50) == 0)
                    {
                        audioTexture.SetPixel(x, y, new Color(1.00f, 0.60f, 0.27f));
                        continue;
                    } 
                    audioTexture.SetPixel(x, y, new Color(1.00f, 0.70f, 0.46f));
                }
            }
            
            float[] audioData = new float[audioClip.samples];
            audioClip.GetData(audioData, 4000);
            
            for (int x = 0; x < width; x++)
            {
                int targetSample = Mathf.FloorToInt((x* audioData.Length) /width);
                float scaledSample = audioData[targetSample]*originOffset;

                for (int i = 0; Mathf.Abs(i) < Mathf.Abs(scaledSample); i+=(int)Mathf.Sign(scaledSample))
                {
                    audioTexture.SetPixel(x, Mathf.Clamp(i+originOffset,0,height), new Color(0.04705882352f, 0.60392156862f, 0.70196078431f));
                }

                audioTexture.SetPixel(x, (int)scaledSample+originOffset, new Color(0.04705882352f, 0.60392156862f, 0.70196078431f));

            }
            


            audioTexture.Apply();
            return audioTexture;
        }
    }
}