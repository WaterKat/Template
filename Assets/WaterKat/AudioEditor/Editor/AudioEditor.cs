using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System;

namespace WaterKat.AudioEditor
{
    public class AudioEditor : EditorWindow
    {
        AudioClip _sourceClip;
        AudioClip sourceClip
        {
            set { if (_sourceClip != value) { _sourceClip = value; onSourceClipUpdate(); } }
            get { return _sourceClip; }
        }

        Texture2D[] TextureBuffer = new Texture2D[5];

        Slider PlayBar = new Slider();
        MinMaxSlider ZoomSlider = new MinMaxSlider();


        [MenuItem("WaterKat/AudioEditor", false, 0)]
        static void Init()
        {
            AudioEditor window = (AudioEditor)EditorWindow.GetWindow(typeof(AudioEditor));
            window.Show();
        }

        TimeTexture test = new TimeTexture();
        int ImageWidth = 10;
        int ImageHeight = 20;
        Vector2Int previousScreenSize = Vector2Int.one;

        Texture2DStack AudioPreviewStack;

        GUIStyle ImagePreviews;
        GUIStyle LabelRightAligned;
        GUIStyle LabelCenterAligned;

        public void CreateStyles()
        {
            ImagePreviews = new GUIStyle() { margin = new RectOffset(7, 7, 0, 0) };
            LabelCenterAligned = new GUIStyle(GUI.skin.label);
            LabelCenterAligned.alignment = TextAnchor.MiddleCenter;
            LabelRightAligned = new GUIStyle(GUI.skin.label);
            LabelRightAligned.alignment = TextAnchor.MiddleRight;
        }

        private void onSourceClipUpdate()
        {
            if (sourceClip == null) { return; }

            PlayBar.maxLimit = (float)sourceClip.samples * (float)sourceClip.channels / (float)sourceClip.frequency;
            PlayBar.minLimit = 0;
            PlayBar.value = 0;
            ZoomSlider.maxLimit = (float)sourceClip.samples * (float)sourceClip.channels / (float)sourceClip.frequency;
            ZoomSlider.minLimit = 0;
            ZoomSlider.ReAlignValues();

        }

        private void OnWindowSizeUpdate()
        {
            ImageWidth = (int)position.width - 14;
            AudioPreviewStack.ResizeRoot(ImageWidth, ImageHeight*4);
        }

        private void OnEnable()
        {
            AudioPreviewStack = new Texture2DStack();
            AudioPreviewStack.AddToStack(SpectrographStackFunc);
            AudioPreviewStack.AddToStack(TimeMarkerStackFunc);
            AudioPreviewStack.RecalculateTextureStack();
        }

