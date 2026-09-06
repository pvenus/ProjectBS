#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Character;
using ResourceTools.Helper;
using Skill;
using UnityEditor;
using UnityEngine;
using EquipmentSkillGenerator = ResourceTools.Skill.EquipmentSkillJsonGenerator;

namespace ResourceTools.Character
{
    public static class Episode1Npc2Complete48Materializer
    {
        private const string SourceRoot =
            "/Users/pvenus/imageforge-mcp/generated_image";
        private const string SelectedManifest =
            "Artifacts/installation/episode1-npc2-imageforge-exact48-20260905/source-target-manifest.json";
        private const string SelectedManifestSha =
            "ffb42f5addbc14fda203b2fab5b8bf95e492f1062ae5cf2a869ce2a5b54149f9";
        private const string LifecycleManifest =
            "Artifacts/candidates/episode1-exact2-npc-selected-expansion-v20260904/selected-border-safe-v002/vfx-lifecycle-authority-v2.json";
        private const string LifecycleManifestSha =
            "4581d47a7274f695d152db2662f5b67dd966445b1f661f593d4c00d9679d56a9";

        private sealed class Spec
        {
            public string Id;
            public string Source;
            public string SkillJson;
            public float Contact;
            public float Recovery;
        }

        private static readonly Spec[] Specs =
        {
            new Spec { Id="character.black_cloth_raider.1", Source="black_cloth_raider",
                SkillJson="Assets/Contents/Skill/json/skill.character.black_cloth_raider.1.basic_attack.black_knife_cut.json",
                Contact=.12f, Recovery=.34f },
            new Spec { Id="character.chain_dragger_raider.1", Source="chain_dragger_raider",
                SkillJson="Assets/Contents/Skill/json/skill.character.chain_dragger_raider.1.basic_attack.chain_swing.json",
                Contact=.18f, Recovery=.45f }
        };

