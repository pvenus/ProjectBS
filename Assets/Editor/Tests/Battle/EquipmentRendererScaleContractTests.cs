using System.IO;
using System.Reflection;
using NUnit.Framework;
using ResourceTools.Skill;
using Skill;
using UnityEngine;

public sealed class EquipmentRendererScaleContractTests
{
    [System.Serializable]
    private sealed class ExactSkillContract
    {
        public ExactCastContract cast;
        public ExactHitContract[] hits;
    }

    [System.Serializable]
    private sealed class ExactCastContract
    {
        public float cooldown;
        public float range;
        public ExactCastMoveContract castMove;
        public ExactEffectEntryContract[] selfEffects;
    }

    [System.Serializable]
    private sealed class ExactCastMoveContract
    {
        public float distance;
    }

    [System.Serializable]
    private sealed class ExactHitContract
    {
        public ExactDamageContract damage;
        public ExactEffectEntryContract[] debuffEffects;
    }

    [System.Serializable]
    private sealed class ExactDamageContract
    {
        public float attackPercentDamage;
        public bool canCritical;
    }

    [System.Serializable]
    private sealed class ExactEffectEntryContract
    {
        public ExactEffectContract effect;
        public float duration;
    }

    [System.Serializable]
    private sealed class ExactEffectContract
    {
        public float force;
        public float value;
        public float duration;
    }

    private static readonly string[] SeojinExact6JsonPaths =
    {
        "Assets/Contents/Skill/json/skill.character.seojin.1.basic_attack.basic_attack.json",
        "Assets/Contents/Skill/json/skill.character.seojin.2.basic_attack.basic_attack.json",
        "Assets/Contents/Skill/json/skill.character.seojin.3.basic_attack.basic_attack.json",
        "Assets/Contents/Skill/json/skill.character.seojin.1.active_1.active_1.json",
        "Assets/Contents/Skill/json/skill.character.seojin.2.active_1.charge.json",
        "Assets/Contents/Skill/json/skill.character.seojin.3.active_1.charge.json"
    };

    [TestCase(0f, 1f)]
    [TestCase(-1f, 1f)]
    [TestCase(float.NaN, 1f)]
    [TestCase(float.PositiveInfinity, 1f)]
    [TestCase(.75f, .75f)]
    [TestCase(2f, 2f)]
    public void RendererScale_NormalizesLegacyAndInvalidValues(float input, float expected)
    {
        Assert.That(EquipmentBaseProfileSO.NormalizeRendererScale(input),
            Is.EqualTo(expected));
    }

    [Test]
    public void BaseProfileJson_MissingRendererScaleDefaultsToOne()
    {
        const string json = "{\"baseProfileId\":\"test\"}";
        BaseProfileJson parsed = JsonUtility.FromJson<BaseProfileJson>(json);
        Assert.That(parsed.rendererScale, Is.EqualTo(1f));
    }

