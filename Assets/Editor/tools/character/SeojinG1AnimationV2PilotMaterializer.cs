#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Character;
using ResourceTools.Helper;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Character
{
    public static class SeojinG1AnimationV2PilotMaterializer
    {
        public const string CharacterId = "character.seojin.1";

        private const string PackageRoot =
            "/Users/pvenus/ProjectBS/Artifacts/GraphicsRemediation/CharacterAnimation/SeojinG1SixActions/selected/revision-06-deathc-stunb-final";
        private const string ManifestSha = "37a234a2a7e766973cb6518c211e5684ae59429dcef6a56de6322ebf82b892c1";
        private const string ImageRoot =
            "Assets/ImagesGenerated/Character/animation/character.seojin.1";
        private const string BasicAttackImageRoot =
            ImageRoot + "/basic_attack";
        private const string ClipRoot =
            "Assets/AnimationClips/Character/character.seojin.1";
        private const string ProfileJson =
            "Assets/Contents/Character/animation/character.seojin.1.animation-profile.v2.json";
        private const string ProfileAsset =
            "Assets/Contents/Character/so/character_seojin_1.animation-profile.asset";
        private const string CharacterAsset =
            "Assets/Contents/Character/so/character_seojin_1.asset";
        private const string CharacterJson =
            "Assets/Contents/Character/json/character.seojin.1.json";

        private static readonly string[] DeathSha =
        {
            "8b8d06e1fdd06677f18aecaa908b60cc04587c46d8ba5efa1e3c0c9e409cc7b2",
            "afbfb20b3e78b9aedd9c0f64e2d91d186113a594dae074f5eaa73ad4a5879290",
            "3484120ca4d07c101da53ea1873ed0b403f117636a47b2fd3005f98bbce51071",
            "293c36c262b05826a00e83c9be7133639e9015d5fd2bcbf4e3ebeb3342ab1d15",
            "3a2f753a77061a68910ef35329718c7dc672901fd7295ec0a5c24c114a1a0254",
            "5710eaa5da482352f490ed71f9e95b85decd4e210ff17acbcdbf13fd4be6af53"
        };
        private static readonly string[] StunSha =
        {
            "7160eee44c749cfb7443b404b6cc33c8a4378c91d847014dfccd9ae4358c911c",
            "cacc9ef7081077a51222ce9772ca67d3aefcc7d80c620b4426b4cbbf41097545",
            "bb25eb43b8d48be6842c7d42c7598add4f933aa857a117b048ba7beec3ed859e",
            "523e1654e1a7b542b0fb6394e900f8dfacc8a3ec23c8ccc023f2fcd82f44af08"
        };

        private static readonly string[] CanonicalSkillJsonPaths =
        {
            "Assets/Contents/Skill/json/skill.character.seojin.1.active_1.active_1.json",
            "Assets/Contents/Skill/json/skill.character.seojin.1.active_4.swift_step.json",
            "Assets/Contents/Skill/json/skill.character.seojin.1.basic_attack.basic_attack.json",
            "Assets/Contents/Skill/json/skill.character.seojin.2.active_1.charge.json",
            "Assets/Contents/Skill/json/skill.character.seojin.2.active_2.crane_wing_formation.json",
            "Assets/Contents/Skill/json/skill.character.seojin.2.active_4.swift_step.json",
            "Assets/Contents/Skill/json/skill.character.seojin.3.active_1.charge.json",
            "Assets/Contents/Skill/json/skill.character.seojin.3.active_2.crane_wing_formation.json",
            "Assets/Contents/Skill/json/skill.character.seojin.3.active_3.turtle_ship_assault.json",
            "Assets/Contents/Skill/json/skill.character.seojin.3.active_4.swift_step.json"
        };

        [MenuItem("Assets/Resource Tools/Character Animation v2/Materialize Seojin G1 P2 Pilot", false, 2100)]
        public static void Materialize()
        {
            if (!TryPrepareContent(out string prepareError))
                throw new InvalidOperationException(prepareError);

            List<string> created = new();
            CharacterSO character = CharacterJsonGenerator.GenerateFromJsonPath(CharacterJson);
            if (character == null)
                throw new InvalidOperationException("Canonical Seojin CharacterSO materialization failed.");
            CharacterAnimationProfileSO previousProfile = character != null ? character.AnimationProfile : null;
            try
            {
                RegenerateCanonicalSkillReferences();
                bool profileExisted = File.Exists(ProfileAsset);
                CharacterAnimationProfileSO profile = CharacterAnimationProfileAssetBuilder.Build(ProfileJson, ProfileAsset);
                if (!profileExisted) created.Add(ProfileAsset);
                if (!CharacterAnimationMigrationTransaction.TryPromote(character, profile, out string error))
                    throw new InvalidOperationException("Profile promotion failed: " + error);
                CharacterAnimationProfileReportWriter.Write(ProfileJson, ProfileAsset, profile);
                string validationReport = ProfileAsset + ".validation.json";
                if (!created.Contains(validationReport)) created.Add(validationReport);
                AssetDatabase.SaveAssets();
                Debug.Log("[CharacterAnimationV2] SEOJIN_G1_P2_CONTENT_COMPLETE. " +
                    $"Death={DeathSha.Length}, Stun={StunSha.Length}, Profile={ProfileAsset}");
            }
            catch
            {
                character = AssetDatabase.LoadAssetAtPath<CharacterSO>(CharacterAsset);
                if (character != null && character.AnimationProfile != previousProfile)
                {
                    character.ApplyEditorAnimationProfile(previousProfile);
                    EditorUtility.SetDirty(character);
                    AssetDatabase.SaveAssets();
                }
                for (int i = created.Count - 1; i >= 0; i--) AssetDatabase.DeleteAsset(created[i]);
                throw;
            }
        }

        private static void RegenerateCanonicalSkillReferences()
        {
            foreach (string jsonPath in CanonicalSkillJsonPaths)
            {
                if (!File.Exists(jsonPath))
                    throw new FileNotFoundException("Canonical Seojin skill json is missing.", jsonPath);
                if (Skill.EquipmentSkillJsonGenerator.GenerateFromJsonPath(jsonPath) == null)
                    throw new InvalidOperationException($"Skill materialization failed: {jsonPath}");
            }
        }

        /// <summary>
        /// Installs the approved source frames and timing-specific clips without requiring a CharacterSO.
        /// This preparation is optional so transitional characters can still use the legacy animation path.
        /// </summary>
        public static bool TryPrepareContent(out string error)
        {
            List<string> created = new();
            try
            {
                ValidateBasicAttackFrames();
                EnsureFolder(ClipRoot);
                CreateDirectionalClips("idle", 6, 12f, true, created);
                CreateDirectionalClips("run", 4, 12f, true, created);
                CreateBodyActionClip("charge", 6, 12f, false, created);
                CreateBodyActionClip("crane_cast", 6, 12f, false, created);
                CreateBodyActionClip("dash", 6, 12f, false, created);
                CreateBodyActionClip("turtle_summon", 6, 12f, false, created);
                Sprite[] death = LoadOrInstallAcceptedFrames("Death", "death", DeathSha, created);
                Sprite[] stun = LoadOrInstallAcceptedFrames("Stun", "stun", StunSha, created);
                Sprite[] basicAttack = LoadCanonicalFrames("basic_attack", 18);
                CreateClip(
                    $"{ClipRoot}/character.seojin.1.basic_attack.combo.continuous18.body.anim",
                    "character.seojin.1.basic_attack.combo.continuous18.body", basicAttack,
                    BuildUniformTimes(18, .1f), 1.8f, false, created);
                AnimationClip deathClip = CreateClip(
                    $"{ClipRoot}/character.seojin.1.death.c.user-manual.anim",
                    "character.seojin.1.death.c.user-manual", death,
                    new[] { 0f, .15f, .30f, .45f, .60f, .78f }, 1.48f, false, created);
                AnimationClip stunClip = CreateClip(
                    $"{ClipRoot}/character.seojin.1.stun.b.user-manual.anim",
                    "character.seojin.1.stun.b.user-manual", stun,
                    new[] { 0f, .17f, .34f, .51f }, .68f, true, created);
                if (deathClip == null || stunClip == null)
                    throw new InvalidOperationException("Clip reload failed.");

                error = null;
                return true;
            }
            catch (Exception exception)
            {
                for (int i = created.Count - 1; i >= 0; i--)
                    AssetDatabase.DeleteAsset(created[i]);
                error = exception.Message;
                return false;
            }
        }

        private static void ValidateBasicAttackFrames()
        {
            const int frameCount = 18;
            const int expectedWidth = 568;
            const int expectedHeight = 340;
            for (int i = 0; i < frameCount; i++)
            {
                string path = $"{BasicAttackImageRoot}/frame-{i:00}.png";
                if (!File.Exists(path))
                    throw new FileNotFoundException("BasicAttack transitional frame is not available.", path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null || texture.width != expectedWidth || texture.height != expectedHeight)
                    throw new InvalidOperationException(
                        $"BasicAttack frame must preserve the approved {expectedWidth}x{expectedHeight} canvas: {path}");
            }
        }

        private static Sprite[] LoadCanonicalFrames(string action, int frameCount)
        {
            Sprite[] sprites = new Sprite[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                string path = $"{ImageRoot}/{action}/frame-{i:00}.png";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprites[i] == null)
                    throw new InvalidOperationException($"Canonical sprite is missing: {path}");
            }
            return sprites;
        }

        private static float[] BuildUniformTimes(int frameCount, float step)
        {
            float[] times = new float[frameCount];
            for (int i = 0; i < frameCount; i++) times[i] = i * step;
            return times;
        }

        private static void CreateDirectionalClips(string action, int frameCount, float frameRate,
            bool loop, List<string> created)
        {
            Sprite[] sprites = LoadCanonicalFrames(action, frameCount);
            CreateStandardClip(
                $"{ClipRoot}/character.seojin.1.{action}.user-manual.Right.anim",
                sprites, frameRate, loop, false, created);
            CreateStandardClip(
                $"{ClipRoot}/character.seojin.1.{action}.user-manual.Left.anim",
                sprites, frameRate, loop, true, created);
        }

        private static void CreateBodyActionClip(string action, int frameCount, float frameRate,
            bool loop, List<string> created)
        {
            CreateStandardClip(
                $"{ClipRoot}/character.seojin.1.body_action.{action}.user-manual.anim",
                LoadCanonicalFrames(action, frameCount), frameRate, loop, false, created);
        }

        private static void CreateStandardClip(string path, Sprite[] sprites, float frameRate, bool loop,
            bool flipX, List<string> created)
        {
            bool existed = File.Exists(path);
            AnimationClip clip = path.IndexOf(".idle.", StringComparison.OrdinalIgnoreCase) >= 0
                ? AnimationClipAssetHelper.CreateOrUpdateCharacterIdleLoop(path, sprites, flipX)
                : AnimationClipAssetHelper.CreateOrUpdateSpriteAnimationClip(
                    path, sprites, frameRate, loop, flipX);
            if (clip == null) throw new InvalidOperationException($"Clip generation failed: {path}");
            if (!existed) created.Add(path);
        }

        private static Sprite[] InstallFrames(string sourceAction, string targetAction, string[] expectedSha,
            List<string> created)
        {
            string targetFolder = $"{ImageRoot}/{targetAction}";
            EnsureFolder(targetFolder);
            Sprite[] sprites = new Sprite[expectedSha.Length];
            for (int i = 0; i < expectedSha.Length; i++)
            {
                string source = $"{PackageRoot}/frames/{sourceAction}/frame-{i:00}.png";
                string target = $"{targetFolder}/frame-{i:00}.png";
                if (Sha256(source) != expectedSha[i]) throw new InvalidOperationException($"Frame SHA mismatch: {source}");
                if (!File.Exists(target))
                {
                    File.Copy(source, target);
                    created.Add(target);
                }
                else if (Sha256(target) != expectedSha[i])
                    throw new InvalidOperationException($"Refusing to overwrite different canonical frame: {target}");
                AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceSynchronousImport);
                TextureImporter importer = AssetImporter.GetAtPath(target) as TextureImporter;
                if (importer == null) throw new InvalidOperationException($"TextureImporter missing: {target}");
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(target);
                if (sprites[i] == null) throw new InvalidOperationException($"Sprite reload failed: {target}");
            }
            return sprites;
        }

        private static Sprite[] LoadOrInstallAcceptedFrames(string sourceAction, string targetAction,
            string[] expectedSha, List<string> created)
        {
            string targetFolder = $"{ImageRoot}/{targetAction}";
            bool completeCanonicalSet = true;
            for (int i = 0; i < expectedSha.Length; i++)
            {
                string target = $"{targetFolder}/frame-{i:00}.png";
                if (!File.Exists(target) || Sha256(target) != expectedSha[i])
                {
                    completeCanonicalSet = false;
                    break;
                }
            }

            if (completeCanonicalSet)
                return LoadCanonicalFrames(targetAction, expectedSha.Length);

            string manifestPath = Path.Combine(PackageRoot, "manifest.json");
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException(
                    $"Canonical {targetAction} frames are incomplete and the accepted source package is unavailable.",
                    manifestPath);
            if (Sha256(manifestPath) != ManifestSha)
                throw new InvalidOperationException("Accepted DeathC/StunB manifest SHA mismatch.");
            return InstallFrames(sourceAction, targetAction, expectedSha, created);
        }

        private static AnimationClip CreateClip(string path, string name, Sprite[] sprites, float[] times,
            float stop, bool loop, List<string> created)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                if (File.Exists(path)) throw new InvalidOperationException($"Unreadable clip exists: {path}");
                clip = new AnimationClip { name = name, frameRate = 60f };
                AssetDatabase.CreateAsset(clip, path);
                created.Add(path);
            }
            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
            ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < keys.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = times[i], value = sprites[i] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.startTime = 0f;
            settings.stopTime = stop;
            settings.keepOriginalPositionY = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
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