        [MenuItem("Assets/Resource Tools/Episode1/Materialize NPC2 COMPLETE48", false, 2150)]
        public static void Materialize()
        {
            if (Sha256(SelectedManifest) != SelectedManifestSha ||
                Sha256(LifecycleManifest) != LifecycleManifestSha)
                throw new InvalidOperationException("Episode1 NPC2 authority manifest SHA mismatch.");

            List<string> created = new();
            string backupRoot = Path.Combine(Path.GetTempPath(),
                "projectbs-episode1-npc2-complete48-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
            Directory.CreateDirectory(backupRoot);
            Dictionary<string, string> backups = SnapshotMutableAssets(backupRoot);
            try
            {
                foreach (Spec spec in Specs) PrepareContent(spec, created);
                foreach (Spec spec in Specs)
                {
                    if (EquipmentSkillGenerator.GenerateFromJsonPath(spec.SkillJson) == null)
                        throw new InvalidOperationException("Skill materialization failed: " + spec.SkillJson);
                    string characterJson = $"Assets/Contents/Character/json/{spec.Id}.json";
                    CharacterSO character = CharacterJsonGenerator.GenerateFromJsonPath(characterJson);
                    if (character == null || character.AnimationProfile == null)
                        throw new InvalidOperationException("Character/profile materialization failed: " + spec.Id);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                ValidateReadback();
                Debug.Log("[Episode1Npc2Complete48] ATOMIC_MATERIALIZATION_PASS backup=" + backupRoot);
            }
            catch
            {
                for (int i = created.Count - 1; i >= 0; i--)
                    if (AssetDatabase.LoadMainAssetAtPath(created[i]) != null || File.Exists(created[i]))
                        AssetDatabase.DeleteAsset(created[i]);
                foreach (KeyValuePair<string, string> pair in backups)
                {
                    File.Copy(pair.Value, pair.Key, true);
                }
                foreach (string path in backups.Keys)
                    if (!path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.SaveAssets();
                throw;
            }
        }

        private static void PrepareContent(Spec spec, List<string> created)
        {
            string imageRoot = $"Assets/ImagesGenerated/Character/animation/{spec.Id}/v2";
            string clipRoot = $"Assets/AnimationClips/Character/{spec.Id}";
            EnsureFolder(imageRoot); EnsureFolder(clipRoot);
            Dictionary<string, Sprite[]> sprites = new();
            foreach (string action in new[] { "Idle", "Move", "BasicAttack", "Death", "Stun", "VFX" })
            {
                string targetAction = action == "VFX" ? "impact_neon_v1" : action.ToLowerInvariant();
                sprites[action] = InstallFrames(spec.Source, action, imageRoot + "/" + targetAction, created);
            }

            CreateDirectional(clipRoot, spec.Id, "idle", sprites["Idle"], 12f, true, created);
            CreateDirectional(clipRoot, spec.Id, "move", sprites["Move"], 12f, true, created);
            CreateTimedClip(clipRoot + $"/{spec.Id}.basic_attack.v2.Right.anim",
                spec.Id + ".basic_attack.v2.Right", sprites["BasicAttack"],
                new[] { 0f, spec.Contact * .5f, spec.Contact, spec.Recovery }, spec.Recovery, false, false, created);
            CreateTimedClip(clipRoot + $"/{spec.Id}.basic_attack.v2.Left.anim",
                spec.Id + ".basic_attack.v2.Left", sprites["BasicAttack"],
                new[] { 0f, spec.Contact * .5f, spec.Contact, spec.Recovery }, spec.Recovery, false, true, created);
            CreateTimedClip(clipRoot + $"/{spec.Id}.death.v2.anim", spec.Id + ".death.v2",
                sprites["Death"], new[] { 0f, .1f, .2f, .3f }, .34f, false, false, created);
            CreateTimedClip(clipRoot + $"/{spec.Id}.stun.v2.anim", spec.Id + ".stun.v2",
                sprites["Stun"], new[] { 0f, .1f, .2f, .3f }, .4f, true, false, created);
            CreateTimedClip(clipRoot + $"/{spec.Id}.impact.neon.v1.anim", spec.Id + ".impact.neon.v1",
                sprites["VFX"], new[] { 0f, .06f, .12f, .18f }, .24f, false, false, created);
        }

        private static Sprite[] InstallFrames(string sourceNpc, string sourceAction, string targetFolder,
            List<string> created)
        {
            EnsureFolder(targetFolder);
            Sprite[] result = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                string source = $"{SourceRoot}/{sourceNpc}/{sourceAction}/frame_{i + 1:00}.png";
                string target = $"{targetFolder}/frame-{i:00}.png";
                if (!File.Exists(source)) throw new FileNotFoundException("Missing selected frame", source);
                if (!File.Exists(target)) { File.Copy(source, target); created.Add(target); }
                else if (Sha256(source) != Sha256(target))
                    File.Copy(source, target, true); // manifest-authenticated replacement; .meta/GUID is retained
                AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceSynchronousImport);
                TextureImporter importer = AssetImporter.GetAtPath(target) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("TextureImporter missing: " + target);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.spritePivot = new Vector2(.5f, .5f);
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                result[i] = AssetDatabase.LoadAssetAtPath<Sprite>(target);
                if (result[i] == null) throw new InvalidOperationException("Sprite reload failed: " + target);
            }
            return result;
        }

        private static void CreateDirectional(string root, string id, string action, Sprite[] sprites,
            float fps, bool loop, List<string> created)
        {
            CreateStandard(root + $"/{id}.{action}.v2.Right.anim", sprites, fps, loop, false, created);
            CreateStandard(root + $"/{id}.{action}.v2.Left.anim", sprites, fps, loop, true, created);
        }

        private static void CreateStandard(string path, Sprite[] sprites, float fps, bool loop, bool flip,
            List<string> created)
        {
            bool existed = File.Exists(path);
            AnimationClip clip = path.IndexOf(".idle.", StringComparison.OrdinalIgnoreCase) >= 0
                ? AnimationClipAssetHelper.CreateOrUpdateCharacterIdleLoop(path, sprites, flip)
                : AnimationClipAssetHelper.CreateOrUpdateSpriteAnimationClip(path, sprites, fps, loop, flip);
            if (clip == null)
                throw new InvalidOperationException("Clip creation failed: " + path);
            if (!existed) created.Add(path);
        }

        private static void CreateTimedClip(string path, string clipName, Sprite[] sprites, float[] times,
            float stop, bool loop, bool flip, List<string> created)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                if (File.Exists(path)) throw new InvalidOperationException("Unreadable clip: " + path);
                clip = new AnimationClip { name = clipName, frameRate = 60f };
                AssetDatabase.CreateAsset(clip, path); created.Add(path);
            }
            ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < keys.Length; i++) keys[i] = new ObjectReferenceKeyframe { time=times[i], value=sprites[i] };
            AnimationUtility.SetObjectReferenceCurve(clip,
                EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite"), keys);
            EditorCurveBinding flipBinding = EditorCurveBinding.FloatCurve(string.Empty, typeof(SpriteRenderer), "m_FlipX");
            AnimationUtility.SetEditorCurve(clip, flipBinding,
                AnimationCurve.Constant(0f, stop, flip ? 1f : 0f));
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop; settings.startTime = 0f; settings.stopTime = stop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip); AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }

        private static Dictionary<string, string> SnapshotMutableAssets(string root)
        {
            Dictionary<string, string> result = new();
            foreach (Spec spec in Specs)
            {
                string safe = spec.Id.Replace('.', '_');
                foreach (string path in new[] {
                    $"Assets/Contents/Character/so/{safe}.asset",
                    spec.SkillJson.Replace("/json/", "/so/").Replace(".json", ".asset"),
                    spec.SkillJson.Replace("/json/", "/so/").Replace(".json", ".cast.asset"),
                    spec.SkillJson.Replace("/json/", "/so/").Replace(".json", ".profile.asset"),
                    spec.SkillJson.Replace("/json/", "/so/").Replace(".json", ".visual.asset") })
                {
                    if (!File.Exists(path)) continue;
                    string backup = Path.Combine(root, Path.GetFileName(path)); File.Copy(path, backup, true);
                    result[path] = backup;
                }

                SnapshotTree(
                    $"Assets/ImagesGenerated/Character/animation/{spec.Id}/v2",
                    root,
                    result);
                SnapshotTree(
                    $"Assets/AnimationClips/Character/{spec.Id}",
                    root,
                    result);
            }
            return result;
        }

        private static void SnapshotTree(
            string tree,
            string backupRoot,
            Dictionary<string, string> result)
        {
            if (!Directory.Exists(tree)) return;
            foreach (string path in Directory.GetFiles(tree, "*", SearchOption.AllDirectories))
            {
                string relative = path.Replace('\\', '/');
                string backup = Path.Combine(backupRoot, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(backup));
                File.Copy(path, backup, true);
                result[relative] = backup;
            }
        }

        private static void ValidateReadback()
        {
            foreach (Spec spec in Specs)
            {
                string safe = spec.Id.Replace('.', '_');
                CharacterSO character = AssetDatabase.LoadAssetAtPath<CharacterSO>(
                    $"Assets/Contents/Character/so/{safe}.asset");
                EquipmentSkillSO skill = EquipmentSkillGenerator.GenerateFromJsonPath(spec.SkillJson);
                if (character == null || character.AnimationProfile == null || skill == null || skill.CastSo == null ||
                    skill.CastSo.BodyActionClip == null ||
                    Mathf.Abs(skill.CastSo.BodyActionPlayback.ContactTime - spec.Contact) > .0001f ||
                    Mathf.Abs(skill.BaseProfileSo.RendererScale - .72f) > .0001f || skill.BaseVisualSo == null)
                    throw new InvalidOperationException("NPC2 readback mismatch: " + spec.Id);

                AnimationClip impactClip = null;
                AnimationClipEntry[] visualClips = skill.BaseVisualSo.AnimationClips;
                if (visualClips != null)
                    for (int i = 0; i < visualClips.Length; i++)
                        if (visualClips[i] != null &&
                            visualClips[i].ClipType == SkillAnimationClipType.ProjectileLoop)
                            impactClip = visualClips[i].Clip;
                string expectedImpact =
                    $"Assets/AnimationClips/Character/{spec.Id}/{spec.Id}.impact.neon.v1.anim";
                if (impactClip == null || AssetDatabase.GetAssetPath(impactClip) != expectedImpact)
                    throw new InvalidOperationException("NPC2 impact clip identity mismatch: " + spec.Id);

                string imageRoot = $"Assets/ImagesGenerated/Character/animation/{spec.Id}/v2";
                foreach (string action in new[] { "Idle", "Move", "BasicAttack", "Death", "Stun", "VFX" })
                {
                    string targetAction = action == "VFX" ? "impact_neon_v1" : action.ToLowerInvariant();
                    for (int i = 0; i < 4; i++)
                    {
                        string source = $"{SourceRoot}/{spec.Source}/{action}/frame_{i + 1:00}.png";
                        string target = $"{imageRoot}/{targetAction}/frame-{i:00}.png";
                        TextureImporter importer = AssetImporter.GetAtPath(target) as TextureImporter;
                        if (Sha256(source) != Sha256(target) || importer == null ||
                            importer.textureType != TextureImporterType.Sprite ||
                            importer.spriteImportMode != SpriteImportMode.Single ||
                            Mathf.Abs(importer.spritePixelsPerUnit - 100f) > .0001f ||
                            importer.filterMode != FilterMode.Point || importer.wrapMode != TextureWrapMode.Clamp ||
                            !importer.alphaIsTransparency || importer.mipmapEnabled)
                            throw new InvalidOperationException("NPC2 frame/importer mismatch: " + target);
                    }
                }
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/'); EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static string Sha256(string path)
        {
            using SHA256 hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }
    }
}
#endif
