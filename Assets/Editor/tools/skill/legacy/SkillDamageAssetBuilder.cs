

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    [Serializable]
    public class DamageJson
    {
        public string damageId;
        public string skillId;
        public string damageType;

        public float baseDamage;
        public float attackPercentDamage;

        public bool canCritical;
        public bool ignoreDefense;
    }

    /// <summary>
    /// SkillDamageSO 전용 에셋 빌더.
    /// JSON의 damage 데이터를 SkillDamageSO 에셋으로 생성/갱신한다.
    /// </summary>
    public static class SkillDamageAssetBuilder
    {
        public static ScriptableObject CreateOrUpdate(
            DamageJson json,
            string outputFolder)
        {
            if (json == null)
            {
                Debug.LogWarning("[SkillDamageAssetBuilder] Damage json is null.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                Debug.LogError("[SkillDamageAssetBuilder] Output folder is null or empty.");
                return null;
            }

            EnsureFolder(outputFolder);

            Type damageType = FindType("SkillDamageSO");

            if (damageType == null)
            {
                Debug.LogError("[SkillDamageAssetBuilder] SkillDamageSO type not found.");
                return null;
            }

            string assetName = ResolveAssetName(json);
            string assetPath = Path.Combine(outputFolder, assetName + ".asset")
                .Replace("\\", "/");

            ScriptableObject damageSo =
                AssetDatabase.LoadAssetAtPath(assetPath, damageType) as ScriptableObject;

            if (damageSo == null)
            {
                damageSo = ScriptableObject.CreateInstance(damageType);
                AssetDatabase.CreateAsset(damageSo, assetPath);
            }

            Apply(damageSo, json);

            EditorUtility.SetDirty(damageSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SkillDamageAssetBuilder] Updated SkillDamageSO: {assetPath}");

            return damageSo;
        }

        private static string ResolveAssetName(DamageJson json)
        {
            if (!string.IsNullOrWhiteSpace(json.damageId))
            {
                return SanitizeFileName(json.damageId);
            }

            if (!string.IsNullOrWhiteSpace(json.skillId))
            {
                return SanitizeFileName(json.skillId + ".damage");
            }

            return "skill.damage";
        }

        private static void Apply(
            ScriptableObject damageSo,
            DamageJson json)
        {
            EditorFieldSetter.SetFirstExistingField(
                damageSo,
                json.damageId,
                "damageId",
                "skillDamageId",
                "id");

            EditorFieldSetter.SetFirstExistingField(
                damageSo,
                json.skillId,
                "skillId",
                "sourceSkillId");

            EditorFieldSetter.SetFirstExistingField(
                damageSo,
                json.damageType,
                "damageType",
                "type");

            EditorFieldSetter.SetFirstExistingField(
                damageSo,
                json.baseDamage,
                "baseDamage",
                "damage");

            EditorFieldSetter.SetFirstExistingField(
                damageSo,
                json.attackPercentDamage,
                "attackPercentDamage",
                "attackDamageRatio",
                "attackRatio");

            EditorFieldSetter.SetFirstExistingField(
                damageSo,
                json.canCritical,
                "canCritical",
                "useCritical",
                "allowCritical");

            EditorFieldSetter.SetFirstExistingField(
                damageSo,
                json.ignoreDefense,
                "ignoreDefense",
                "isIgnoreDefense");
        }

        private static Type FindType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);

                if (type != null)
                {
                    return type;
                }

                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].Name == typeName)
                    {
                        return types[i];
                    }
                }
            }

            return null;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return;
            }

            folderPath = folderPath.Replace("\\", "/");

            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');

            if (parts.Length == 0 || parts[0] != "Assets")
            {
                Debug.LogError($"[SkillDamageAssetBuilder] Folder path must start with Assets: {folderPath}");
                return;
            }

            string currentPath = "Assets";

            for (int i = 1; i < parts.Length; i++)
            {
                string nextPath = currentPath + "/" + parts[i];

                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }

                currentPath = nextPath;
            }
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "skill.damage";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}