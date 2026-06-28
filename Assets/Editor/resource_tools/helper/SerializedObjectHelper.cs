

using System;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    public static class SerializedObjectHelper
    {
        public static SerializedProperty FindRequiredProperty(
            SerializedObject serializedObject,
            string propertyName)
        {
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                return property;
            }

            throw new InvalidOperationException(
                $"[SerializedObjectHelper] Serialized property not found: {propertyName}");
        }

        public static void SetString(
            SerializedObject serializedObject,
            string propertyName,
            string value)
        {
            SerializedProperty property =
                FindRequiredProperty(serializedObject, propertyName);

            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = value;
                return;
            }

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                SetEnum(property, value, propertyName);
                return;
            }

            throw new InvalidOperationException(
                $"[SerializedObjectHelper] Property is not string or enum: {propertyName} type={property.propertyType}");
        }

        public static void SetBool(
            SerializedObject serializedObject,
            string propertyName,
            bool value)
        {
            SerializedProperty property =
                FindRequiredProperty(serializedObject, propertyName);

            property.boolValue = value;
        }

        public static void SetInt(
            SerializedObject serializedObject,
            string propertyName,
            int value)
        {
            SerializedProperty property =
                FindRequiredProperty(serializedObject, propertyName);

            property.intValue = value;
        }

        public static void SetFloat(
            SerializedObject serializedObject,
            string propertyName,
            float value)
        {
            SerializedProperty property =
                FindRequiredProperty(serializedObject, propertyName);

            property.floatValue = value;
        }

        public static void SetLayerMask(
            SerializedObject serializedObject,
            string propertyName,
            LayerMask value)
        {
            SerializedProperty property =
                FindRequiredProperty(serializedObject, propertyName);

            property.intValue = value.value;
        }

        public static void SetEnum(
            SerializedProperty property,
            string value,
            string propertyName)
        {
            string[] enumNames = property.enumNames;

            for (int i = 0; i < enumNames.Length; i++)
            {
                if (string.Equals(
                        enumNames[i],
                        value,
                        StringComparison.OrdinalIgnoreCase))
                {
                    property.enumValueIndex = i;
                    return;
                }
            }

            Debug.LogError(
                $"[SerializedObjectHelper] Enum value not found. property={propertyName} value={value}");
        }
    }
}