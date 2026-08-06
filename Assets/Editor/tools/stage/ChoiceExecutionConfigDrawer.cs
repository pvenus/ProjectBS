using System.Collections.Generic;
using Stage;
using UnityEditor;
using UnityEngine;

namespace StageEditor
{
    [CustomPropertyDrawer(typeof(ChoiceExecutionConfig), true)]
    public sealed class ChoiceExecutionConfigDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.managedReferenceValue == null)
            {
                DrawInitializer(position, property, label);
                EditorGUI.EndProperty();
                return;
            }

            SerializedProperty typeProperty =
                property.FindPropertyRelative("executionType");
            SerializedProperty dataProperty =
                property.FindPropertyRelative("data");

            float y = position.y;
            Rect typeRect = new(
                position.x,
                y,
                position.width,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(typeRect, typeProperty, label);

            if (EditorGUI.EndChangeCheck())
            {
                ChoiceExecutionType executionType =
                    (ChoiceExecutionType)typeProperty.intValue;

                dataProperty.managedReferenceValue =
                    ChoiceExecutionDataFactory.Create(executionType);
                property.serializedObject.ApplyModifiedProperties();
            }

            y += EditorGUIUtility.singleLineHeight + Spacing;

            if (dataProperty.managedReferenceValue != null)
            {
                float dataHeight =
                    EditorGUI.GetPropertyHeight(
                        dataProperty,
                        true);
                Rect dataRect = new(
                    position.x,
                    y,
                    position.width,
                    dataHeight);

                EditorGUI.PropertyField(
                    dataRect,
                    dataProperty,
                    new GUIContent("Data"),
                    true);
                y += dataHeight + Spacing;
            }

            DrawValidationIssues(property, position, ref y);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            if (property.managedReferenceValue == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float height = EditorGUIUtility.singleLineHeight;
            SerializedProperty dataProperty =
                property.FindPropertyRelative("data");

            if (dataProperty?.managedReferenceValue != null)
            {
                height += Spacing
                          + EditorGUI.GetPropertyHeight(
                              dataProperty,
                              true);
            }

            ChoiceExecutionConfig config =
                property.managedReferenceValue
                    as ChoiceExecutionConfig;
            List<string> errors =
                ChoiceExecutionConfigValidator.Validate(config);

            foreach (string error in errors)
            {
                height += Spacing + GetHelpBoxHeight(error);
            }

            return height;
        }

        private static void DrawInitializer(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            Rect labelRect = new(
                position.x,
                position.y,
                EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight);
            Rect buttonRect = new(
                position.x + EditorGUIUtility.labelWidth,
                position.y,
                position.width - EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(labelRect, label);

            if (!GUI.Button(buttonRect, "Create Execution Config"))
            {
                return;
            }

            property.managedReferenceValue =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.CompleteEvent);
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void DrawValidationIssues(
            SerializedProperty property,
            Rect position,
            ref float y)
        {
            ChoiceExecutionConfig config =
                property.managedReferenceValue
                    as ChoiceExecutionConfig;
            List<string> errors =
                ChoiceExecutionConfigValidator.Validate(config);

            foreach (string error in errors)
            {
                float height = GetHelpBoxHeight(error);
                Rect rect = new(
                    position.x,
                    y,
                    position.width,
                    height);

                EditorGUI.HelpBox(rect, error, MessageType.Error);
                y += height + Spacing;
            }
        }

        private static float GetHelpBoxHeight(string message)
        {
            return Mathf.Max(
                EditorGUIUtility.singleLineHeight * 2f,
                EditorStyles.helpBox.CalcHeight(
                    new GUIContent(message),
                    EditorGUIUtility.currentViewWidth));
        }
    }
}
