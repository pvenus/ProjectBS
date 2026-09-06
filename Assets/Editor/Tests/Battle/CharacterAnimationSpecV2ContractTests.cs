#if UNITY_EDITOR
using System.IO;
using Character;
using NUnit.Framework;
using UnityEngine;

public sealed class CharacterAnimationSpecV2ContractTests
{
    [Test]
    public void SeojinG1UsesOneCanonicalAnimationRootAndGeneratorPreservesLegacyOnPreparationFailure()
    {
        const string imageRoot =
            "Assets/ImagesGenerated/Character/animation/character.seojin.1";
        const string clipRoot =
            "Assets/AnimationClips/Character/character.seojin.1";
        string generator = File.ReadAllText(
            "Assets/Editor/tools/character/CharacterJsonGenerator.cs");
        string materializer = File.ReadAllText(
            "Assets/Editor/tools/character/SeojinG1AnimationV2PilotMaterializer.cs");

        Assert.That(Directory.Exists(imageRoot), Is.True);
        Assert.That(Directory.Exists(clipRoot), Is.True);
        Assert.That(File.Exists($"{imageRoot}/basic_attack/frame-17.png"), Is.True);
        Assert.That(File.Exists(
            $"{clipRoot}/character.seojin.1.basic_attack.combo.continuous18.body.anim"), Is.True);
        Assert.That(materializer, Does.Contain("LoadOrInstallAcceptedFrames"));
        Assert.That(materializer, Does.Contain("completeCanonicalSet"));
        Assert.That(generator, Does.Contain("characterSo.AnimationClips.ToList()"));
        Assert.That(generator, Does.Contain("legacy clips remain available"));
        Assert.That(generator, Does.Contain("basic_attack.combo.continuous18.body"));
        Assert.That(generator, Does.Contain("death.c.user-manual"));
    }

    [Test]
    public void Priority_IsDeathThenCcThenSkillThenBasicThenMovement()
    {
        Assert.AreEqual(CharacterAnimationResolver.Priority.Death,
            CharacterAnimationResolver.ResolvePriority(true, true, true, true, true));
        Assert.AreEqual(CharacterAnimationResolver.Priority.AttackDisabledCc,
            CharacterAnimationResolver.ResolvePriority(false, true, true, true, true));
        Assert.AreEqual(CharacterAnimationResolver.Priority.SkillAction,
            CharacterAnimationResolver.ResolvePriority(false, false, true, true, true));
        Assert.AreEqual(CharacterAnimationResolver.Priority.BasicAttack,
            CharacterAnimationResolver.ResolvePriority(false, false, false, true, true));
        Assert.AreEqual(CharacterAnimationResolver.Priority.Move,
            CharacterAnimationResolver.ResolvePriority(false, false, false, false, true));
    }

    [Test]
    public void SkillResolution_PrefersPersistedCastClip_AndSupportsExplicitRendererlessFallback()
    {
        AnimationClip castClip = new AnimationClip();
        AnimationClip resolved = CharacterAnimationResolver.ResolveSkillClip(
            null, "skill.test", castClip, out CharacterAnimationFallbackPolicy fallback);
        Assert.AreSame(castClip, resolved);
        Assert.AreEqual(CharacterAnimationFallbackPolicy.LegacyDirectional, fallback);
        Object.DestroyImmediate(castClip);
    }

    [Test]
    public void Contract_IsVersionedAndMaterializerNeverWritesYaml()
    {
        string root = Directory.GetCurrentDirectory();
        string builder = File.ReadAllText(Path.Combine(root,
            "Assets/Editor/tools/character/CharacterAnimationProfileAssetBuilder.cs"));
        string schema = File.ReadAllText(Path.Combine(root,
            "Assets/Contents/Character/animation/character-animation-profile-v2.schema.json"));
        Assert.That(schema, Does.Contain("\"schemaVersion\": { \"const\": 2 }"));
        Assert.That(schema, Does.Contain("fallbackReceipt"));
        Assert.That(builder, Does.Contain("AssetDatabase.CreateAsset"));
        Assert.That(builder, Does.Contain("ForceSynchronousImport"));
        Assert.That(builder, Does.Not.Contain("File.WriteAllText"));
    }

    [Test]
    public void SeojinProjection_CompletesG1WithoutInventingMissingGradeSkills()
    {
        string report = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),
            "Assets/Contents/Character/animation/seojin-v2-validation.json"));
        Assert.That(report, Does.Contain("P4_SEOJIN_G1_CONTENT_COMPLETE_STATIC_PASS"));
        Assert.That(report, Does.Contain("\"characterId\": \"character.seojin.1\""));
        Assert.That(report, Does.Contain("\"missingSelections\": []"));
        Assert.That(report, Does.Contain("\"pendingPhysical\": []"));
        Assert.That(report, Does.Contain("G1 Active2/Active3 were not invented"));
        Assert.That(report, Does.Contain("Death"));
        Assert.That(report, Does.Contain("AttackDisabledCc"));
    }

    [Test]
    public void MigrationDryRun_HasNoAssetMutationCalls()
    {
        string source = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),
            "Assets/Editor/tools/character/CharacterAnimationMigrationDryRun.cs"));
        Assert.That(source, Does.Contain("DRY_RUN_NO_ASSET_MUTATION"));
        Assert.That(source, Does.Not.Contain("CreateAsset("));
        Assert.That(source, Does.Not.Contain("SetDirty("));
        Assert.That(source, Does.Not.Contain("SaveAssets("));
        Assert.That(source, Does.Not.Contain("ImportAsset("));
    }

    [Test]
    public void MigrationTransaction_IsSingleCharacterValidatedAndRollbackSafe()
    {
        string source = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),
            "Assets/Editor/tools/character/CharacterAnimationMigrationTransaction.cs"));
        Assert.That(source, Does.Contain("Validate(character, candidate)"));
        Assert.That(source, Does.Contain("CharacterAnimationProfileSO previous"));
        Assert.That(source, Does.Contain("Persisted CharacterSO profile readback mismatch"));
        Assert.That(source, Does.Not.Contain("FindAssets("));
    }

    [Test]
    public void CharacterGeneration_IntegratesV2Materialization_WithLegacyFailover()
    {
        string root = Directory.GetCurrentDirectory();
        string generator = File.ReadAllText(Path.Combine(root,
            "Assets/Editor/tools/character/CharacterJsonGenerator.cs"));
        string pilot = File.ReadAllText(Path.Combine(root,
            "Assets/Editor/tools/character/SeojinG1AnimationV2PilotMaterializer.cs"));

        int saveCharacter = generator.IndexOf("AssetDatabase.SaveAssets();", System.StringComparison.Ordinal);
        int generateProfile = generator.IndexOf("TryGenerateAnimationProfile(characterSo", System.StringComparison.Ordinal);
        Assert.That(saveCharacter, Is.GreaterThanOrEqualTo(0));
        Assert.That(generateProfile, Is.GreaterThan(saveCharacter),
            "CharacterSO must be persisted before optional v2 profile promotion.");
        Assert.That(generator, Does.Contain("TryPrepareContent(out string prepareError)"));
        Assert.That(generator, Does.Contain("legacy CharacterSO generation remains active"));
        Assert.That(generator, Does.Contain("catch (Exception exception)"));
        Assert.That(generator, Does.Contain("TryPromote(characterSo, candidate"));
        Assert.That(pilot, Does.Contain("public static bool TryPrepareContent(out string error)"));
        Assert.That(pilot, Does.Contain("AssetDatabase.DeleteAsset(created[i])"));
    }
}
#endif
