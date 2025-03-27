#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimpleTextOutline))]
public class SimpleTextOutlineEditor : UnityEditor.Editor
{
    SerializedProperty outlineColor;
    SerializedProperty outlineThickness;
    SerializedProperty outlineOffset;
    
    private void OnEnable()
    {
        outlineColor = serializedObject.FindProperty("outlineColor");
        outlineThickness = serializedObject.FindProperty("outlineThickness");
        outlineOffset = serializedObject.FindProperty("outlineOffset");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUI.BeginChangeCheck();
        
        EditorGUILayout.PropertyField(outlineColor, new GUIContent("Колір аутлайну"));
        EditorGUILayout.PropertyField(outlineThickness, new GUIContent("Товщина аутлайну"));
        EditorGUILayout.PropertyField(outlineOffset, new GUIContent("Зсув аутлайну"));
        
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            ((SimpleTextOutline)target).UpdateOutline();
        }
        
        if (GUILayout.Button("Оновити аутлайн"))
        {
            ((SimpleTextOutline)target).UpdateOutline();
        }
    }
}
#endif