using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ResourceTools.Helper;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    public static class JangdanDungLayeredPresentationMaterializer
    {
        private const string SourceRoot =
            "Artifacts/GraphicsRemediation/SkillAnimation/JangdanDungLayeredVFX/selected/revision-01";
        private const string ExpectedManifestSha =
            "ea372841048a077df7d42e49cc3a1801ca117c29b4f2c5dc268b42e6f16ccafc";
        private const string SkillId = "skill.character.seojin.1.active_5.jangdan_dung";
        private const string ProfileId = SkillId + ".layered.v1";
        private const string ImageRoot =
            "Assets/ImagesGenerated/Skill/animation/skill.character.seojin.1.active_5.jangdan_dung/layered-v1";
        private const string ClipRoot = "Assets/AnimationClips/Skill";
        private const string ProfilePath =
            "Assets/Contents/Skill/so/skill.character.seojin.1.active_5.jangdan_dung.layered.v1.asset";
        private const string VisualPath =
            "Assets/Contents/Skill/so/skill.character.seojin.1.active_5.jangdan_dung.visual.asset";

        private sealed class LayerSpec
        {
            internal string source;
            internal string canonical;
            internal int count;
            internal int size;
            internal float start;
            internal float lifetime;
            internal float stop;
            internal float[] keys;
            internal bool loop;
            internal ProjectilePresentationLayerRole role;
            internal bool required;
            internal int order;
        }

        private static readonly LayerSpec[] Specs =
        {
            new() { source="groundField", canonical="ground-field", count=4, size=512,
                start=0f, lifetime=1.5f, stop=.5f, keys=new[]{0f,.125f,.25f,.375f},
                loop=true, role=ProjectilePresentationLayerRole.GroundField, required=true, order=0 },
            new() { source="pullFlow", canonical="pull-flow", count=6, size=256,
                start=.25f, lifetime=.75f, stop=.75f, keys=new[]{0f,.15f,.3f,.45f,.6f,.75f},
                loop=false, role=ProjectilePresentationLayerRole.PullFlow, order=1 },
            new() { source="setupPulse", canonical="setup-pulse", count=3, size=256,
                start=.8f, lifetime=.3f, stop=.3f, keys=new[]{0f,.15f,.3f},
                loop=false, role=ProjectilePresentationLayerRole.SetupPulse, order=2 },
            new() { source="finishResidue", canonical="finish-residue", count=4, size=256,
                start=1.1f, lifetime=.4f, stop=.4f, keys=new[]{0f,.125f,.25f,.4f},
                loop=false, role=ProjectilePresentationLayerRole.Residue, order=3 }
        };

        [MenuItem("Tools/ProjectBS/Skills/Materialize Seojin Jangdan Dung Layered VFX")]
        public static void Materialize()
        {
            ValidateSource();
            var clips = new List<AnimationClip>();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (LayerSpec spec in Specs)
                {
                    string folder = $"{ImageRoot}/{spec.canonical}";
                    EnsureFolder(folder);
                    var paths = new string[spec.count];
                    for (int i = 0; i < spec.count; i++)
                    {
                        string source = Path.GetFullPath(
                            $"{SourceRoot}/{spec.source}/frame-{i:00}.png");
                        string target = $"{folder}/frame-{i:00}.png";
                        if (!File.Exists(target) || !BytesEqual(source, target))
                            File.Copy(source, target, true);
                        paths[i] = target;
                    }
                    AssetDatabase.StopAssetEditing();
                    foreach (string path in paths)
                    {
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                        ConfigureImporter(path);
                    }
                    AssetDatabase.StartAssetEditing();
                    Sprite[] sprites = paths.Select(AssetDatabase.LoadAssetAtPath<Sprite>).ToArray();
                    if (sprites.Any(sprite => sprite == null))
                        throw new InvalidOperationException($"Sprite import failed: {folder}");
                    clips.Add(CreateOrUpdateClip(spec, sprites));
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            LayeredProjectilePresentationProfileSO profile =
                AssetDatabase.LoadAssetAtPath<LayeredProjectilePresentationProfileSO>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<LayeredProjectilePresentationProfileSO>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }
            var entries = new ProjectilePresentationLayerEntry[Specs.Length];
            for (int i = 0; i < Specs.Length; i++)
            {
                LayerSpec spec = Specs[i];
                entries[i] = new ProjectilePresentationLayerEntry();
                entries[i].ApplyEditorData(spec.canonical, spec.role, spec.required, clips[i],
                    spec.start, spec.lifetime, spec.loop,
                    spec.role == ProjectilePresentationLayerRole.GroundField
                        ? global::Skill.SkillSortingRelation.BelowOwner
                        : global::Skill.SkillSortingRelation.AboveOwner,
                    spec.order);
            }
            profile.ApplyEditorData(ProfileId, entries, .25f, .8f, 1.1f, 1.5f);
            profile.name = ProfileId;
            EditorUtility.SetDirty(profile);

            BaseVisualSO visual = AssetDatabase.LoadAssetAtPath<BaseVisualSO>(VisualPath);
            if (visual == null) throw new InvalidOperationException($"Missing BaseVisualSO: {VisualPath}");
            visual.ApplyLayeredPresentationProfileEditor(profile);
            EditorUtility.SetDirty(visual);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(ProfilePath, ImportAssetOptions.ForceSynchronousImport);
            profile = AssetDatabase.LoadAssetAtPath<LayeredProjectilePresentationProfileSO>(ProfilePath);
            visual = AssetDatabase.LoadAssetAtPath<BaseVisualSO>(VisualPath);
            if (profile == null || visual == null || visual.LayeredPresentationProfile != profile ||
                !profile.HasRequiredGroundField() || Math.Abs(profile.ResolveMaximumLayerEnd() - 1.5f) > .0001f)
                throw new InvalidOperationException("Persisted layered Dung graph failed readback.");
            Debug.Log("[JANGDAN-DUNG-LAYERED-INSTALL-V1] exact17/profile/visual graph materialized.");
        }

        private static AnimationClip CreateOrUpdateClip(LayerSpec spec, Sprite[] sprites)
        {
            string path = $"{ClipRoot}/{SkillId}.layered.{spec.canonical}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip { frameRate = 60f, name = Path.GetFileNameWithoutExtension(path) };
                AssetDatabase.CreateAsset(clip, path);
            }
            var keys = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = spec.keys[i], value = sprites[i] };
            EditorCurveBinding binding = new()
                { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), "m_FlipX"),
                AnimationCurve.Constant(0f, spec.stop, 0f));
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.startTime = 0f;
            settings.stopTime = spec.stop;
            settings.loopTime = spec.loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void ValidateSource()
        {
            string manifest = Path.GetFullPath($"{SourceRoot}/manifest.json");
            if (!File.Exists(manifest) || Sha256(manifest) != ExpectedManifestSha)
                throw new InvalidOperationException("Dung exact17 manifest SHA mismatch.");
            foreach (LayerSpec spec in Specs)
                for (int i = 0; i < spec.count; i++)
                    if (!File.Exists(Path.GetFullPath(
                            $"{SourceRoot}/{spec.source}/frame-{i:00}.png")))
                        throw new InvalidOperationException(
                            $"Incomplete Dung source: {spec.source}/frame-{i:00}.png");
        }

        private static void ConfigureImporter(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"Missing importer: {path}");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.spritePivot = new Vector2(.5f, .5f);
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static bool BytesEqual(string a, string b) =>
            File.ReadAllBytes(a).SequenceEqual(File.ReadAllBytes(b));

        private static string Sha256(string path)
        {
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(File.ReadAllBytes(path)).Select(x => x.ToString("x2")));
        }

        private static void EnsureFolder(string path)
        {
            string current = "Assets";
            foreach (string part in path.Substring("Assets/".Length).Split('/'))
            {
                string next = $"{current}/{part}";
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, part);
                current = next;
            }
        }
    }
}
