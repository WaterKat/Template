using UnityEngine;
using UnityEditor;
//using UnityEngine.UIElements;
using Unity.Collections;

namespace WaterKat.AudioManager
{
    public class TestAudioEditor : EditorWindow
    {
        AudioClip sourceClip;
        AudioClip SourceClip
        {
            get { return sourceClip; }
            set { if (value == sourceClip) { return; } sourceClip = value; miniDisplay = UnzoomedSpectrographTexture(sourceClip, 40); ; }
        }
        //AudioClip ExportClip;

        Texture miniDisplay;
        Texture darkenedAudioTexture;

        // Add menu named "My Window" to the Window menu
        [MenuItem("WaterKat/TestAudioEditor", false, 50)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            TestAudioEditor window = (TestAudioEditor)EditorWindow.GetWindow(typeof(TestAudioEditor));
            window.Show();
            
        }

        void UpdateAudioTexture()
        {
            miniDisplay = UnzoomedSpectrographTexture(sourceClip, 16);
            darkenedAudioTexture = UpdateDarkenedTexture(SpectrographTexture(sourceClip, 4));
        }
        
        class Slider {
            public float minVal = 0;
            public float minLimit = 0;
            public float maxVal = 100;
            public float maxLimit = 100;
            public float originalminLimit;
            public float originalmaxLimit;

            public Slider() { LogMinMaxLimit(); }
            public void LogMinMaxLimit()
            {
                originalminLimit = minLimit;
                originalmaxLimit = maxLimit;
            }
        }

        Slider ZoomSlider = new Slider();
        Slider TrimSlider = new Slider();
        Vector2 OldScreenSize = Vector2.zero;

        void OnGUI()
        {
            if ((OldScreenSize.x != position.width) || (OldScreenSize.y != position.height))
            {
                UpdateScreen();
                OldScreenSize = new Vector2(position.width, position.height);
            }
            GUILayout.Label("AudioViewer", EditorStyles.boldLabel);
            SourceClip = EditorGUILayout.ObjectField("Source Audio", SourceClip, typeof(AudioClip), false) as AudioClip;

            EditorGUILayout.LabelField("Zoom", EditorStyles.boldLabel);
            if (miniDisplay != null)
            {
                GUILayout.Box(miniDisplay, new GUIStyle() { margin = new RectOffset(), fixedHeight = miniDisplay.height, fixedWidth = miniDisplay.width }, new GUILayoutOption[] { });
            }

            { //This controls the ZoomSlider
                float min = ZoomSlider.minVal;
                float max = ZoomSlider.maxVal;
                EditorGUILayout.MinMaxSlider(ref ZoomSlider.minVal, ref ZoomSlider.maxVal, ZoomSlider.minLimit, ZoomSlider.maxLimit);
                if ((ZoomSlider.minVal != min) || (ZoomSlider.maxVal != max))
                {
                    TrimSlider.minLimit = ZoomSlider.minVal;
                    TrimSlider.maxLimit = ZoomSlider.maxVal;
                }
            }

            EditorGUILayout.LabelField("Trim", EditorStyles.boldLabel);

            GUILayout.Box(darkenedAudioTexture, new GUIStyle() { margin = new RectOffset(), fixedHeight = position.width / 4, fixedWidth = position.width }, new GUILayoutOption[] { });

            {
                float min = TrimSlider.minVal;
                float max = TrimSlider.maxVal;
                EditorGUILayout.MinMaxSlider(ref TrimSlider.minVal, ref TrimSlider.maxVal, TrimSlider.minLimit, TrimSlider.maxLimit);
                if ((ZoomSlider.minVal != min) || (ZoomSlider.maxVal != max))
                {
                    //darkenedAudioTexture = SpectrographTexture(sourceClip, 4);
                    darkenedAudioTexture = UpdateDarkenedTexture(SpectrographTexture(sourceClip, 4));
                }

            }


            //ExportClip = EditorGUILayout.ObjectField("Export Audio", ExportClip, typeof(AudioClip), false) as AudioClip;
            if (
            GUILayout.Button("Export!"))
            {
                float[] sourceData = new float[sourceClip.samples*sourceClip.channels];
                sourceClip.GetData(sourceData, 0);
                float[] newdata = new float[(int)((TrimSlider.maxVal - TrimSlider.minVal) / TrimSlider.originalmaxLimit* sourceData.Length)];
                int startData = (int)(TrimSlider.minVal / TrimSlider.originalmaxLimit * sourceData.Length);
                for (int  i = 0;  i <newdata.Length;  i++)
                {
                    newdata[i] =  sourceData[startData+i];
                }

                AudioClip test = AudioClip.Create("Output", newdata.Length,sourceClip.channels,SourceClip.frequency,false);
                test.SetData(newdata,0);
                SavWav.Save("Test", test);
                Debug.Log("OldSamples" + sourceClip.samples);
                //Debug.Log("NewSamples" + ExportClip.samples);
                Debug.Log("DONE");
            }
        }
        void UpdateScreen()
        {
            UpdateAudioTexture();
        }

        
        Texture UpdateDarkenedTexture(Texture inputTexture)
        {
            if (inputTexture == null) { return null; }
            int width = (int)inputTexture.width;
            int height = (int)inputTexture.height;
            Texture2D audioTexture = new Texture2D(width,height, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(inputTexture, audioTexture);
            float minPixel = (TrimSlider.minVal-TrimSlider.minLimit)/(TrimSlider.maxLimit-TrimSlider.minLimit)* width;
            float maxPixel = (TrimSlider.maxVal - TrimSlider.minLimit) / (TrimSlider.maxLimit - TrimSlider.minLimit) * width;

            for (int x = 0; x < width; x++)
            {
                if ((x == (int)minPixel) || (x == (int)maxPixel))
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color newColor = audioTexture.GetPixel(x, y);
                        newColor /= 2;
                        newColor.a = 1;
                        audioTexture.SetPixel(x, y, newColor);
                    }
                }
            }

