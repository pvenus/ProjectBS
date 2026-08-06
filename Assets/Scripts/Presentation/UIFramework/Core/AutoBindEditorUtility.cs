#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class AutoBindEditorUtility
{
    public static void Bind(MonoBehaviour target)
    {
        if (target == null)
        {
            return;
        }

        string prefix = GetPrefix(target);

        SerializedObject serializedObject =
            new SerializedObject(target);

        FieldInfo[] fields =
            target.GetType().GetFields(
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);

        foreach (FieldInfo field in fields)
        {
            AutoBindAttribute attribute =
                field.GetCustomAttribute<AutoBindAttribute>();

            if (attribute == null)
            {
                continue;
            }

            string targetName =
                BuildTargetName(
                    prefix,
                    field,
                    attribute);

            Component component =
                FindComponent(
                    target.transform,
                    targetName,
                    field.FieldType);

            if (component == null)
            {
                continue;
            }

            SerializedProperty property =
                serializedObject.FindProperty(field.Name);

            if (property == null)
            {
                continue;
            }

            property.objectReferenceValue = component;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(target);

        PrefabUtility.RecordPrefabInstancePropertyModifications(target);
    }

    private static string GetPrefix(MonoBehaviour target)
    {
        AutoBindPrefixAttribute attribute =
            target.GetType()
                .GetCustomAttribute<AutoBindPrefixAttribute>();

        if (attribute == null)
        {
            return string.Empty;
        }

        return attribute.prefix;
    }

    private static string BuildTargetName(
        string prefix,
        FieldInfo field,
        AutoBindAttribute attribute)
    {
        string objectName;

        if (!string.IsNullOrWhiteSpace(attribute.customName))
        {
            objectName = attribute.customName;
        }
        else
        {
            objectName = field.Name;

            if (objectName.StartsWith("_"))
            {
                objectName = objectName.Substring(1);
            }

            objectName =
                char.ToUpper(objectName[0]) +
                objectName.Substring(1);
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            return objectName;
        }

        return $"{prefix}_{objectName}";
    }

    private static Component FindComponent(
        Transform root,
        string objectName,
        Type componentType)
    {
        Transform[] children =
            root.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name != objectName)
            {
                continue;
            }

            Component component =
                child.GetComponent(componentType);

            if (component != null)
            {
                return component;
            }
        }

        return null;
    }
}

#endif
