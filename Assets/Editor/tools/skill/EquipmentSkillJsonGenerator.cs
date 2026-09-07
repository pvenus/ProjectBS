#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skill;
using UnityEditor;
using UnityEngine;
using ResourceTools.Helper;

namespace ResourceTools.Skill
{
    public static class EquipmentSkillJsonGenerator
    {
        private const string ContentJsonFolder = "Assets/Contents/Skill/json";
        private const string ContentSoFolder = "Assets/Contents/Skill/so";

        // New JSON model (2024-06):
        [Serializable]
        public class EquipmentSkillJson
        {
            public string equipmentId;
            public string aimMode;
            public string aimInputSource;
            public string skillName;
            public string desc;
            public string description;

            public string baseProfile;
            public string cast;
            public string move;
            public string hits;
            public string spawnSkill;
            public string spawn;
            public string upgrade;
            public string baseVisual;
            public string combo;
        }

        [Serializable]
        private sealed class ComboJson
        {
            public bool enabled;
            public float duration;
            public float totalLungeCap;
            public bool useCanonicalBodyChoreography;
            public string segmentedBodyActionClip;
            public int segmentedBodyFrameCount = 18;
            public ComboStepJson[] steps;
        }

        [Serializable]
        private sealed class ComboStepJson
        {
            public int comboIndex;
            public string hitId;
            public string bodyActionClip;
            public int bodySegmentStartFrame = -1;
            public int bodySegmentEndFrame = -1;
            public string vfxClip;
            public string visualClip;
            public string vfxProfile;
            public float minimumVisualLifetime;
            public string bodyPresentationCalibration;
            public string vfxPresentationCalibration;
            public float startTime;
            public float hitTime;
            public float activeEnd;
            public float recoveryEnd;
            public float damageWeight;
            public float lungeDistance;
        }

        [Serializable]
        private class EquipmentSkillRootJson
        {
            public string equipmentId;
            public string aimMode;
            public string aimInputSource;
            public string skillName;
            public string desc;
            public string description;
        }


        // Example JSON format:
        // {
        //   "equipmentId": "skill.military_officer.1.passive_1",
        //   "characterName": "military_officer",
        //   "skillName": "basic_attack",
        // }
        // Usage:
        // Select a json file in the Project window.
        // Right Click -> Skill -> Generate EquipmentSkillSO From Json
        [MenuItem("Assets/Skill/Generate EquipmentSkillSO From Json", false, 2000)]
        public static void Generate()
        {
            TextAsset jsonAsset = Selection.activeObject as TextAsset;

            if (jsonAsset == null)
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Select a json file in the Project window first.");
                return;
            }

            string jsonPath = AssetDatabase.GetAssetPath(jsonAsset);
            GenerateFromJsonPath(jsonPath);
        }

