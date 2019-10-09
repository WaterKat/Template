using UnityEngine;
using UnityEditor;
using WaterKat.MathW;

namespace WaterKat.WKPlayer2D
{
    [CustomEditor(typeof(Jump2D))]
    public class Jump2DEditor : Editor
    {
        SerializedProperty calculationMode;

        SerializedProperty Gravity;
        SerializedProperty JumpHeight;
        SerializedProperty JumpTime;
        SerializedProperty JumpVelocity;


        void OnEnable()
        {
            calculationMode = serializedObject.FindProperty("calculationMode");

            Gravity = serializedObject.FindProperty("Gravity");
            JumpHeight = serializedObject.FindProperty("JumpHeight");
            JumpTime = serializedObject.FindProperty("JumpTime");
            JumpVelocity = serializedObject.FindProperty("JumpVelocity");


        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(calculationMode);

            EditorGUILayout.Space();

            EditorGUI.indentLevel += 1;
            switch (calculationMode.enumValueIndex)
            {
                case 0:
                    EditorGUILayout.PropertyField(JumpHeight);
                    EditorGUILayout.PropertyField(JumpTime);
                    break;
                case 1:
                    EditorGUILayout.PropertyField(Gravity);
                    EditorGUILayout.PropertyField(JumpHeight);
                    break;
                case 2:
                    EditorGUILayout.PropertyField(Gravity);
                    EditorGUILayout.PropertyField(JumpTime);
                    break;
            }
            EditorGUI.indentLevel -= 1;

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.PropertyField(Gravity);
            EditorGUILayout.PropertyField(JumpHeight);
            EditorGUILayout.PropertyField(JumpTime);
            EditorGUILayout.PropertyField(JumpVelocity);


            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}