    [Test]
    public void ProjectileVisual_AppliesScaleOnlyToRendererBaselineWithoutAccumulation()
    {
        GameObject root = new("ProjectileRoot");
        GameObject scaler = new("Scaler");
        GameObject rendererObject = new("Renderer");
        try
        {
            scaler.transform.SetParent(root.transform, false);
            rendererObject.transform.SetParent(scaler.transform, false);
            SpriteRenderer renderer = rendererObject.AddComponent<SpriteRenderer>();
            rendererObject.transform.localScale = new Vector3(2f, 3f, 1f);
            BoxCollider2D collider = scaler.AddComponent<BoxCollider2D>();
            Vector3 rootBaseline = root.transform.localScale;
            Vector3 scalerBaseline = scaler.transform.localScale;
            Vector2 colliderBaseline = collider.size;

            ProjectileVisual visual = root.AddComponent<ProjectileVisual>();
            MethodInfo apply = typeof(ProjectileVisual).GetMethod(
                "ApplyRendererScale", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo restore = typeof(ProjectileVisual).GetMethod(
                "RestoreRendererScale", BindingFlags.Instance | BindingFlags.NonPublic);

            apply.Invoke(visual, new object[] { 1.5f });
            Assert.That(renderer.transform.localScale, Is.EqualTo(new Vector3(3f, 4.5f, 1.5f)));
            apply.Invoke(visual, new object[] { .5f });
            Assert.That(renderer.transform.localScale, Is.EqualTo(new Vector3(1f, 1.5f, .5f)));
            Assert.That(root.transform.localScale, Is.EqualTo(rootBaseline));
            Assert.That(scaler.transform.localScale, Is.EqualTo(scalerBaseline));
            Assert.That(collider.size, Is.EqualTo(colliderBaseline));

            restore.Invoke(visual, null);
            Assert.That(renderer.transform.localScale, Is.EqualTo(new Vector3(2f, 3f, 1f)));
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void RuntimePropagation_IsExplicitAndDoesNotReuseProjectileScale()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string resolver = File.ReadAllText(Path.Combine(projectRoot,
            "Assets/Scripts/Ability/Skills/Services/EquipmentSkillResolver.cs"));
        string runtime = File.ReadAllText(Path.Combine(projectRoot,
            "Assets/Scripts/Ability/Skills/Runtime/ProjectileRuntimeData.cs"));
        string visual = File.ReadAllText(Path.Combine(projectRoot,
            "Assets/Scripts/Ability/Skills/Projectiles/ProjectileVisual.cs"));

        Assert.That(runtime, Does.Contain("public float rendererScale = 1f;"));
        Assert.That(resolver, Does.Contain("rendererScale = EquipmentBaseProfileSO.NormalizeRendererScale"));
        Assert.That(visual, Does.Contain("baselineRendererLocalScale * effectiveScale"));
        Assert.That(visual, Does.Not.Contain("transform.localScale = baselineRendererLocalScale"));
    }

    [TestCase(0, .8f, .8f, .2f, .6f, .35f, "Linear", 0f)]
    [TestCase(1, 1f, .8f, .2f, .6f, .4f, "Linear", 0f)]
    [TestCase(2, 1.2f, .8f, .2f, .6f, .45f, "Linear", 0f)]
    [TestCase(3, .8f, 1.5f, .3f, 1f, 0f, "Hover", 0f)]
    [TestCase(4, 1f, 1.5f, .3f, 1f, 0f, "Hover", 0f)]
    [TestCase(5, 1.2f, 1.5f, .3f, 1f, 0f, "Hover", 0f)]
    public void SeojinExact6_JsonRoundTripsG1SemanticsAndVisualScaleOnly(
        int index,
        float projectileScale,
        float rendererScale,
        float lifetime,
        float colliderRadius,
        float spawnOffset,
        string moveType,
        float rotationOffset)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string json = File.ReadAllText(Path.Combine(projectRoot, SeojinExact6JsonPaths[index]));
        MethodInfo parseRoot = typeof(EquipmentSkillJsonGenerator).GetMethod(
            "ParseEquipmentSkillJson", BindingFlags.Static | BindingFlags.NonPublic);
        MethodInfo parseProfile = typeof(EquipmentSkillJsonGenerator).GetMethod(
            "ParseBaseProfile", BindingFlags.Static | BindingFlags.NonPublic);

        EquipmentSkillJsonGenerator.EquipmentSkillJson root =
            (EquipmentSkillJsonGenerator.EquipmentSkillJson)parseRoot.Invoke(null, new object[] { json });
        BaseProfileJson profile = (BaseProfileJson)parseProfile.Invoke(
            null, new object[] { root.baseProfile });
        MoveJson move = JsonUtility.FromJson<MoveJson>(root.move);
        ProjectileArrangementJson arrangement =
            JsonUtility.FromJson<ProjectileArrangementJson>(profile.projectile);
        ProjectileSpawnJson spawn = JsonUtility.FromJson<ProjectileSpawnJson>(profile.projectileSpawn);
        BattleSkillBrainMetaJson brain =
            JsonUtility.FromJson<BattleSkillBrainMetaJson>(profile.brainMeta);

        Assert.That(profile.projectileCount, Is.EqualTo(1));
        Assert.That(profile.projectileScale, Is.EqualTo(projectileScale));
        Assert.That(profile.rendererScale, Is.EqualTo(rendererScale));
        Assert.That(profile.projectileLifetime, Is.EqualTo(lifetime));
        Assert.That(profile.projectileColliderRadius, Is.EqualTo(colliderRadius));
        Assert.That(profile.skillComponentType, Is.EqualTo("Projectile"));
        Assert.That(arrangement.arrangement, Is.EqualTo("Spread"));
        Assert.That(arrangement.arrangementValue, Is.Zero);
        Assert.That(arrangement.spreadAngle, Is.Zero);
        Assert.That(arrangement.radius, Is.Zero);
        Assert.That(spawn.spawnOffset, Is.EqualTo(spawnOffset));
        Assert.That(spawn.interval, Is.Zero);
        Assert.That(brain.category, Is.EqualTo("Attack"));
        Assert.That(brain.targetType, Is.EqualTo("Enemy"));
        Assert.That(brain.tacticalNeed,
            Is.EqualTo(index < 3 ? "None" : "OffensivePressure"));
        Assert.That(brain.basePriority, Is.EqualTo(index < 3 ? 0f : .8f));
        Assert.That(move.moveType, Is.EqualTo(moveType));
        Assert.That(move.applyDirectionRotation, Is.True);
        Assert.That(move.rotationOffset, Is.EqualTo(rotationOffset));
        if (index < 3)
        {
            Assert.That(root.baseProfile, Does.Not.Contain("attackArchetype"));
        }
        else
        {
            Assert.That(root.baseProfile, Does.Contain("\"attackArchetype\": \"Melee\""));
        }
        Assert.That(root.baseProfile, Does.Not.Contain("projectilePrefabName"));
    }

    [TestCase(0, 3f, .6f, 0f, 0f, 0f, 1f, true)]
    [TestCase(1, 2.5f, .7f, 0f, 0f, 0f, 1f, true)]
    [TestCase(2, 2f, .8f, 0f, 0f, 0f, 1f, true)]
    [TestCase(3, 10f, 2f, 2f, 15f, 3f, 1.8f, true)]
    [TestCase(4, 9f, 2f, 2.5f, 17f, 4f, 1.8f, true)]
    [TestCase(5, 8f, 2f, 3f, 20f, 5f, 1.8f, true)]
    public void SeojinExact6_BaseCastHitValuesAreExplicit(
        int index,
        float cooldown,
        float range,
        float dashDistance,
        float knockbackForce,
        float buffDuration,
        float attackPercentDamage,
        bool canCritical)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string json = File.ReadAllText(Path.Combine(projectRoot, SeojinExact6JsonPaths[index]));
        ExactSkillContract skill = JsonUtility.FromJson<ExactSkillContract>(json);
        ExactHitContract hit = skill.hits[0];

        Assert.That(skill.cast.cooldown, Is.EqualTo(cooldown));
        Assert.That(skill.cast.range, Is.EqualTo(range));
        Assert.That(hit.damage.attackPercentDamage, Is.EqualTo(attackPercentDamage));
        Assert.That(hit.damage.canCritical, Is.EqualTo(canCritical));

        if (index >= 3)
        {
            Assert.That(skill.cast.castMove.distance, Is.EqualTo(dashDistance));
            Assert.That(hit.debuffEffects[0].effect.force, Is.EqualTo(knockbackForce));
            Assert.That(skill.cast.selfEffects[0].effect.value, Is.EqualTo(20f));
            Assert.That(skill.cast.selfEffects[0].effect.duration, Is.EqualTo(buffDuration));
            Assert.That(skill.cast.selfEffects[0].duration, Is.EqualTo(buffDuration));
        }
    }
}