            audioTexture.Apply();
            return audioTexture;
        }

        Texture UnzoomedSpectrographTexture(AudioClip audioClip, int heightRatio)
        {
            int width = (int)position.width;
            int height = (int)position.width / heightRatio;
            int originOffset = height / 2;
            Texture2D audioTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);

            if (audioClip == null) { return audioTexture; }
            /*
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    audioTexture.SetPixel(x, y, new Color(0, 0, 0));
                }
            }
            */
            float[] audioData = new float[sourceClip.samples * sourceClip.channels];
            audioClip.GetData(audioData, 0);

            for (int x = 0; x < width; x++)
            {
                //                int targetSample = Mathf.FloorToInt((x* audioData.Length) /width);
                //float startIndex = ZoomSlider.minVal / (ZoomSlider.maxLimit - ZoomSlider.minLimit) * audioData.Length;
                //float endIndex = ZoomSlider.maxVal / (ZoomSlider.maxLimit - ZoomSlider.minLimit) * audioData.Length;
                float startIndex = 0;
                float endIndex = audioData.Length;
                float relativeIndex = (float)x / (float)width;
                int currentIndex = Mathf.FloorToInt(((1 - relativeIndex) * startIndex) + (relativeIndex * endIndex));


                //int targetSample = Mathf.FloorToInt((TrimSlider.minVal / TrimSlider.maxLimit)*audioData.Length)+Mathf.FloorToInt(((float)x/width)*TrimmedRange);
                //Debug.Log("TargetSample" + targetSample);
                float scaledSample = audioData[currentIndex] * originOffset;

                for (int i = 0; Mathf.Abs(i) < Mathf.Abs(scaledSample); i += (int)Mathf.Sign(scaledSample))
                {
                    audioTexture.SetPixel(x, Mathf.Clamp(i + originOffset, 0, height), new Color(1, 0, 0));
                }

                audioTexture.SetPixel(x, (int)scaledSample + originOffset, new Color(1, 0, 0));

            }



            audioTexture.Apply();
            return audioTexture;
        }

        Texture SpectrographTexture(AudioClip audioClip,int heightRatio)
        {
            int width = (int)position.width;
            int height = (int)position.width/heightRatio;
            int originOffset = height / 2;
            Texture2D audioTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);

            if (audioClip == null) { return audioTexture; }
            /*
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (y == originOffset)
                    {
                        audioTexture.SetPixel(x, y, new Color(0.70f, 0.35f, 0.07f));
                        continue;
                    }
                    if (Mathf.Repeat(x,10) == 0)
                    {
                        audioTexture.SetPixel(x, y, new Color(1.00f, 0.60f, 0.27f));
                        continue;
                    } 
                    audioTexture.SetPixel(x, y, new Color(1.00f, 0.70f, 0.46f));
                }
            }*/
            
            float[] audioData = new float[sourceClip.samples * sourceClip.channels];
            audioClip.GetData(audioData, 0);
            
            for (int x = 0; x < width; x++)
            {
                //                int targetSample = Mathf.FloorToInt((x* audioData.Length) /width);
                float startIndex =ZoomSlider.minVal / (ZoomSlider.maxLimit - ZoomSlider.minLimit) * audioData.Length;
                float endIndex = ZoomSlider.maxVal / (ZoomSlider.maxLimit - ZoomSlider.minLimit) * audioData.Length;
                float relativeIndex = (float)x / (float)width;
                int currentIndex = Mathf.FloorToInt(((1 - relativeIndex) * startIndex) + (relativeIndex * endIndex));


                //int targetSample = Mathf.FloorToInt((TrimSlider.minVal / TrimSlider.maxLimit)*audioData.Length)+Mathf.FloorToInt(((float)x/width)*TrimmedRange);
                //Debug.Log("TargetSample" + targetSample);
                float scaledSample = audioData[currentIndex]*originOffset;

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