#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Character;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Character
{
    public static class CharacterAnimationProfileReportWriter
    {
        [Serializable] private sealed class Report
        {
            public int schemaVersion;
            public string generatorVersion;
            public string characterId;
            public string sourceJsonSha256;
            public string outputAssetSha256;
            public string assetGuid;
            public long fileId;
            public string classification;
            public CharacterAnimationProfileValidator.Issue[] issues;
        }

        public static void Write(string sourcePath, string assetPath, CharacterAnimationProfileSO profile)
        {
            CharacterSO character = AssetDatabase.FindAssets($"t:CharacterSO {profile.CharacterId}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<CharacterSO>(path))
                .FirstOrDefault(item => item != null && item.CharacterId == profile.CharacterId);
            CharacterAnimationProfileValidator.Issue[] issues = character != null
                ? CharacterAnimationProfileValidator.Validate(character).ToArray()
                : Array.Empty<CharacterAnimationProfileValidator.Issue>();
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(profile, out string guid, out long fileId);
            Report report = new()
            {
                schemaVersion = CharacterAnimationProfileSO.CurrentSchemaVersion,
                generatorVersion = CharacterAnimationProfileAssetBuilder.GeneratorVersion,
                characterId = profile.CharacterId,
                sourceJsonSha256 = Sha256(sourcePath),
                outputAssetSha256 = Sha256(assetPath),
                assetGuid = guid,
                fileId = fileId,
                classification = issues.Any(x => x.severity == "BLOCKER") ? "BLOCKER" :
                    issues.Any(x => x.severity == "ERROR") ? "ERROR" :
                    issues.Any(x => x.severity == "WARN") ? "WARN" : "PASS",
                issues = issues
            };
            string reportPath = assetPath + ".validation.json";
            File.WriteAllText(reportPath, JsonUtility.ToJson(report, true) + "\n");
            AssetDatabase.ImportAsset(reportPath, ImportAssetOptions.ForceSynchronousImport);
        }

        private static string Sha256(string path)
        {
            using SHA256 hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
#endif
