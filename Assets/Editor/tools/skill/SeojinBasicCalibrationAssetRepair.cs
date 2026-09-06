using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Skill;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    internal static class SeojinBasicCalibrationAssetRepair
    {
        private const string SoRoot = "Assets/Contents/Skill/so";
        private static readonly Regex EntryPattern = new Regex(
            @"-\s*frameIndex:\s*(?<i>\d+)\s+scale:\s*\{x:\s*(?<sx>[-+0-9.eE]+),\s*y:\s*(?<sy>[-+0-9.eE]+)\}\s+offset:\s*\{x:\s*(?<ox>[-+0-9.eE]+),\s*y:\s*(?<oy>[-+0-9.eE]+)\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly string[] ProfileIds =
        {
            "skill.character.seojin.basic_attack.combo.body.hit0.calibration",
            "skill.character.seojin.basic_attack.combo.body.hit1.calibration",
            "skill.character.seojin.basic_attack.combo.body.hit2.calibration",
            "skill.character.seojin.1.basic_attack.combo.vfx.hit1.calibration",
            "skill.character.seojin.1.basic_attack.combo.vfx.hit2.calibration",
            "skill.character.seojin.2.basic_attack.combo.vfx.hit1.calibration",
            "skill.character.seojin.2.basic_attack.combo.vfx.hit2.calibration",
            "skill.character.seojin.3.basic_attack.combo.vfx.hit1.calibration",
            "skill.character.seojin.3.basic_attack.combo.vfx.hit2.calibration"
        };

        [MenuItem("Tools/Skill/Repair Seojin Basic Calibration Exact9")]
        private static void RepairExact9()
        {
            string backupRoot = Path.Combine(
                "/private/tmp",
                "projectbs-seojin-basic-calibration-exact9-rollback-" +
                DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture));
            string tempRoot = $"Assets/__SeojinBasicCalibrationRepair_{Guid.NewGuid():N}";
            Directory.CreateDirectory(backupRoot);
            Directory.CreateDirectory(tempRoot);

            var originals = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            var originalGuids = new Dictionary<string, string>(StringComparer.Ordinal);
            var tempPaths = new Dictionary<string, string>(StringComparer.Ordinal);
            try
            {
                // Read only the locked numeric payload from the corrupt legacy bytes. The replacement
                // schema is emitted exclusively by Unity's serializer through CreateAsset.
                for (int p = 0; p < ProfileIds.Length; p++)
                {
                    string id = ProfileIds[p];
                    string canonicalPath = CanonicalPath(id);
                    if (!File.Exists(canonicalPath) || !File.Exists(canonicalPath + ".meta"))
                        throw new InvalidOperationException($"Calibration asset/meta missing: {canonicalPath}");
                    originals.Add(canonicalPath, File.ReadAllBytes(canonicalPath));
                    File.WriteAllBytes(Path.Combine(backupRoot, id + ".asset"), originals[canonicalPath]);
                    File.Copy(canonicalPath + ".meta", Path.Combine(backupRoot, id + ".asset.meta"), true);
                    string guid = AssetDatabase.AssetPathToGUID(canonicalPath);
                    if (string.IsNullOrWhiteSpace(guid) || originalGuids.ContainsValue(guid))
                        throw new InvalidOperationException($"Missing or duplicate canonical GUID: {canonicalPath}");
                    originalGuids.Add(canonicalPath, guid);

                    CalibrationValue[] values = ParseLockedValues(originals[canonicalPath], canonicalPath);
                    string tempPath = $"{tempRoot}/{id}.asset";
                    SpritePresentationCalibrationProfileSO instance =
                        ScriptableObject.CreateInstance<SpritePresentationCalibrationProfileSO>();
                    instance.name = id;
                    SerializedObject serialized = new SerializedObject(instance);
                    serialized.FindProperty("profileId").stringValue = id;
                    SerializedProperty entries = serialized.FindProperty("entries");
                    entries.arraySize = 6;
                    for (int i = 0; i < values.Length; i++)
                    {
                        SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                        entry.FindPropertyRelative("frameIndex").intValue = i;
                        entry.FindPropertyRelative("scale").vector2Value = values[i].Scale;
                        entry.FindPropertyRelative("offset").vector2Value = values[i].Offset;
                    }
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    AssetDatabase.CreateAsset(instance, tempPath);
                    tempPaths.Add(canonicalPath, tempPath);
                }

                AssetDatabase.SaveAssets();
                for (int p = 0; p < ProfileIds.Length; p++)
                {
                    string canonicalPath = CanonicalPath(ProfileIds[p]);
                    string tempPath = tempPaths[canonicalPath];
                    AssetDatabase.ImportAsset(tempPath, ImportAssetOptions.ForceSynchronousImport);
                    SpritePresentationCalibrationProfileSO fixture =
                        AssetDatabase.LoadAssetAtPath<SpritePresentationCalibrationProfileSO>(tempPath);
                    ValidateProfile(fixture, ProfileIds[p], tempPath);
                }

                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (KeyValuePair<string, string> pair in tempPaths)
                        File.Copy(pair.Value, pair.Key, true);
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }

                for (int p = 0; p < ProfileIds.Length; p++)
                {
                    string id = ProfileIds[p];
                    string canonicalPath = CanonicalPath(id);
                    AssetDatabase.ImportAsset(
                        canonicalPath,
                        ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                    if (!string.Equals(AssetDatabase.AssetPathToGUID(canonicalPath), originalGuids[canonicalPath], StringComparison.Ordinal))
                        throw new InvalidOperationException($"GUID changed during repair: {canonicalPath}");
                    ValidateProfile(
                        AssetDatabase.LoadAssetAtPath<SpritePresentationCalibrationProfileSO>(canonicalPath),
                        id,
                        canonicalPath);
                }

                RebindExact3PersistedObjects();
                AssetDatabase.SaveAssets();
                Debug.Log($"[SeojinBasicCalibrationAssetRepair] PASS exact9; rollback={backupRoot}");
            }
            catch (Exception exception)
            {
                foreach (KeyValuePair<string, byte[]> pair in originals)
                    File.WriteAllBytes(pair.Key, pair.Value);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                Debug.LogException(exception);
                throw;
            }
            finally
            {
                AssetDatabase.DeleteAsset(tempRoot);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
        }

        private static void RebindExact3PersistedObjects()
        {
            for (int grade = 1; grade <= 3; grade++)
            {
                string skillPath = $"{SoRoot}/skill.character.seojin.{grade}.basic_attack.basic_attack.asset";
                EquipmentSkillSO skill = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(skillPath);
                if (skill == null) throw new InvalidOperationException($"Missing EquipmentSkillSO: {skillPath}");
                SerializedObject serialized = new SerializedObject(skill);
                SerializedProperty steps = serialized.FindProperty("comboProfile.steps");
                if (steps == null || steps.arraySize != 3)
                    throw new InvalidOperationException($"Expected comboProfile.steps[3]: {skillPath}");
                for (int step = 0; step < 3; step++)
                {
                    SerializedProperty item = steps.GetArrayElementAtIndex(step);
                    item.FindPropertyRelative("bodyPresentationCalibration").objectReferenceValue =
                        LoadPersisted($"skill.character.seojin.basic_attack.combo.body.hit{step}.calibration");
                    item.FindPropertyRelative("vfxPresentationCalibration").objectReferenceValue = step == 0
                        ? null
                        : LoadPersisted($"skill.character.seojin.{grade}.basic_attack.combo.vfx.hit{step}.calibration");
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(skill);
            }
        }

        private static SpritePresentationCalibrationProfileSO LoadPersisted(string id)
        {
            string path = CanonicalPath(id);
            SpritePresentationCalibrationProfileSO profile =
                AssetDatabase.LoadAssetAtPath<SpritePresentationCalibrationProfileSO>(path);
            ValidateProfile(profile, id, path);
            return profile;
        }

        private static CalibrationValue[] ParseLockedValues(byte[] bytes, string path)
        {
            string text = System.Text.Encoding.UTF8.GetString(bytes);
            MatchCollection matches = EntryPattern.Matches(text);
            if (matches.Count != 6) throw new InvalidOperationException($"Expected 6 locked values: {path}");
            var result = new CalibrationValue[6];
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                int frame = int.Parse(match.Groups["i"].Value, CultureInfo.InvariantCulture);
                if (frame != i) throw new InvalidOperationException($"Unordered frame {frame} at {path}");
                result[i] = new CalibrationValue(
                    ParseVector(match, "sx", "sy"),
                    ParseVector(match, "ox", "oy"));
            }
            return result;
        }

        private static Vector2 ParseVector(Match match, string x, string y) => new Vector2(
            float.Parse(match.Groups[x].Value, NumberStyles.Float, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[y].Value, NumberStyles.Float, CultureInfo.InvariantCulture));

        private static void ValidateProfile(SpritePresentationCalibrationProfileSO profile, string id, string path)
        {
            if (profile == null || !string.Equals(profile.ProfileId, id, StringComparison.Ordinal) ||
                profile.Entries == null || profile.Entries.Length != 6)
                throw new InvalidOperationException($"Unity serializer readback failed: {path}");
            for (int i = 0; i < profile.Entries.Length; i++)
                if (profile.Entries[i] == null || profile.Entries[i].FrameIndex != i || !profile.Entries[i].IsValid)
                    throw new InvalidOperationException($"Invalid persisted entry {i}: {path}");
        }

        private static string CanonicalPath(string id) => $"{SoRoot}/{id}.asset";

        private readonly struct CalibrationValue
        {
            public CalibrationValue(Vector2 scale, Vector2 offset) { Scale = scale; Offset = offset; }
            public Vector2 Scale { get; }
            public Vector2 Offset { get; }
        }
    }
}
