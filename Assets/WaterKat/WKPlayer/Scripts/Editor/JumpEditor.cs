using UnityEngine;
using UnityEditor;

namespace WaterKat.WKPlayer
{
    [CustomEditor(typeof(Jump))]
    public class JumpEditor : Editor
    {
        SerializedProperty calculationMode;

        SerializedProperty Gravity;
        SerializedProperty JumpHeight;
        SerializedProperty JumpTime;

        void OnEnable()
        {
            calculationMode = serializedObject.FindProperty("calculationMode");

            Gravity = serializedObject.FindProperty("Gravity");
            JumpHeight = serializedObject.FindProperty("JumpHeight");
            JumpTime = serializedObject.FindProperty("JumpTime");



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
            
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}