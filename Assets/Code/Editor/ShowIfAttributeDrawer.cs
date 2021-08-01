using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfAttributeDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement();

        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        var b = property.serializedObject.FindProperty(showIf.propertyName);
        if (b.propertyType != SerializedPropertyType.Boolean)
        {
            if (b.boolValue)
            {
                var propertyToShow = new PropertyField(property);
                container.Add(propertyToShow);
            }
        }

        return container;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        var b = property.serializedObject.FindProperty(showIf.propertyName);
        if (b.propertyType != SerializedPropertyType.Boolean)
        {
            return 0;
        }
        if (b.boolValue)
        {
            return base.GetPropertyHeight(property, label);
        }
        return 0;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        var b = property.serializedObject.FindProperty(showIf.propertyName);
        if (b.propertyType != SerializedPropertyType.Boolean)
        {
            EditorGUILayout.HelpBox("Property '" + showIf.propertyName + "' must be of type 'bool'", MessageType.Error);
            return;
        }
        if (b.boolValue)
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}