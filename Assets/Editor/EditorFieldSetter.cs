using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Editor 전용 필드 세팅 유틸.
/// ScriptableObject/일반 객체의 실제 필드명이 조금 달라도 후보 필드명 중 첫 번째로 존재하는 필드를 찾아 값을 넣는다.
/// </summary>
public static class EditorFieldSetter
{
    private const BindingFlags InstanceFieldFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    public static bool SetFirstExistingField(
        object target,
        object value,
        params string[] fieldNames)
    {
        if (target == null || fieldNames == null || fieldNames.Length == 0)
        {
            return false;
        }

        Type targetType = target.GetType();

        for (int i = 0; i < fieldNames.Length; i++)
        {
            string fieldName = fieldNames[i];

            if (string.IsNullOrWhiteSpace(fieldName))
            {
                continue;
            }

            FieldInfo field = FindField(targetType, fieldName);

            if (field == null)
            {
                continue;
            }

            object convertedValue = ConvertValue(value, field.FieldType);

            try
            {
                field.SetValue(target, convertedValue);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[EditorFieldSetter] Failed to set field. " +
                    $"Target={targetType.Name} Field={fieldName} " +
                    $"Value={value} FieldType={field.FieldType.Name} " +
                    $"Error={exception.Message}");
                return false;
            }
        }

        return false;
    }

    private static FieldInfo FindField(
        Type type,
        string fieldName)
    {
        while (type != null)
        {
            FieldInfo field = type.GetField(
                fieldName,
                InstanceFieldFlags);

            if (field != null)
            {
                return field;
            }

            type = type.BaseType;
        }

        return null;
    }

    private static object ConvertValue(
        object value,
        Type targetType)
    {
        if (targetType == null)
        {
            return value;
        }

        if (value == null)
        {
            return targetType.IsValueType
                ? Activator.CreateInstance(targetType)
                : null;
        }

        Type valueType = value.GetType();

        if (targetType.IsAssignableFrom(valueType))
        {
            return value;
        }

        if (targetType.IsEnum)
        {
            if (value is string stringValue)
            {
                if (Enum.TryParse(targetType, stringValue, true, out object enumValue))
                {
                    return enumValue;
                }

                return Activator.CreateInstance(targetType);
            }

            try
            {
                return Enum.ToObject(targetType, value);
            }
            catch
            {
                return Activator.CreateInstance(targetType);
            }
        }

        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return value;
        }
    }
}