        private void OnGUI()
        {
            if (ImagePreviews == null) { CreateStyles(); }

            if (AudioPreviewStack == null) { AudioPreviewStack = new Texture2DStack(); }

            if (previousScreenSize != new Vector2Int(Mathf.FloorToInt(position.width), Mathf.FloorToInt(position.height)))
            {
                OnWindowSizeUpdate();
                previousScreenSize.x = Mathf.FloorToInt(position.width);
                previousScreenSize.y = Mathf.FloorToInt(position.height);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("WaterKat Audio Editor", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            sourceClip = EditorGUILayout.ObjectField("Source Audio", sourceClip, typeof(AudioClip), false) as AudioClip;
            if (sourceClip == null) { return; }

            GUILayout.Space(10);

            {
                GUILayout.Label("Audio Preview", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();
                GUILayout.Label(timeFloatToString(0));
                GUILayout.Label(timeFloatToString(PlayBar.value), LabelCenterAligned);
                GUILayout.Label(timeFloatToString(PlayBar.maxLimit), LabelRightAligned);
                GUILayout.EndHorizontal();
                PlayBar.value = GUILayout.HorizontalSlider(PlayBar.value, PlayBar.minLimit, PlayBar.maxLimit);

                GUILayout.BeginHorizontal();
                GUILayout.Button("|◀");
                GUILayout.Button("▮▮");
                GUILayout.Button("▶");
                GUILayout.Button("|✖|", new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold });
                GUILayout.Button("►|");
                GUILayout.EndHorizontal();
            }//AudioControls

            GUILayout.Space(20);

            {
                GUILayout.Label("Zoom", EditorStyles.boldLabel);

                GUILayout.Box(test.SpectrographIcon(ImageWidth, ImageHeight, sourceClip), ImagePreviews);

                EditorGUILayout.MinMaxSlider(ref ZoomSlider.minValue, ref ZoomSlider.maxValue, ZoomSlider.minLimit, ZoomSlider.maxLimit);
                //ZoomSlider.minValue = Mathf.FloorToInt(ZoomSlider.minValue);
                //ZoomSlider.maxValue = Mathf.Min(Mathf.CeilToInt(ZoomSlider.maxValue), ZoomSlider.maxLimit);
            }//Zoom

            GUILayout.Space(20);


            GUILayout.Label("Audio Preview", EditorStyles.boldLabel);


            //Texture2D testtexture = test.renderTimeMarkers((int)position.width - 14, 20, ZoomSlider);

            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(timeFloatToString(ZoomSlider.minValue));
                GUILayout.Label(timeFloatToString(ZoomSlider.maxValue), LabelRightAligned);
                GUILayout.EndHorizontal();



                GUILayout.Space(15);

                AudioPreviewStack.RecalculateTextureStack();
                GUILayout.Box(AudioPreviewStack.finalTexture2D, ImagePreviews);

                Rect barRect = GUILayoutUtility.GetLastRect();
                Vector2 mousePosition = Event.current.mousePosition;

                if (Event.current.type == EventType.Repaint && barRect.Contains(mousePosition))
                {
                    float relativeMousePosition = relativePosition(mousePosition.x, barRect.x, barRect.x + barRect.width);
                    float relativeTime = Mathf.Lerp(ZoomSlider.minValue, ZoomSlider.maxValue, relativeMousePosition);
                    GUI.Label(new Rect(mousePosition.x-150, barRect.y-(ImageHeight*3), 300, 100), timeFloatToString(relativeTime),LabelCenterAligned);
                }
            }
        }

        Texture2D SpectrographStackFunc(Texture2D _inputTexture)
        {
            Color innerFill = new Color(0.04705882352f, 0.60392156862f, 0.70196078431f);
            Color outerFill = new Color(0.04705882352f, 0.60392156862f, 0.70196078431f);
            int originOffset = _inputTexture.height / 2;

            if (sourceClip == null) { return _inputTexture; }

            float[] audioData = new float[sourceClip.samples * sourceClip.channels];
            sourceClip.GetData(audioData, 0);

            for (int x = 0; x < _inputTexture.width; x++)
            {
                float startIndex = ZoomSlider.minValue / (ZoomSlider.maxLimit - ZoomSlider.minLimit) * audioData.Length;
                float endIndex = ZoomSlider.maxValue / (ZoomSlider.maxLimit - ZoomSlider.minLimit) * audioData.Length;
                float relativeIndex = (float)x / (float)_inputTexture.width;
                int currentIndex = Mathf.FloorToInt(((1 - relativeIndex) * startIndex) + (relativeIndex * endIndex));

                float scaledSample = audioData[currentIndex] * originOffset;

                for (int i = 0; Mathf.Abs(i) < Mathf.Abs(scaledSample); i += (int)Mathf.Sign(scaledSample))
                {
                    _inputTexture.SetPixel(x, Mathf.Clamp(i + originOffset, 0, _inputTexture.height), innerFill);
                }

                _inputTexture.SetPixel(x, (int)scaledSample + originOffset, outerFill);
            }

            _inputTexture.Apply();
            return _inputTexture;
        }

        Texture2D CursorLineStackFunc(Texture2D _inputTexture)
        {
            if (sourceClip == null) { return _inputTexture; }

            Color lineFill = new Color(0.04705882352f, 0.60392156862f, 0.70196078431f);
            float startTime = ZoomSlider.minValue;
            float endTime = ZoomSlider.maxValue;

            int relativeX = Mathf.FloorToInt(AudioEditor.relativePosition(Event.current.mousePosition.x, startTime, endTime) * sizeX);
                for (int y = sizeY - Mathf.FloorToInt(maxMarkerLength * BarLengths[i]); y < sizeY; y++)
                {
                    _inputTexture.SetPixel(relativeX, y, new Color(1 * (1 - Mathf.Pow((float)i / times, 2)), 0, (float)i / times, 1));
                }
            }

            _inputTexture.Apply();
            return _inputTexture;
        }

