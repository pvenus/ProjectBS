#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Character
{
    /// <summary>Read-only P3 inventory. DryRun methods never dirty, create, save, or import assets.</summary>
    public static class CharacterAnimationMigrationDryRun
    {
        [Serializable] public sealed class CharacterReport
        {
            public string characterId;
            public string assetPath;
            public string assetGuid;
            public string classification;
            public string proposedProfilePath;
            public string rollback = "restore nullable profile ref; legacy clip/skill refs stay untouched";
            public CharacterAnimationProfileValidator.Issue[] issues;
        }

        [Serializable] public sealed class InventoryReport
        {
            public int schemaVersion = CharacterAnimationProfileSO.CurrentSchemaVersion;
            public string generatorVersion = CharacterAnimationProfileAssetBuilder.GeneratorVersion;
            public string mode = "DRY_RUN_NO_ASSET_MUTATION";
            public CharacterReport[] characters;
        }

        public static string DryRunAllPlayers()
        {
            CharacterReport[] reports = AssetDatabase.FindAssets("t:CharacterSO")
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => Build(path))
                .Where(report => report != null)
                .ToArray();
            return JsonUtility.ToJson(new InventoryReport { characters = reports }, true);
        }

        public static string DryRunOne(string characterAssetPath)
        {
            CharacterReport report = Build(characterAssetPath);
            return JsonUtility.ToJson(new InventoryReport
            {
                characters = report == null ? Array.Empty<CharacterReport>() : new[] { report }
            }, true);
        }

        private static CharacterReport Build(string path)
        {
            CharacterSO character = AssetDatabase.LoadAssetAtPath<CharacterSO>(path);
            if (character == null || character.CharacterType != CharacterType.Player) return null;
            CharacterAnimationProfileValidator.Issue[] issues =
                CharacterAnimationProfileValidator.Validate(character).ToArray();
            return new CharacterReport
            {
                characterId = character.CharacterId,
                assetPath = path,
                assetGuid = AssetDatabase.AssetPathToGUID(path),
                proposedProfilePath = path.Replace(".asset", ".animation-profile.asset"),
                classification = issues.Any(x => x.severity == "BLOCKER") ? "BLOCKER" :
                    issues.Any(x => x.severity == "ERROR") ? "ERROR" :
                    issues.Any(x => x.severity == "WARN") ? "WARN" : "PASS",
                issues = issues
            };
        }
    }
}
#endif