        [MenuItem("Assets/Skill/Generate All EquipmentSkillSO From Folder", false, 2001)]
        public static void GenerateAllFromSelectedFolder()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrWhiteSpace(folderPath) ||
                !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError(
                    "[EquipmentSkillJsonGenerator] Select a folder in the Project window first.");
                return;
            }

            string[] jsonPaths = Directory.GetFiles(
                folderPath,
                "*.json",
                SearchOption.TopDirectoryOnly);
            Array.Sort(jsonPaths, StringComparer.Ordinal);

            if (jsonPaths.Length == 0)
            {
                Debug.LogWarning(
                    $"[EquipmentSkillJsonGenerator] No json files found in folder: {folderPath}");
                return;
            }

            int generatedCount = 0;
            int failedCount = 0;

            for (int i = 0; i < jsonPaths.Length; i++)
            {
                string jsonPath = jsonPaths[i].Replace("\\", "/");

                try
                {
                    if (GenerateFromJsonPath(jsonPath) != null)
                    {
                        generatedCount++;
                    }
                    else
                    {
                        failedCount++;
                    }
                }
                catch (Exception exception)
                {
                    failedCount++;
                    Debug.LogError(
                        $"[EquipmentSkillJsonGenerator] Failed to generate skill. " +
                        $"path={jsonPath}\n{exception}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[EquipmentSkillJsonGenerator] Folder generation completed. " +
                $"folder={folderPath}, generated={generatedCount}, failed={failedCount}");
        }

        public static EquipmentSkillSO GenerateFromJsonPath(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath) || !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Selected asset is not a json file.");
                return null;
            }

            string json = File.ReadAllText(jsonPath);
            EquipmentSkillJson data = ParseEquipmentSkillJson(json);

            BaseVisualJson baseVisual = ParseObject<BaseVisualJson>(data?.baseVisual);
            if (baseVisual != null)
            {
                Debug.Log($"[EquipmentSkillJsonGenerator] BaseVisual detected: {baseVisual.visualId}");
            }

            if (data == null || string.IsNullOrEmpty(data.equipmentId))
            {
                Debug.LogError($"[EquipmentSkillJsonGenerator] Invalid EquipmentSkill json: {jsonPath}");
                return null;
            }

            string outputFolder = ResolveOutputFolder(jsonPath);

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Cannot resolve output folder from json path.");
                return null;
            }

            return CreateOrUpdateSkill(
                data,
                outputFolder,
                jsonPath);
        }

        private static string ResolveOutputFolder(string jsonPath)
        {
            string jsonFolder = Path.GetDirectoryName(jsonPath)?.Replace("\\", "/");
            return string.Equals(jsonFolder, ContentJsonFolder, StringComparison.OrdinalIgnoreCase)
                ? ContentSoFolder
                : jsonFolder;
        }

        public static EquipmentSkillSO CreateOrUpdateSkill(
            EquipmentSkillJson data,
            string outputFolder,
            string sourceJsonPath = null)
        {
            if (data == null || string.IsNullOrEmpty(data.equipmentId))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Invalid EquipmentSkill json data.");
                return null;
            }

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] outputFolder is empty.");
                return null;
            }

            EnsureFolder(outputFolder);

            GenerateSkillString(data);

            BaseProfileJson baseProfile = ParseBaseProfile(data.baseProfile);
            CastJson cast = ParseCast(data.cast);
            MoveJson move = ParseObject<MoveJson>(data.move);
            HitJson[] hits = ParseHitArray(data.hits);
            SpawnSkillJson spawnSkill = ParseObject<SpawnSkillJson>(data.spawnSkill);
            SpawnSkillJson spawn = ParseObject<SpawnSkillJson>(data.spawn);
            SkillUpgradeAsssetBuilder.SkillUpgradeTableJson upgrade =
                ParseObject<SkillUpgradeAsssetBuilder.SkillUpgradeTableJson>(data.upgrade);
            BaseVisualJson baseVisual = ParseObject<BaseVisualJson>(data.baseVisual);

            string assetPath = $"{outputFolder}/{data.equipmentId}.asset";
            EquipmentSkillSO skillSo = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(assetPath);
            bool isNewAsset = false;

            EquipmentBaseProfileSO baseProfileSo =
                HasBaseProfile(baseProfile)
                    ? EquipmentBaseProfileAssetBuilder.CreateOrUpdate(
                        baseProfile,
                        outputFolder)
                    : null;

            SkillCastSO castSo =
                HasCast(cast)
                    ? SkillCastAssetBuilder.CreateOrUpdate(
                        cast,
                        outputFolder) as SkillCastSO
                    : null;

            SkillMoveSO moveSo =
                HasMove(move)
                    ? SkillMoveAssetBuilder.CreateOrUpdate(
                        move,
                        outputFolder) as SkillMoveSO
                    : null;

            SkillHitSO[] hitSos =
                HasHits(hits)
                    ? CreateOrUpdateHits(
                        hits,
                        outputFolder)
                    : Array.Empty<SkillHitSO>();
            int expectedHitCount = hits != null
                ? hits.Count(hit => hit != null && !string.IsNullOrWhiteSpace(hit.hitId))
                : 0;
            if (hitSos.Length != expectedHitCount)
            {
                Debug.LogError(
                    $"[EquipmentSkillJsonGenerator] Hit materialization was incomplete; skill references were not changed. " +
                    $"equipmentId={data.equipmentId} expected={expectedHitCount} actual={hitSos.Length}");
                return null;
            }

            // Support both spawnSkill and spawn for spawn skill JSON
            SpawnSkillJson resolvedSpawnSkill = spawnSkill ?? spawn;
            string resolvedSpawnJson = spawnSkill != null ? data.spawnSkill : data.spawn;
            string childSkillJson = ExtractJsonValue(resolvedSpawnJson, "skill");
            EquipmentSkillJson childSkillData = ParseEquipmentSkillJson(childSkillJson);
            EquipmentSkillSO childSkillSo = childSkillData != null &&
                                             !string.IsNullOrWhiteSpace(childSkillData.equipmentId)
                ? CreateOrUpdateSkill(childSkillData, outputFolder, sourceJsonPath)
                : null;

            SpawnSkillSO spawnSkillSo =
                SkillSpawnAssetBuilder.HasSpawnSkill(resolvedSpawnSkill)
                    ? SkillSpawnAssetBuilder.CreateOrUpdate(
                        resolvedSpawnSkill,
                        outputFolder,
                        childSkillSo)
                    : null;

            EquipmentUpgradeTableSO upgradeTableSo =
                upgrade != null
                    ? SkillUpgradeAsssetBuilder.CreateOrUpdate(
                        upgrade,
                        sourceJsonPath,
                        outputFolder)
                    : null;

            BaseVisualSO baseVisualSo =
                HasBaseVisual(baseVisual)
                    ? SkillBaseVisualAssetBuilder.CreateOrUpdate(
                        baseVisual,
                        outputFolder)
                    : null;

            // Swift Step's one-shot VFX clip is generated by the BaseVisual builder.
            // Reload the cast after that clip exists so the persisted cast never keeps
            // a null/stale reference merely because materialization order differed.
            if (castSo != null && baseVisualSo != null &&
                !string.IsNullOrWhiteSpace(data.equipmentId) &&
                data.equipmentId.EndsWith(".active_4.swift_step", StringComparison.Ordinal))
            {
                castSo = SkillCastAssetBuilder.CreateOrUpdate(
                    cast,
                    outputFolder) as SkillCastSO;
            }

            if (skillSo == null)
            {
                skillSo = ScriptableObject.CreateInstance<EquipmentSkillSO>();
                isNewAsset = true;
            }

            ApplySkillFields(
                skillSo,
                data,
                baseProfileSo,
                castSo,
                moveSo,
                hitSos,
                spawnSkillSo,
                upgradeTableSo,
                baseVisualSo,
                ParseObject<ComboJson>(data.combo));

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(skillSo, assetPath);
                Debug.Log($"[EquipmentSkillJsonGenerator] Created EquipmentSkillSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(skillSo);
                Debug.Log($"[EquipmentSkillJsonGenerator] Updated EquipmentSkillSO: {assetPath}");
            }

            AssetDatabase.SaveAssetIfDirty(skillSo);
            AssetDatabase.SaveAssets();

            return skillSo;
        }


        private static bool HasBaseProfile(
            BaseProfileJson baseProfile)
        {
            return baseProfile != null &&
                   !string.IsNullOrWhiteSpace(baseProfile.baseProfileId);
        }

        private static bool HasCast(
            CastJson cast)
        {
            return cast != null &&
                   !string.IsNullOrWhiteSpace(cast.castId);
        }

        private static bool HasMove(
            MoveJson move)
        {
            return move != null &&
                   !string.IsNullOrWhiteSpace(move.moveId);
        }

        private static bool HasBaseVisual(
            BaseVisualJson baseVisual)
        {
            return baseVisual != null &&
                   !string.IsNullOrWhiteSpace(baseVisual.visualId);
        }


        private static bool HasHits(
            HitJson[] hits)
        {
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] != null &&
                    !string.IsNullOrWhiteSpace(hits[i].hitId))
                {
                    return true;
                }
            }

            return false;
        }

        private static void GenerateSkillString(
            EquipmentSkillJson data)
        {
            if (data == null ||
                string.IsNullOrWhiteSpace(data.equipmentId))
            {
                return;
            }

            SkillStringBuilder.ExtractSkillStrings(
                data.equipmentId,
                data.skillName,
                ResolveSkillDescription(data));
        }

        private static string ResolveSkillDescription(
            EquipmentSkillJson data)
        {
            if (data == null)
            {
                return null;
            }

            return !string.IsNullOrWhiteSpace(data.desc)
                ? data.desc
                : data.description;
        }

        private static void ApplySkillFields(
            EquipmentSkillSO skillSo,
            EquipmentSkillJson data,
            EquipmentBaseProfileSO baseProfileSo,
            SkillCastSO castSo,
            SkillMoveSO moveSo,
            SkillHitSO[] hitSos,
            SpawnSkillSO spawnSkillSo,
            EquipmentUpgradeTableSO upgradeTableSo,
            BaseVisualSO baseVisualSo,
            ComboJson combo)
        {
            SerializedObject serializedObject = new SerializedObject(skillSo);

            SetString(serializedObject, "equipmentId", data.equipmentId);
            SetObjectReference(
                serializedObject,
                "icon",
                FindSpriteById($"{data.equipmentId}.icon"));
            SetObjectReference(serializedObject, "baseProfileSo", baseProfileSo);
            SetObjectReference(serializedObject, "castSo", castSo);
            SetObjectReference(serializedObject, "moveSo", moveSo);
            SetObjectArray(serializedObject, "hitSos", hitSos);
            SetObjectReference(serializedObject, "spawnSkillSo", spawnSkillSo);
            SetObjectReference(serializedObject, "upgradeTableSo", upgradeTableSo);
            SetObjectReference(serializedObject, "baseVisualSo", baseVisualSo);
            ApplyComboProfile(serializedObject, combo, hitSos);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            skillSo.ConfigureAimMode(data.aimMode);
            skillSo.ConfigureAimInputSource(data.aimInputSource);
        }

        private static void ApplyComboProfile(
            SerializedObject serializedObject,
            ComboJson combo,
            SkillHitSO[] hitSos)
        {
            SerializedProperty profile = serializedObject.FindProperty("comboProfile");
            if (profile == null)
            {
                return;
            }

            profile.FindPropertyRelative("enabled").boolValue = combo != null && combo.enabled;
            profile.FindPropertyRelative("duration").floatValue = combo != null ? Mathf.Max(0f, combo.duration) : 0f;
            profile.FindPropertyRelative("totalLungeCap").floatValue = combo != null ? Mathf.Max(0f, combo.totalLungeCap) : 0f;
            profile.FindPropertyRelative("useCanonicalBodyChoreography").boolValue =
                combo != null && combo.useCanonicalBodyChoreography;
            profile.FindPropertyRelative("segmentedBodyActionClip").objectReferenceValue =
                FindAnimationClip(combo != null ? combo.segmentedBodyActionClip : null);
            profile.FindPropertyRelative("segmentedBodyFrameCount").intValue =
                combo != null && combo.segmentedBodyFrameCount > 0
                    ? combo.segmentedBodyFrameCount
                    : 18;

            SerializedProperty steps = profile.FindPropertyRelative("steps");
            ComboStepJson[] sourceSteps = combo != null ? combo.steps : null;
            steps.arraySize = sourceSteps != null ? sourceSteps.Length : 0;
            for (int i = 0; i < steps.arraySize; i++)
            {
                ComboStepJson source = sourceSteps[i];
                SerializedProperty step = steps.GetArrayElementAtIndex(i);
                step.FindPropertyRelative("comboIndex").intValue = source.comboIndex;
                step.FindPropertyRelative("hit").objectReferenceValue = FindHit(hitSos, source.hitId);
                step.FindPropertyRelative("bodyActionClip").objectReferenceValue =
                    FindAnimationClip(source.bodyActionClip);
                step.FindPropertyRelative("bodySegmentStartFrame").intValue =
                    source.bodySegmentStartFrame;
                step.FindPropertyRelative("bodySegmentEndFrame").intValue =
                    source.bodySegmentEndFrame;
                string vfxClip = !string.IsNullOrWhiteSpace(source.vfxClip)
                    ? source.vfxClip
                    : source.visualClip;
                step.FindPropertyRelative("visualClip").objectReferenceValue = FindAnimationClip(vfxClip);
                step.FindPropertyRelative("vfxProfileOverride").objectReferenceValue =
                    FindAnimationVfxProfile(source.vfxProfile);
                step.FindPropertyRelative("minimumVisualLifetime").floatValue =
                    Mathf.Max(0f, source.minimumVisualLifetime);
                step.FindPropertyRelative("bodyPresentationCalibration").objectReferenceValue =
                    FindCalibrationProfile(source.bodyPresentationCalibration);
                step.FindPropertyRelative("vfxPresentationCalibration").objectReferenceValue =
                    FindCalibrationProfile(source.vfxPresentationCalibration);
                step.FindPropertyRelative("startTime").floatValue = source.startTime;
                step.FindPropertyRelative("hitTime").floatValue = source.hitTime;
                step.FindPropertyRelative("activeEnd").floatValue = source.activeEnd;
                step.FindPropertyRelative("recoveryEnd").floatValue = source.recoveryEnd;
                step.FindPropertyRelative("damageWeight").floatValue = source.damageWeight;
                step.FindPropertyRelative("lungeDistance").floatValue = source.lungeDistance;
            }
        }

        private static SkillHitSO FindHit(SkillHitSO[] hitSos, string hitId)
        {
            if (hitSos == null || string.IsNullOrWhiteSpace(hitId))
            {
                return null;
            }

            for (int i = 0; i < hitSos.Length; i++)
            {
                if (hitSos[i] != null && string.Equals(hitSos[i].HitId, hitId, StringComparison.Ordinal))
                {
                    return hitSos[i];
                }
            }

            return null;
        }

        private static SpritePresentationCalibrationProfileSO FindCalibrationProfile(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId)) return null;
            string canonicalPath = $"{ContentSoFolder}/{profileId}.asset";
            AssetDatabase.ImportAsset(
                canonicalPath,
                ImportAssetOptions.ForceSynchronousImport |
                ImportAssetOptions.ForceUpdate);
            SpritePresentationCalibrationProfileSO canonical =
                AssetDatabase.LoadAssetAtPath<SpritePresentationCalibrationProfileSO>(canonicalPath);
            if (canonical == null || !string.Equals(canonical.ProfileId, profileId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Missing or mismatched canonical calibration profile '{profileId}' at '{canonicalPath}'.");
            }
            if (canonical.Entries == null || canonical.Entries.Length != 6)
            {
                throw new InvalidOperationException(
                    $"Calibration profile '{profileId}' must contain exactly 6 persisted entries; " +
                    $"read {canonical.Entries?.Length ?? 0} at '{canonicalPath}'.");
            }
            for (int i = 0; i < canonical.Entries.Length; i++)
            {
                SpritePresentationCalibrationEntry entry = canonical.Entries[i];
                if (entry == null || entry.FrameIndex != i || !entry.IsValid)
                {
                    throw new InvalidOperationException(
                        $"Calibration profile '{profileId}' has invalid persisted frame {i} at '{canonicalPath}'.");
                }
            }
            string[] guids = AssetDatabase.FindAssets("t:SpritePresentationCalibrationProfileSO");
            Array.Sort(guids, StringComparer.Ordinal);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                SpritePresentationCalibrationProfileSO candidate =
                    AssetDatabase.LoadAssetAtPath<SpritePresentationCalibrationProfileSO>(path);
                if (candidate == null || !string.Equals(candidate.ProfileId, profileId, StringComparison.Ordinal)) continue;
                if (candidate != canonical)
                {
                    throw new InvalidOperationException($"Duplicate calibration profile '{profileId}'.");
                }
            }
            return canonical;
        }

        private static SkillAnimationVfxProfileSO FindAnimationVfxProfile(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId)) return null;
            string[] guids = AssetDatabase.FindAssets("t:SkillAnimationVfxProfileSO");
            Array.Sort(guids, StringComparer.Ordinal);
            SkillAnimationVfxProfileSO resolved = null;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                SkillAnimationVfxProfileSO candidate =
                    AssetDatabase.LoadAssetAtPath<SkillAnimationVfxProfileSO>(path);
                if (candidate == null || !string.Equals(candidate.ProfileId, profileId, StringComparison.Ordinal))
                    continue;
                if (resolved != null && resolved != candidate)
                    throw new InvalidOperationException($"Duplicate animation VFX profile '{profileId}'.");
                resolved = candidate;
            }
            if (resolved == null)
                throw new InvalidOperationException($"Missing animation VFX profile '{profileId}'.");
            return resolved;
        }

        private static AnimationClip FindAnimationClip(string clipName)
        {
            if (string.IsNullOrWhiteSpace(clipName))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"{clipName} t:AnimationClip");
            Array.Sort(guids, StringComparer.Ordinal);
            AnimationClip resolved = null;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip != null && string.Equals(clip.name, clipName, StringComparison.Ordinal))
                {
                    if (resolved != null && resolved != clip)
                    {
                        Debug.LogError(
                            $"[EquipmentSkillJsonGenerator] Duplicate AnimationClip name '{clipName}'. " +
                            "Combo binding is fail-closed until the duplicate is removed.");
                        return null;
                    }
                    resolved = clip;
                }
            }

            return resolved;
        }


        private static void SetString(
            SerializedObject serializedObject,
            string propertyName,
            string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property not found: {propertyName}");
                return;
            }

            property.stringValue = value;
        }


        private static void SetObjectReference(
            SerializedObject serializedObject,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property not found: {propertyName}");
                return;
            }

            property.objectReferenceValue = value;
        }

        private static void SetObjectArray<T>(
            SerializedObject serializedObject,
            string propertyName,
            T[] values)
            where T : UnityEngine.Object
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property not found: {propertyName}");
                return;
            }

            if (!property.isArray)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property is not array: {propertyName}");
                return;
            }

            int length = values != null ? values.Length : 0;
            property.arraySize = length;

            for (int i = 0; i < length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }


        private static SkillHitSO[] CreateOrUpdateHits(
            HitJson[] hits,
            string outputFolder)
        {
            if (hits == null || hits.Length == 0)
            {
                return Array.Empty<SkillHitSO>();
            }

            List<SkillHitSO> result = new();

            for (int i = 0; i < hits.Length; i++)
            {
                HitJson hitJson = hits[i];
                if (hitJson == null ||
                    string.IsNullOrWhiteSpace(hitJson.hitId))
                {
                    continue;
                }

                SkillHitSO hitSo = SkillHitAssetBuilder.CreateOrUpdate(
                    hitJson,
                    outputFolder) as SkillHitSO;

                if (hitSo == null)
                {
                    Debug.LogError(
                        $"[EquipmentSkillJsonGenerator] Failed to create hit asset. hitId={hitJson.hitId}");
                    return Array.Empty<SkillHitSO>();
                }

                result.Add(hitSo);
            }

            return result.Count > 0
                ? result.ToArray()
                : Array.Empty<SkillHitSO>();
        }

        [MenuItem("Assets/Skill/Generate EquipmentSkillSO From Json", true)]
        private static bool ValidateGenerate()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem("Assets/Skill/Generate All EquipmentSkillSO From Folder", true)]
        private static bool ValidateGenerateAllFromSelectedFolder()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrWhiteSpace(path) &&
                   AssetDatabase.IsValidFolder(path);
        }

        private static Sprite FindSpriteById(string iconId)
        {
            if (string.IsNullOrEmpty(iconId))
            {
                return null;
            }

            Sprite sprite = SpriteHelper.FindSpriteByName(iconId);
            if (sprite != null)
            {
                return sprite;
            }

            Debug.LogWarning(
                $"[EquipmentSkillJsonGenerator] Icon sprite not found. " +
                $"Expected=Assets/ImagesGenerated/Skill/icon/{iconId}.png");
            return null;
        }

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            folder = folder.Replace("\\", "/");

            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
            string leaf = Path.GetFileName(folder);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(leaf))
            {
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }
        private static EquipmentSkillJson ParseEquipmentSkillJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            EquipmentSkillRootJson root = JsonUtility.FromJson<EquipmentSkillRootJson>(json);
            if (root == null)
            {
                return null;
            }

            string baseVisual = ExtractJsonValue(json, "baseVisual");
            if (string.IsNullOrWhiteSpace(baseVisual))
            {
                string visualSet = ExtractJsonValue(json, "visualSet");
                baseVisual = ExtractJsonValue(visualSet, "baseVisual");
            }

            EquipmentSkillJson data = new EquipmentSkillJson
            {
                equipmentId = root.equipmentId,
                aimMode = root.aimMode,
                aimInputSource = root.aimInputSource,
                skillName = root.skillName,
                desc = root.desc,
                description = root.description,
                baseProfile = ExtractJsonValue(json, "baseProfile"),
                cast = ExtractJsonValue(json, "cast"),
                move = ExtractJsonValue(json, "move"),
                hits = ExtractJsonValue(json, "hits"),
                spawnSkill = ExtractJsonValue(json, "spawnSkill"),
                spawn = ExtractJsonValue(json, "spawn"),
                upgrade = ExtractJsonValue(json, "upgradeTable"),
                baseVisual = baseVisual,
                combo = ExtractJsonValue(json, "combo")
            };

            return data;
        }

        private static BaseProfileJson ParseBaseProfile(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            BaseProfileJson baseProfile = JsonUtility.FromJson<BaseProfileJson>(json);
            if (baseProfile != null)
            {
                baseProfile.projectile = ExtractJsonValue(json, "projectile");
                baseProfile.projectileSpawn = ExtractJsonValue(json, "projectileSpawn");
                baseProfile.brainMeta = ExtractJsonValue(json, "brainMeta");
            }

            return baseProfile;
        }

        private static CastJson ParseCast(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            CastJson cast = JsonUtility.FromJson<CastJson>(json);
            if (cast != null)
            {
                if (string.IsNullOrWhiteSpace(cast.mobilityVfxClipPath))
                    cast.mobilityVfxClipPath = cast.mobilityVfxClip;
                if (string.IsNullOrWhiteSpace(cast.bodyActionClipPath))
                    cast.bodyActionClipPath = cast.bodyActionClip;
                cast.burst = ExtractJsonValue(json, "burst");
                cast.castMove = ExtractJsonValue(json, "castMove");
                cast.selfEffects = ExtractJsonValue(json, "selfEffects");
                cast.postMoveSelfEffects = ExtractJsonValue(json, "postMoveSelfEffects");
                cast.bodyPresentation = ExtractJsonValue(json, "bodyPresentation");
                cast.bodyActionPlayback = ExtractJsonValue(json, "bodyActionPlayback");
            }

            return cast;
        }

        private static HitJson[] ParseHitArray(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            string[] itemJsons = ExtractJsonArrayItems(json);
            if (itemJsons == null || itemJsons.Length == 0)
            {
                return Array.Empty<HitJson>();
            }

            HitJson[] result = new HitJson[itemJsons.Length];
            for (int i = 0; i < itemJsons.Length; i++)
            {
                string itemJson = itemJsons[i];
                HitJson hitJson = JsonUtility.FromJson<HitJson>(itemJson);

                if (hitJson != null)
                {
                    hitJson.damage = ExtractJsonValue(itemJson, "damage");
                    hitJson.buffEffects = ExtractJsonValue(itemJson, "buffEffects");
                    hitJson.debuffEffects = ExtractJsonValue(itemJson, "debuffEffects");
                    hitJson.split = ExtractJsonValue(itemJson, "split");
                }

                result[i] = hitJson;
            }

            return result;
        }

        private static string[] ExtractJsonArrayItems(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            int startIndex = json.IndexOf('[');
            int endIndex = json.LastIndexOf(']');
            if (startIndex < 0 || endIndex <= startIndex)
            {
                return null;
            }

            List<string> items = new();
            int index = startIndex + 1;

            while (index < endIndex)
            {
                while (index < endIndex &&
                       (char.IsWhiteSpace(json[index]) || json[index] == ','))
                {
                    index++;
                }

                if (index >= endIndex)
                {
                    break;
                }

                if (json[index] != '{')
                {
                    break;
                }

                string item = ExtractBalancedJson(json, index, '{', '}');
                if (string.IsNullOrWhiteSpace(item))
                {
                    break;
                }

                items.Add(item);
                index += item.Length;
            }

            return items.ToArray();
        }
        private static T ParseObject<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return JsonUtility.FromJson<T>(json);
        }

        private static T[] ParseArray<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            ArrayWrapper<T> wrapper = JsonUtility.FromJson<ArrayWrapper<T>>($"{{\"items\":{json}}}");
            return wrapper != null
                ? wrapper.items
                : null;
        }

        [Serializable]
        private class ArrayWrapper<T>
        {
            public T[] items;
        }

        private static string ExtractJsonValue(string json, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            int keyIndex = FindTopLevelProperty(json, propertyName);
            if (keyIndex < 0)
            {
                return null;
            }

            int colonIndex = json.IndexOf(':', keyIndex + propertyName.Length + 2);
            if (colonIndex < 0)
            {
                return null;
            }

            int valueStart = colonIndex + 1;
            while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart]))
            {
                valueStart++;
            }

            if (valueStart >= json.Length)
            {
                return null;
            }

            char startChar = json[valueStart];
            if (startChar == '{')
            {
                return ExtractBalancedJson(json, valueStart, '{', '}');
            }

            if (startChar == '[')
            {
                return ExtractBalancedJson(json, valueStart, '[', ']');
            }

            return null;
        }

        private static int FindTopLevelProperty(string json, string propertyName)
        {
            int objectDepth = 0;
            int arrayDepth = 0;
            bool inString = false;
            bool escape = false;

            for (int i = 0; i < json.Length; i++)
            {
                char current = json[i];
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (current == '\\' && inString)
                {
                    escape = true;
                    continue;
                }

                if (current == '"')
                {
                    if (!inString && objectDepth == 1 && arrayDepth == 0)
                    {
                        int endQuote = i + 1;
                        while (endQuote < json.Length && json[endQuote] != '"')
                        {
                            if (json[endQuote] == '\\') endQuote++;
                            endQuote++;
                        }

                        if (endQuote < json.Length &&
                            string.Equals(
                                json.Substring(i + 1, endQuote - i - 1),
                                propertyName,
                                StringComparison.Ordinal))
                        {
                            int afterKey = endQuote + 1;
                            while (afterKey < json.Length && char.IsWhiteSpace(json[afterKey])) afterKey++;
                            if (afterKey < json.Length && json[afterKey] == ':') return i;
                        }
                    }

                    inString = !inString;
                    continue;
                }

                if (inString) continue;

                switch (current)
                {
                    case '{': objectDepth++; break;
                    case '}': objectDepth--; break;
                    case '[': arrayDepth++; break;
                    case ']': arrayDepth--; break;
                }
            }

            return -1;
        }

        private static string ExtractBalancedJson(
            string json,
            int startIndex,
            char openChar,
            char closeChar)
        {
            int depth = 0;
            bool inString = false;
            bool escape = false;

            for (int i = startIndex; i < json.Length; i++)
            {
                char current = json[i];

                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (current == '\\')
                {
                    escape = true;
                    continue;
                }

                if (current == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (inString)
                {
                    continue;
                }

                if (current == openChar)
                {
                    depth++;
                }
                else if (current == closeChar)
                {
                    depth--;
                    if (depth == 0)
                    {
                        return json.Substring(startIndex, i - startIndex + 1);
                    }
                }
            }

            return null;
        }
    }
}
#endif
