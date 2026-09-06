#if UNITY_EDITOR
using System;
using System.Linq;
using Character;
using UnityEditor;

namespace ResourceTools.Character
{
    /// <summary>One-character promotion boundary. Never performs bulk discovery or migration.</summary>
    public static class CharacterAnimationMigrationTransaction
    {
        public static bool TryPromote(CharacterSO character, CharacterAnimationProfileSO candidate,
            out string error)
        {
            error = null;
            if (character == null || candidate == null)
            {
                error = "Character and candidate profile are required.";
                return false;
            }
            CharacterAnimationProfileValidator.Issue[] issues =
                CharacterAnimationProfileValidator.Validate(character, candidate).ToArray();
            if (issues.Any(x => x.severity == "BLOCKER" || x.severity == "ERROR"))
            {
                error = string.Join("; ", issues.Where(x => x.severity != "WARN")
                    .Select(x => $"{x.code}:{x.subjectId}"));
                return false;
            }

            string characterPath = AssetDatabase.GetAssetPath(character);
            CharacterAnimationProfileSO previous = character.AnimationProfile;
            try
            {
                Undo.RecordObject(character, "Promote Character Animation Profile v2");
                character.ApplyEditorAnimationProfile(candidate);
                EditorUtility.SetDirty(character);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(characterPath, ImportAssetOptions.ForceSynchronousImport);
                CharacterSO persisted = AssetDatabase.LoadAssetAtPath<CharacterSO>(characterPath);
                if (persisted == null || persisted.AnimationProfile != candidate)
                    throw new InvalidOperationException("Persisted CharacterSO profile readback mismatch.");
                return true;
            }
            catch (Exception exception)
            {
                character.ApplyEditorAnimationProfile(previous);
                EditorUtility.SetDirty(character);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(characterPath, ImportAssetOptions.ForceSynchronousImport);
                error = exception.Message;
                return false;
            }
        }
    }
}
#endif
