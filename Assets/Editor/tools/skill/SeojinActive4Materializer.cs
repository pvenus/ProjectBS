#if UNITY_EDITOR
using System;
using Character;
using ResourceTools.Skill;
using Skill;
using UnityEditor;
using UnityEngine;

public static class SeojinActive4Materializer
{
    private const string MenuPath = "Tools/ProjectBS/Active4/Materialize Seojin Exact3";

    [MenuItem(MenuPath)]
    public static void Materialize()
    {
        for (int grade = 1; grade <= 3; grade++)
        {
            string skillId = $"skill.character.seojin.{grade}.active_4.swift_step";
            string jsonPath = $"Assets/Contents/Skill/json/{skillId}.json";
            EquipmentSkillSO skill = EquipmentSkillJsonGenerator.GenerateFromJsonPath(jsonPath);
            if (skill == null || skill.CastSo == null || skill.BaseVisualSo == null ||
                skill.CastSo.MobilityVfxClip == null || skill.Icon == null)
            {
                throw new InvalidOperationException(
                    $"Active4 materialization incomplete: grade={grade}, id={skillId}");
            }

            string characterPath = $"Assets/Contents/Character/so/character_seojin_{grade}.asset";
            CharacterSO character = AssetDatabase.LoadAssetAtPath<CharacterSO>(characterPath);
            if (character == null)
            {
                throw new InvalidOperationException($"CharacterSO not found: {characterPath}");
            }

            SerializedObject serialized = new SerializedObject(character);
            SerializedProperty skills = serialized.FindProperty("skills");
            SerializedProperty active4 = null;
            for (int i = 0; i < skills.arraySize; i++)
            {
                SerializedProperty candidate = skills.GetArrayElementAtIndex(i);
                if (string.Equals(
                    candidate.FindPropertyRelative("slotKey").stringValue,
                    SkillPoolSlotKeys.Active4,
                    StringComparison.Ordinal))
                {
                    active4 = candidate;
                    break;
                }
            }

            if (active4 == null)
            {
                int index = skills.arraySize;
                skills.InsertArrayElementAtIndex(index);
                active4 = skills.GetArrayElementAtIndex(index);
            }

            active4.FindPropertyRelative("slotKey").stringValue = SkillPoolSlotKeys.Active4;
            active4.FindPropertyRelative("skillSo").objectReferenceValue = skill;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(character);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SeojinActive4Materializer] COMPLETE exact3 materialization PASS.");
    }
}
#endif
