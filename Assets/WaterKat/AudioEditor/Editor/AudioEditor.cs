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
        MinMaxSlider TrimSlider = new MinMaxSlider();

        int ImageWidth = 10;
        int ImageHeight = 20;
        Vector2Int previousScreenSize = Vector2Int.one;

        Texture2DStack BackgroundStack;
        Texture2DStack WaveformIconStack;
        Texture2DStack AudioPreviewStack;
        
        Vector2 LastMousePosition = Vector2.zero;

        bool PlaybackPreviewVisible = true;
        bool ZoomPreviewVisisble = true;
        bool AudioPreviewVisible = true;
        bool TrimPreviewVisible = true;
        bool EffectsVisible = true;

        int testint = 0;

        IAudioEffect TestTrimEffect = new TrimEffect();
        IAudioEffect TestAmplifyEffect = new AmplifyEffect();

        [MenuItem("WaterKat/AudioEditor", false, 0)]
        static void Init()
        {
            AudioEditor window = (AudioEditor)EditorWindow.GetWindow(typeof(AudioEditor));
            window.Show();
        }

        private void OnEnable()
        {
            BackgroundStack = new Texture2DStack();

            WaveformIconStack = new Texture2DStack();
            WaveformIconStack.AddToStack(WaveformIconStackFunc);
            WaveformIconStack.RecalculateTextureStack();

            AudioPreviewStack = new Texture2DStack();
            AudioPreviewStack.AddToStack(WaveformStackFunc);
            AudioPreviewStack.AddToStack(TimeMarkerStackFunc);
            AudioPreviewStack.AddToStack(CursorLineStackFunc);
            AudioPreviewStack.AddToStack(TrimLineStackFunc);
            AudioPreviewStack.RecalculateTextureStack();

            OnWindowSizeUpdate();
        }

        private void OnGUI()
        {

            if (Event.current.mousePosition != LastMousePosition)
            {
                LastMousePosition = Event.current.mousePosition;
            }


            if (ImagePreviews == null) { CreateStyles(); }

            if (AudioPreviewStack == null) { AudioPreviewStack = new Texture2DStack(); }

            if (previousScreenSize != new Vector2Int(Mathf.FloorToInt(position.width), Mathf.FloorToInt(position.height)))
            {
                previousScreenSize.x = Mathf.FloorToInt(position.width);
                previousScreenSize.y = Mathf.FloorToInt(position.height);
                OnWindowSizeUpdate();
            }

            GUI.Box(new Rect(0, 0, position.width, position.height), BackgroundStack.finalTexture2D);

            GUILayout.BeginHorizontal();
            GUILayout.Label("WaterKat Audio Editor", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            sourceClip = EditorGUILayout.ObjectField("Source Audio", sourceClip, typeof(AudioClip), false) as AudioClip;
            if (sourceClip == null) { return; }

            GUILayout.Space(10);

            if (VisibilityCheck("Audio Playback", ref PlaybackPreviewVisible))
            {
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

                GUILayout.Space(20);
            }



            if (VisibilityCheck("Zoom", ref ZoomPreviewVisisble))
            {
                GUILayout.Box(WaveformIconStack.finalTexture2D, ImagePreviews);

                float lastmin = ZoomSlider.minValue;
                float lastmax = ZoomSlider.maxValue;
                EditorGUILayout.MinMaxSlider(ref ZoomSlider.minValue, ref ZoomSlider.maxValue, ZoomSlider.minLimit, ZoomSlider.maxLimit);
                if ((lastmin != ZoomSlider.minValue) || (lastmax != ZoomSlider.maxValue))
                {
                    AudioPreviewStack.RecalculateTextureStackAtPoint(0);
                }

                GUILayout.Space(20);
            }



            if (VisibilityCheck("Audio Preview", ref AudioPreviewVisible))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(timeFloatToString(ZoomSlider.minValue));
                GUILayout.Label(timeFloatToString(ZoomSlider.maxValue), LabelRightAligned);
                GUILayout.EndHorizontal();

                GUILayout.Space(15);

                GUILayout.Box(AudioPreviewStack.finalTexture2D, ImagePreviews);

                AudioPreviewStack.RecalculateTextureStackAtPoint(2);
                Rect barRect = GUILayoutUtility.GetLastRect();

                int marginOffset = Mathf.FloorToInt((50 / 2) + ImagePreviews.margin.left);
                float relativeMousePosition = relativePosition(LastMousePosition.x, barRect.x, barRect.x + barRect.width);
                float relativeTime = Mathf.Lerp(ZoomSlider.minValue, ZoomSlider.maxValue, relativeMousePosition);
                GUI.Label(new Rect(Mathf.Clamp(LastMousePosition.x, 0 + marginOffset, position.width - marginOffset) - 150, barRect.y - (ImageHeight * 3), 300, 100), timeFloatToString(relativeTime), LabelCenterAligned);
            }


            if (TrimPreviewVisible)
            {
                TrimSlider.minLimit = ZoomSlider.minValue;
                TrimSlider.maxLimit = ZoomSlider.maxValue;
                float lastMin = TrimSlider.minValue;
                float lastMax = TrimSlider.maxValue;
                EditorGUILayout.MinMaxSlider(ref TrimSlider.minValue, ref TrimSlider.maxValue, TrimSlider.minLimit, TrimSlider.maxLimit);
                if ((lastMin != TrimSlider.minValue) || (lastMax != TrimSlider.maxValue))
                {
                    AudioPreviewStack.RecalculateTextureStackAtPoint(4);
                }
            }
            VisibilityCheck("Trim Preview", ref TrimPreviewVisible);
            if (TrimPreviewVisible)
            {
                GUILayout.Space(10);
            }




            if (VisibilityCheck("Effects", ref EffectsVisible))
            {
                ((TrimEffect)TestTrimEffect).updateEffect(TrimSlider);
                ((TrimEffect)TestTrimEffect).DrawEffect();
                ((TrimEffect)TestTrimEffect).updateSlider(TrimSlider);
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical(GUILayout.MaxWidth(10),GUILayout.ExpandWidth(false));
                    {
                        if (GUILayout.Button("▲", ReorderButton))
                        {
                            testint++;
                        }
                        GUILayout.Label(testint.ToString(),LabelCenterAligned);
                        if (GUILayout.Button("▼", ReorderButton))
                        {
                            testint--;
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    {
                        ((AmplifyEffect)TestAmplifyEffect).DrawEffect();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(20);

                ((AmplifyEffect)TestAmplifyEffect).DrawEffect();
            }

        }


        #region OnChangeUpdate
        private void onSourceClipUpdate()
        {
            if (sourceClip == null) { return; }

            PlayBar.maxLimit = (float)sourceClip.samples * (float)sourceClip.channels / (float)sourceClip.frequency;
            PlayBar.minLimit = 0;
            PlayBar.value = 0;
            ZoomSlider.maxLimit = (float)sourceClip.samples * (float)sourceClip.channels / (float)sourceClip.frequency;
            ZoomSlider.minLimit = 0;
            ZoomSlider.ReAlignValues();
            TrimSlider.minLimit = ZoomSlider.minValue;
            TrimSlider.maxLimit = ZoomSlider.maxValue;
            TrimSlider.ReAlignValues();
            AudioPreviewStack.RecalculateTextureStack();
            WaveformIconStack.RecalculateTextureStack();

        }

        private void OnWindowSizeUpdate()
        {
            ImageWidth = (int)position.width - 14;
            BackgroundStack.ResizeRoot(Mathf.FloorToInt(position.width), Mathf.FloorToInt(position.height), new Color32(192, 192, 192, 255));
            AudioPreviewStack.ResizeRoot(ImageWidth, ImageHeight * 5,new Color32(189, 195, 199, 255));
            WaveformIconStack.ResizeRoot(ImageWidth, ImageHeight*2, new Color32(52, 73, 94, 255));
        }
        #endregion
        
        #region Styles
        GUIStyle ImagePreviews;
        GUIStyle LabelRightAligned;
        GUIStyle LabelCenterAligned;
        GUIStyle VisibilityToggle;
        GUIStyle ReorderButton;

        public void CreateStyles()
        {
            ImagePreviews = new GUIStyle() { margin = new RectOffset(7, 7, 0, 0) };
            LabelCenterAligned = new GUIStyle(GUI.skin.label);
            LabelCenterAligned.alignment = TextAnchor.MiddleCenter;
            LabelRightAligned = new GUIStyle(GUI.skin.label);
            LabelRightAligned.alignment = TextAnchor.MiddleRight;
            VisibilityToggle = new GUIStyle(GUI.skin.button);
            VisibilityToggle.alignment = TextAnchor.MiddleRight;
            VisibilityToggle.stretchWidth = false;
            ReorderButton = new GUIStyle(GUI.skin.button);
            ReorderButton.alignment = TextAnchor.MiddleLeft;
            ReorderButton.stretchWidth = false;
        }
        #endregion

        #region StackFunc
        Texture2D WaveformStackFunc(Texture2D _inputTexture)
        {
            Color innerFill = new Color32(52, 152, 219, 255);
            Color outerFill = new Color32(52,73,94, 255);
            int originOffset = _inputTexture.height / 2;

            if (sourceClip == null) { return _inputTexture; }

            float[] audioData = new float[sourceClip.samples * sourceClip.channels];
            sourceClip.GetData(audioData, 0);

            float startIndex = ZoomSlider.minValue / (ZoomSlider.maxLimit - ZoomSlider.minLimit) * audioData.Length;
            float endIndex = ZoomSlider.maxValue / (ZoomSlider.maxLimit - ZoomSlider.minLimit) * audioData.Length;

            for (int x = 0; x < _inputTexture.width; x++)
            {

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

        public Texture2D WaveformIconStackFunc(Texture2D _inputTexture)
        {
            Color PrimaryLine = new Color32(142, 68, 173, 255);
            Color SecondaryLine = new Color32(155, 89, 182, 255);

            int sizeX = _inputTexture.width;
            int sizeY = _inputTexture.height;
            int originOffset = sizeY / 2;

            if (sourceClip == null) { return _inputTexture; }
            
            float[] audioData = new float[sourceClip.samples * sourceClip.channels];
            sourceClip.GetData(audioData, 0);
            NormalizeDataTable(audioData);

            for (int x = 0; x < sizeX; x++)
            {
                int sampleIndex = Mathf.FloorToInt(AudioEditor.relativePosition(x, 0, sizeX) * audioData.Length);
                float scaledSample = audioData[sampleIndex] * originOffset;

                for (int i = 0; Mathf.Abs(i) < Mathf.Abs(scaledSample); i += (int)Mathf.Sign(scaledSample))
                {
                    _inputTexture.SetPixel(x, Mathf.Clamp(i + originOffset, 0, sizeY), SecondaryLine);
                }

                _inputTexture.SetPixel(x, (int)scaledSample + originOffset, PrimaryLine);
            }

            _inputTexture.Apply();
            return _inputTexture;
        }

        Texture2D CursorLineStackFunc(Texture2D _inputTexture)
        {
            if (sourceClip == null) { return _inputTexture; }

            Color lineFill = new Color(0f, 0f, 0f, 1f);


            int sizeX = _inputTexture.width;
            int sizeY = _inputTexture.height;

            int maxMarkerLength = sizeY / 2;


            float firstPosition = 0 + ImagePreviews.margin.left;
            float lastPosition = position.width - ImagePreviews.margin.right;

            int relativeX = Mathf.FloorToInt(AudioEditor.relativePosition(LastMousePosition.x, firstPosition, lastPosition) * sizeX);

            for (int y = 0; y < sizeY; y++)
            {
                _inputTexture.SetPixel(relativeX, y, lineFill);
            }


            _inputTexture.Apply();
            return _inputTexture;
        }

        Texture2D TrimLineStackFunc(Texture2D _inputTexture)
        {
            if (sourceClip == null) { return _inputTexture; }

            Color lineFill = new Color32(39, 174, 96, 255);
            
            int sizeX = _inputTexture.width;
            int sizeY = _inputTexture.height;

            int maxMarkerLength = sizeY / 2;

            int firstLine = Mathf.FloorToInt(relativePosition(TrimSlider.minValue, TrimSlider.minLimit, TrimSlider.maxLimit) *sizeX);
            int secondLine = Mathf.FloorToInt(relativePosition(TrimSlider.maxValue, TrimSlider.minLimit, TrimSlider.maxLimit) * sizeX);

            for (int y = 0; y < sizeY; y++)
            {
                _inputTexture.SetPixel(firstLine, y, lineFill);
            }
            for (int y = 0; y < sizeY; y++)
            {
                _inputTexture.SetPixel(secondLine, y, lineFill);
            }

            _inputTexture.Apply();
            return _inputTexture;
        }

        Texture2D TimeMarkerStackFunc(Texture2D _inputTexture)
        {
            Color BaseLine = new Color32(192, 57, 43, 255);
            Color PrimaryLine = new Color32(192, 57, 43, 255);
            Color SecondaryLine = new Color32(155, 89, 182, 255);

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
                        _inputTexture.SetPixel(relativeX, y, Color.Lerp(PrimaryLine,SecondaryLine,((float)i/times)));
                    }
                }
            }
            for (int x = 0; x < sizeX; x++)
            {
                _inputTexture.SetPixel(x, sizeY - 1, BaseLine);
            }
            for (int y = sizeY - maxMarkerLength; y < sizeY; y++)
            {
                _inputTexture.SetPixel(0, y, PrimaryLine);
            }
            _inputTexture.Apply();
            return _inputTexture;
        }
        #endregion

        #region GUILayouts

        bool SunCloudToggle(bool toggle)
        {
            if (toggle)
            {
                toggle = !GUILayout.Toggle(!toggle, "☀", VisibilityToggle);
            }
            else
            {
                toggle = !GUILayout.Toggle(!toggle, "☁", VisibilityToggle);
            }
            return toggle;
        }

        bool ChangingToggle(bool toggle,string active,string unactive)
        {
            if (toggle)
            {
                toggle = !GUILayout.Toggle(!toggle, active, VisibilityToggle);
            }
            else
            {
                toggle = !GUILayout.Toggle(!toggle, unactive, VisibilityToggle);
            }
            return toggle;
        }

        bool VisibilityCheck(string LabelName,ref bool input)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(LabelName, EditorStyles.boldLabel);
            input = SunCloudToggle(input);
            GUILayout.EndHorizontal();
            return input;
        }


        void DrawTrimEffect(TrimEffect audioEffect)
        {
            audioEffect.visible = EditorGUILayout.Foldout(audioEffect.visible, audioEffect.name);
            EditorGUI.indentLevel++;
            if (audioEffect.visible)
            {
                GUILayout.BeginVertical();


                // GUI.Box(horizontalsize, new GUIContent(new Texture2DStack((int)horizontalsize.x, (int)horizontalsize.y, new Color32(255, 255, 255, 255)).finalTexture2D));


                audioEffect.enabled = GUILayout.Toggle(audioEffect.enabled,"Enabled");

                GUILayout.BeginHorizontal();
                GUILayout.Label("Trim Start: " + timeFloatToString(TrimSlider.minValue));
                TrimSlider.minValue = Mathf.Min(EditorGUILayout.FloatField(TrimSlider.minValue), TrimSlider.maxValue);
                GUILayout.EndHorizontal();
                

                GUILayout.BeginHorizontal();
                GUILayout.Label("Trim End: " + timeFloatToString(TrimSlider.maxValue));
                TrimSlider.maxValue = Mathf.Max(EditorGUILayout.FloatField(TrimSlider.maxValue), TrimSlider.minValue);
                GUILayout.EndHorizontal();

                

                GUILayout.EndVertical();
            }

            EditorGUI.indentLevel--;
        }

        #endregion

        public static string timeFloatToString(float desiredTime)
        {
            return Mathf.FloorToInt(desiredTime / 60).ToString("00") + ":" + ((desiredTime) % 60).ToString("00.00");
        }
        public static float relativePosition(float point, float minLimit, float maxLimit)
        {
            float relativePoint = point - minLimit;
            float relativeSize = maxLimit - minLimit;
            return (relativePoint / relativeSize);
        }
        void NormalizeDataTable(float[] table)
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

    }

    interface IAudioEffect
    {
        bool enabled { get; set; }
        bool visible { get; set; }
        string name { get; set; }
        int order { get; set; }

        float[] ProcessAudioData(float[] inputAudioData);
        void DrawEffect();
    }

    class AmplifyEffect : IAudioEffect
    {
        private bool _enabled = true;
        public bool enabled { get { return _enabled; } set { _enabled = value; } }

        private string _name = "new_AmplifyEffect";
        public string name { get { return _name; } set { _name = value; } }

        private bool _visible = true;
        public bool visible { get { return _visible; } set { _visible = value; } }

        private int _order;

        public int order { get { return _order; } set { _order = value; } }


        public float[] ProcessAudioData(float[] inputAudioData)
        {
            throw new NotImplementedException();
        }

        public void DrawEffect()
        {
            visible = EditorGUILayout.Foldout(visible, name);
            if (visible)
            {
                GUILayout.BeginVertical();

                enabled = GUILayout.Toggle(enabled, "Enabled");

                GUILayout.BeginHorizontal();
                GUILayout.Label("Trim Start: ");
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Trim End: ");
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            //throw new NotImplementedException();
        }
    }

    class TrimEffect : IAudioEffect
    {
        private bool _enabled = true;
        public bool enabled { get { return _enabled; } set { _enabled = value; } }

        private string _name = "new_TrimEffect";
        public string name { get { return _name; } set { _name = value; } }

        private bool _visible = true;
        public bool visible { get { return _visible; } set { _visible = value; } }

        private int _order;

        public int order { get { return _order; } set { _order = value; } }

        public float TrimStart = 0;
        public float TrimEnd = 1;

        public float[] ProcessAudioData(float[] inputAudioData)
        {
            throw new NotImplementedException();
        }

        public void updateEffect(MinMaxSlider minMaxSlider)
        {
            TrimStart = minMaxSlider.minValue;
            TrimEnd = minMaxSlider.maxValue;
        }
        public void updateSlider(MinMaxSlider minMaxSlider)
        {
            minMaxSlider.minValue = TrimStart;
            minMaxSlider.maxValue = TrimEnd;
        }
        public void DrawEffect()
        {
            visible = EditorGUILayout.Foldout(visible, name);
            if (visible)
            {
                GUILayout.BeginVertical();

                enabled = GUILayout.Toggle(enabled, "Enabled");

                GUILayout.BeginHorizontal();
                GUILayout.Label("Trim Start: " + AudioEditor.timeFloatToString(TrimStart));
                TrimStart = Mathf.Min(EditorGUILayout.FloatField(TrimStart), TrimEnd);
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Label("Trim End: " + AudioEditor.timeFloatToString(TrimEnd));
                TrimEnd = Mathf.Max(EditorGUILayout.FloatField(TrimEnd), TrimStart);
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
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
                newTexture.wrapMode = TextureWrapMode.Clamp;
                Graphics.CopyTexture(originalTexture, newTexture);
                AddToTextureStack(i, functionStack[i](newTexture));
            }
        }
        public void RecalculateTextureStackAtPoint(int _index)
        {
            if (functionStack.Count <= _index) { return; }
            if (_index < 1) { RecalculateTextureStack(); return; }
            for (int i = _index; i < functionStack.Count; i++)
            {
                Texture2D originalTexture = textureStack[i - 1];
                Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.ARGB32, false);
                newTexture.wrapMode = TextureWrapMode.Clamp;
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
            root.wrapMode = TextureWrapMode.Clamp;
            RecalculateTextureStack();
        }
        public void ResizeRoot(int sizeX, int sizeY, Color backgroundColor)
        {
            root = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);
            root.wrapMode = TextureWrapMode.Clamp;
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
        public Slider(float _minLimit, float _maxLimit)
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