        Texture2D TimeMarkerStackFunc(Texture2D _inputTexture)
        {
            float startTime = ZoomSlider.minValue;
            float endTime = ZoomSlider.maxValue;
            int sizeX = _inputTexture.width;
            int sizeY = _inputTexture.height;
            int maxMarkerLength = 15;

            _inputTexture.wrapMode = TextureWrapMode.Clamp;

            float[] Divisions = new float[] { 60, 30, 15, 5, 1, 0.5f, 0.25f };
            float[] BarLengths = new float[] { 1, .78f, .7f, .6f, .48f, .3f, .2f };
            int times = Divisions.Length;
            if (endTime - startTime > 10f) { times--; }
            if (endTime - startTime > 30f) { times--; }
            if (endTime - startTime > 60f) { times--; }

            for (int i = times - 1; i >= 0; i--)
            {
                int Bars = Mathf.FloorToInt(endTime / Divisions[i]);
                for (int x = 1; x <= Bars; x++)
                {
                    int relativeX = Mathf.FloorToInt(AudioEditor.relativePosition(Divisions[i] * x, startTime, endTime) * sizeX);
                    for (int y = sizeY - Mathf.FloorToInt(maxMarkerLength * BarLengths[i]); y < sizeY; y++)
                    {
                        _inputTexture.SetPixel(relativeX, y, new Color(1 * (1 - Mathf.Pow((float)i / times, 2)), 0, (float)i / times, 1));
                    }
                }
            }
            for (int x = 0; x < sizeX; x++)
            {
                _inputTexture.SetPixel(x, sizeY - 1, new Color(1, 0, 0, 1));
            }
            for (int y = sizeY-maxMarkerLength; y < sizeY; y++)
            {
                _inputTexture.SetPixel(0, y, new Color(1, 0, 0, 1));
            }
            _inputTexture.Apply();
            return _inputTexture;
        }

        string timeFloatToString(float desiredTime)
        {
            return Mathf.FloorToInt(desiredTime / 60).ToString("00") + ":" + ((desiredTime) % 60).ToString("00.00");
        }
        public static float relativePosition(float point, float minLimit, float maxLimit)
        {
            float relativePoint = point - minLimit;
            float relativeSize = maxLimit - minLimit;
            return (relativePoint / relativeSize);
        }
    }



    class TimeTexture
    {
        //public float TotalLength;
        //public float startLength;
        //public float endLength;
        //public Texture2D lastTexture;

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
            for (int i = 0; i < table.Length; i++)
            {
                table[i] = table[i] / largestValue;
            }
        }

        public Texture2D SpectrographIcon(int sizeX, int sizeY, AudioClip audioClip)
        {
            int originOffset = sizeY / 2;
            Texture2D audioTexture = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);

            if (audioClip == null) { return audioTexture; }

            float[] audioData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(audioData, 0);
            normalizeData(audioData);

            for (int x = 0; x < sizeX; x++)
            {
                int sampleIndex = Mathf.FloorToInt(AudioEditor.relativePosition(x, 0, sizeX) * audioData.Length);
                float scaledSample = audioData[sampleIndex] * originOffset;

                for (int i = 0; Mathf.Abs(i) < Mathf.Abs(scaledSample); i += (int)Mathf.Sign(scaledSample))
                {
                    audioTexture.SetPixel(x, Mathf.Clamp(i + originOffset, 0, sizeY), new Color(0, 0.5f, 0.5f, 1));
                }

                audioTexture.SetPixel(x, (int)scaledSample + originOffset, new Color(0, 0.5f, 0.5f, 1));
            }

