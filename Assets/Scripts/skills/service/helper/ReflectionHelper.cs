

using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Resolver 계층에서만 사용하는 reflection 유틸리티.
/// 과도기적으로 CreateDto 시그니처가 통일되지 않았거나,
/// private field / public property를 안전하게 읽고 써야 할 때 사용한다.
/// </summary>
public static class ReflectionHelper
{
    private const BindingFlags InstanceFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public static object ReadMemberObject(object source, string memberName)
    {
        if (source == null || string.IsNullOrWhiteSpace(memberName))
        {
            return null;
        }

        Type type = source.GetType();

        PropertyInfo property = type.GetProperty(memberName, InstanceFlags);
        if (property != null && property.CanRead)
        {
            return property.GetValue(source);
        }

        FieldInfo field = type.GetField(memberName, InstanceFlags);
        if (field != null)
        {
            return field.GetValue(source);
        }

        return null;
    }

    public static T ReadValue<T>(object source, string memberName, T fallback = default)
    {
        object value = ReadMemberObject(source, memberName);
        if (value is T typed)
        {
            return typed;
        }

        return fallback;
    }

    public static bool TryReadValue<T>(object source, string memberName, out T value)
    {
        object rawValue = ReadMemberObject(source, memberName);
        if (rawValue is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public static void WriteMemberObject(object target, string memberName, object value)
    {
        if (target == null || string.IsNullOrWhiteSpace(memberName))
        {
            return;
        }

        Type type = target.GetType();

        PropertyInfo property = type.GetProperty(memberName, InstanceFlags);
        if (property != null && property.CanWrite)
        {
            property.SetValue(target, value);
            return;
        }

        FieldInfo field = type.GetField(memberName, InstanceFlags);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }

    public static bool TryWriteMemberObject(object target, string memberName, object value)
    {
        if (target == null || string.IsNullOrWhiteSpace(memberName))
        {
            return false;
        }

        Type type = target.GetType();

        PropertyInfo property = type.GetProperty(memberName, InstanceFlags);
        if (property != null && property.CanWrite)
        {
            property.SetValue(target, value);
            return true;
        }

        FieldInfo field = type.GetField(memberName, InstanceFlags);
        if (field != null)
        {
            field.SetValue(target, value);
            return true;
        }

        return false;
    }

    public static T TryInvokeCreateDto<T>(object source, params object[] args) where T : class
    {
        if (source == null)
        {
            return null;
        }

        MethodInfo[] methods = source.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
        foreach (MethodInfo method in methods)
        {
            if (method.Name != "CreateDto")
            {
                continue;
            }

            if (!typeof(T).IsAssignableFrom(method.ReturnType))
            {
                continue;
            }

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != (args?.Length ?? 0))
            {
                continue;
            }

            try
            {
                object result = method.Invoke(source, args);
                return result as T;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"ReflectionHelper: CreateDto invoke failed on {source.GetType().Name}.{method.Name} - {ex.Message}");
                return null;
            }
        }

        return null;
    }
}