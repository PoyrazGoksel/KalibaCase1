using DG.DemiEditor;
using UnityEditor;
using UnityEngine;

namespace Extensions.Unity.Editor
{
    [CustomPropertyDrawer(typeof(TransformEncapsulated))]
    [CanEditMultipleObjects]
    public class TransformEncapsulatedEditor : PropertyDrawer
    {
    
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            SerializedProperty transProp = property.FindPropertyRelative("_transform");

            Object newVal = transProp.objectReferenceValue;
        
            newVal = EditorGUI.ObjectField
            (
                rect,
                label,
                newVal,
                typeof(Transform),
                true
            );
            transProp.SetValue(newVal);
            EditorGUI.EndProperty();
        }
    }
}