            audioTexture.Apply();
            return audioTexture;
        }

        public Texture2D ZoomedWaveform(int sizeX, int sizeY, MinMaxSlider zoomSlider, AudioClip audioClip)
        {
            int originOffset = sizeY / 2;
            Texture2D audioTexture = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);

            if (audioClip == null) { return audioTexture; }

            float[] audioData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(audioData, 0);
            normalizeData(audioData);

            for (int x = 0; x < sizeX; x++)
            {
                int sampleIndex = Mathf.FloorToInt(AudioEditor.relativePosition(x, 0, sizeX) * audioData.Length);
                float scaledSample = audioData[sampleIndex] * originOffset;

                for (int i = 0; Mathf.Abs(i) < Mathf.Abs(scaledSample); i += (int)Mathf.Sign(scaledSample))
                {
                    audioTexture.SetPixel(x, Mathf.Clamp(i + originOffset, 0, sizeY), new Color(0, 0.5f, 0.5f, 1));
                }

                audioTexture.SetPixel(x, (int)scaledSample + originOffset, new Color(0, 0.5f, 0.5f, 1));
            }

            audioTexture.Apply();
            return audioTexture;
        }

        public static Texture2D renderTimeMarkers(int sizex, int sizey, MinMaxSlider zoomSlider)
        {
            float startTime = zoomSlider.minValue;
            float endTime = zoomSlider.maxValue;

            Texture2D tempTexture = new Texture2D((int)sizex, (int)sizey, TextureFormat.ARGB32, false);
            tempTexture.wrapMode = TextureWrapMode.Clamp;

            float[] Divisions = new float[] { 60, 30, 15, 5, 1, 0.5f, 0.25f };
            float[] BarLengths = new float[] { 1, .78f, .7f, .6f, .48f, .3f, .2f };
            int times = Divisions.Length;
            if (endTime - startTime > 10f) { times--; }
            if (endTime - startTime > 30f) { times--; }
            if (endTime - startTime > 60f) { times--; }
            for (int i = times - 1; i >= 0; i--)
            {
                int Bars = Mathf.FloorToInt(endTime / Divisions[i]);
                for (int x = 1; x <= Bars; x++)
                {
                    int relativeX = Mathf.FloorToInt(AudioEditor.relativePosition(Divisions[i] * x, startTime, endTime) * sizex);
                    for (int y = sizey - Mathf.FloorToInt(sizey * BarLengths[i]); y < sizey; y++)
                    {
                        tempTexture.SetPixel(relativeX, y, new Color(1 * (1 - Mathf.Pow((float)i / times, 2)), 0, (float)i / times, 1));
                    }
                }
            }
            for (int x = 0; x < sizex; x++)
            {
                tempTexture.SetPixel(x, sizey - 1, new Color(1, 0, 0, 1));
            }
            for (int y = 0; y < sizey; y++)
            {
                tempTexture.SetPixel(0, y, new Color(1, 0, 0, 1));
            }
            tempTexture.Apply();
            return tempTexture;
        }

        float TimefromSamples(int samples, int samplerate)
        {
            if (samplerate == 0)
            {
                samplerate = 44100;
            }
            return ((float)samples) / samplerate;
        }
    }

    class Texture2DStack
    {
        Texture2D _root;
        Texture2D root
        {
            get
            {
                return _root;
            }
            set
            {
                if (_root != value)
                {
                    _root = value;
                    RefreshStack();
                }
            }
        }

        List<Func<Texture2D, Texture2D>> functionStack = new List<Func<Texture2D, Texture2D>>();

        List<Texture2D> textureStack = new List<Texture2D>();
        public Texture2D finalTexture2D
        {
            get
            {
                if (textureStack.Count > 0)
                {
                    return textureStack[textureStack.Count - 1];
                }
                return root;
            }
        }

        void AddToTextureStack(int _index, Texture2D texture2D)
        {
            if (textureStack.Count > _index)
            {
                textureStack[_index] = texture2D;
            }
            else
            {
                textureStack.Add(texture2D);
            }
        }
        public void AddToStack(Func<Texture2D, Texture2D> stackFunction)
        {
            functionStack.Add(stackFunction);
            RecalculateTextureStackAtPoint(functionStack.Count);
        }
        public void AddToStack(int _index, Func<Texture2D, Texture2D> stackFunction)
        {
            functionStack.Insert(_index, stackFunction);
            RecalculateTextureStackAtPoint(_index);
        }

        public void RecalculateTextureStack()
        {
            if (functionStack.Count < 1) { return; }
            for (int i = 0; i < functionStack.Count; i++)
            {
                Texture2D originalTexture;
                if (i > 0)
                { originalTexture = textureStack[i - 1]; }
                else
                { originalTexture = root; }
                Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.ARGB32, false);
                Graphics.CopyTexture(originalTexture, newTexture);
                AddToTextureStack(i, functionStack[i](newTexture));
            }
        }
        void RecalculateTextureStackAtPoint(int _index)
        {
            if (functionStack.Count <= _index) { return; }
            if (_index < 1) { RecalculateTextureStack(); return; }
            for (int i = _index; i < functionStack.Count; i++)
            {
                Texture2D originalTexture = textureStack[i - 1];
                Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height);
                Graphics.CopyTexture(originalTexture, newTexture);
                AddToTextureStack(i, functionStack[i](newTexture));
            }
        }

        void RefreshStack()
        {
            textureStack.Clear();
            RecalculateTextureStack();
        }

        public void ResizeRoot(int sizeX, int sizeY)
        {
            root = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);
            RecalculateTextureStack();
        }
        public void ResizeRoot(int sizeX, int sizeY, Color backgroundColor)
        {
            root = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);
            Color[] colors = new Color[sizeX * sizeY];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = backgroundColor;
            }
            root.SetPixels(colors);
            root.Apply();
            RecalculateTextureStack();
        }

        public Texture2DStack()
        {
            root = Texture2D.whiteTexture;
        }
        public Texture2DStack(int sizeX, int sizeY)
        {
            root = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);
        }
        public Texture2DStack(int sizeX, int sizeY, Color backgroundColor)
        {
            root = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);
            Color[] colors = new Color[sizeX * sizeY];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = backgroundColor;
            }
            root.SetPixels(colors);
            root.Apply();
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

        internal void ReAlignValues()
        {
            minValue = 0;
            maxValue = maxLimit;
        }
    }
}