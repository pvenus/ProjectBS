using System.IO;
using System.Reflection;
using Character;
using Character.Skill;
using NUnit.Framework;
using UnityEngine;

public sealed class SeojinActive4SwiftStepContractTests
{
    private const string Root = "Assets/Contents/Skill/json/";

    [TestCase(1, 11f, 2.0f, 12f, 10f, 3f, 5)]
    [TestCase(2, 10f, 2.5f, 15f, 12f, 3.5f, 10)]
    [TestCase(3, 9f, 3.0f, 18f, 15f, 4f, 15)]
    public void ExactGradeContract(
        int grade, float cooldown, float distance, float attackSpeed,
        float moveSpeed, float duration, int cap)
    {
        string text = File.ReadAllText($"{Root}skill.character.seojin.{grade}.active_4.swift_step.json");
        Assert.That(text, Does.Contain("\"slotName\": \"active_4\"").Or.Contain("\"slotName\":\"active_4\""));
        Assert.That(text, Does.Contain("\"skillComponentType\": \"Mobility\"").Or.Contain("\"skillComponentType\":\"Mobility\""));
        Assert.That(text, Does.Contain($"\"cooldown\": {cooldown}").Or.Contain($"\"cooldown\":{cooldown}"));
        Assert.That(text, Does.Contain($"\"distance\": {distance}").Or.Contain($"\"distance\":{distance}"));
        Assert.That(text, Does.Contain("\"anticipation\": 0.08").Or.Contain("\"anticipation\":0.08"));
        Assert.That(text, Does.Contain($"\"value\": {attackSpeed}").Or.Contain($"\"value\":{attackSpeed}"));
        Assert.That(text, Does.Contain($"\"value\": {moveSpeed}").Or.Contain($"\"value\":{moveSpeed}"));
        Assert.That(text, Does.Contain($"\"duration\": {duration}").Or.Contain($"\"duration\":{duration}"));
        Assert.That(text, Does.Contain($"\"level\": {cap}").Or.Contain($"\"level\":{cap}"));
        Assert.That(text, Does.Contain("\"hits\": []").Or.Contain("\"hits\":[]"));
    }

    [Test]
    public void RegistryAndMobilityPathAreAdditive()
    {
        string pool = File.ReadAllText("Assets/Scripts/Ability/Skills/Services/SkillPoolService.cs");
        string helper = File.ReadAllText("Assets/Scripts/Ability/Skills/Services/Helpers/SkillUseHelper.cs");
        string cast = File.ReadAllText("Assets/Scripts/Actor/Character/service/skill/CastMoveService.cs");
        Assert.That(pool, Does.Contain("SkillPoolSlotKeys.Active4"));
        Assert.That(helper, Does.Contain("case SkillComponentType.Mobility"));
        Assert.That(helper, Does.Contain("ApplyPostMoveSelfEffects"));
        Assert.That(cast, Does.Contain("CharacterMovementPriority"));
        Assert.That(cast, Does.Contain("MinSuccessDistance"));
        Assert.That(cast, Does.Contain("IsNonBlockingCastCollider"));
        Assert.That(cast, Does.Contain("hit.attachedRigidbody"));
        Assert.That(cast, Does.Contain("RigidbodyType2D.Static"));
        Assert.That(cast, Does.Contain("priority == CharacterMovementPriority.SwiftStep"));
        Assert.That(cast, Does.Contain("body.position = position"));
        Assert.That(cast, Does.Contain("body.MovePosition(position)"));
        Assert.That(cast, Does.Contain("TryAcquireExternalMovement("));
        Assert.That(cast, Does.Contain("priority == CharacterMovementPriority.SwiftStep"));
        Assert.That(cast, Does.Contain("ReleaseExternalMovement(this)"));
        Assert.That(cast, Does.Contain("ResolveCurrentPosition"));
        Assert.That(cast, Does.Contain("preparationFrameObserved"));
        Assert.That(cast, Does.Contain("remainingAnticipation"));

        string party = File.ReadAllText("Assets/Scripts/Actor/Party/PartyMovementMono.cs");
        Assert.That(party, Does.Contain("bool allowManualOverride = false"));
        Assert.That(party, Does.Contain("!allowManualOverride && HasRecentManualInput()"));
    }

    [Test]
    public void CastMoveCollisionClassificationSkipsActorsAndTriggersButKeepsWalls()
    {
        MethodInfo classify = typeof(CastMoveService).GetMethod(
            "IsNonBlockingCastCollider",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(classify, Is.Not.Null);

        GameObject caster = new GameObject("caster");
        GameObject enemy = new GameObject("enemy");
        GameObject enemyHurtbox = new GameObject("hurtbox");
        GameObject ally = new GameObject("ally");
        GameObject trigger = new GameObject("pickup-trigger");
        GameObject wall = new GameObject("wall");
        try
        {
            enemy.SetActive(false);
            enemy.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            enemy.AddComponent<CharacterManager>();
            enemyHurtbox.transform.SetParent(enemy.transform, false);
            Collider2D childEnemyCollider = enemyHurtbox.AddComponent<BoxCollider2D>();

            ally.SetActive(false);
            ally.AddComponent<CharacterManager>();
            Collider2D allyCollider = ally.AddComponent<BoxCollider2D>();

            Collider2D triggerCollider = trigger.AddComponent<CircleCollider2D>();
            triggerCollider.isTrigger = true;
            Collider2D wallCollider = wall.AddComponent<BoxCollider2D>();

            Assert.That(InvokeCollisionClassify(classify, childEnemyCollider, caster.transform), Is.True);
            Assert.That(InvokeCollisionClassify(classify, allyCollider, caster.transform), Is.True);
            Assert.That(InvokeCollisionClassify(classify, triggerCollider, caster.transform), Is.True);
            Assert.That(InvokeCollisionClassify(classify, wallCollider, caster.transform), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(caster);
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(ally);
            Object.DestroyImmediate(trigger);
            Object.DestroyImmediate(wall);
        }
    }

    private static bool InvokeCollisionClassify(
        MethodInfo method,
        Collider2D collider,
        Transform caster)
    {
        return (bool)method.Invoke(null, new object[] { collider, caster });
    }

    [TestCase(2.0f)]
    [TestCase(2.5f)]
    [TestCase(3.0f)]
    public void SwiftStepResolvedPositionProducesExactRootDelta(float distance)
    {
        MethodInfo apply = typeof(CastMoveService).GetMethod(
            "ApplyPosition",
            BindingFlags.Static | BindingFlags.NonPublic);
        MethodInfo read = typeof(CastMoveService).GetMethod(
            "ResolveCurrentPosition",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(apply, Is.Not.Null);
        Assert.That(read, Is.Not.Null);

        GameObject root = new GameObject("swift-step-root");
        try
        {
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.position = Vector2.zero;
            apply.Invoke(null, new object[]
            {
                body,
                root.transform,
                Vector2.right * distance,
                true
            });
            Vector2 terminal = (Vector2)read.Invoke(
                null,
                new object[] { body, root.transform });
            Assert.That(Vector2.Distance(Vector2.zero, terminal), Is.EqualTo(distance).Within(0.0001f));
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void SwiftStepMayOwnMovementDuringManualInputAndReleasesCleanly()
    {
        GameObject root = new GameObject("swift-step-party-owner");
        root.SetActive(false);
        try
        {
            PartyMovementMono party = root.AddComponent<PartyMovementMono>();
            object owner = new object();
            party.SetManualMoveInput(Vector2.right);
            Assert.That(party.TryAcquireExternalMovement(owner), Is.False);
            Assert.That(party.TryAcquireExternalMovement(owner, true), Is.True);
            party.ReleaseExternalMovement(owner);
            Assert.That(party.TryAcquireExternalMovement(new object(), true), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void Complete21UsesCodeDrivenBodyAndOneShotVfx()
    {
        for (int grade = 1; grade <= 3; grade++)
        {
            string text = File.ReadAllText($"{Root}skill.character.seojin.{grade}.active_4.swift_step.json");
            Assert.That(text, Does.Contain("baseVisual"));
            Assert.That(text, Does.Contain($"skill.character.seojin.{grade}.active_4.swift_step.visual"));
            Assert.That(text, Does.Contain($"presentation.character.seojin.{grade}.active_4.swift_step.body.v1"));
            Assert.That(text, Does.Not.Contain("mobilityBodyClipPath"));
        }

        string builder = File.ReadAllText("Assets/Editor/tools/skill/builder/SkillBaseVisualAssetBuilder.cs");
        Assert.That(builder, Does.Contain("SwiftStepDurations"));
        Assert.That(builder, Does.Contain("bool loopTime = !isSwiftStep"));

        string cast = File.ReadAllText("Assets/Scripts/Ability/Skills/Definitions/cast/SkillCastSO.cs");
        Assert.That(cast, Does.Contain("CharacterMobilityBodyPresentationController"));
        Assert.That(cast, Does.Contain("PreparationFrameObserved"));
        Assert.That(cast, Does.Contain("private const float Lifecycle = .42f"));
        Assert.That(cast, Does.Contain("CreateEcho"));
        Assert.That(cast, Does.Not.Contain("mobilityBodyClip"));
    }

    [Test]
    public void SwiftStepUsesFrameLatchedPointZeroEightCommitAndPointFourTwoRestore()
    {
        string[] expectedTimes =
        {
            "time: 0", "time: 0.08", "time: 0.13",
            "time: 0.2", "time: 0.28", "time: 0.36"
        };
        for (int grade = 1; grade <= 3; grade++)
        {
            string clip = File.ReadAllText(
                $"Assets/AnimationClips/Skill/skill.character.seojin.{grade}.active_4.swift_step.visual.loop.anim");
            int cursor = -1;
            for (int i = 0; i < expectedTimes.Length; i++)
            {
                int next = clip.IndexOf(expectedTimes[i], cursor + 1, System.StringComparison.Ordinal);
                Assert.That(next, Is.GreaterThan(cursor), $"G{grade} frame {i} timing/order");
                cursor = next;
            }
            Assert.That(clip, Does.Contain("time: 0.42"));
        }

        string move = File.ReadAllText(
            "Assets/Scripts/Actor/Character/service/skill/CastMoveService.cs");
        Assert.That(move, Does.Contain("yield return null"));
        Assert.That(move, Does.Contain("while (!preparationFrameObserved())"));
        Assert.That(move, Does.Contain("activeCommitted = true"));
        Assert.That(move, Does.Contain("CanContinueMovement(target)"));
        Assert.That(move.IndexOf("activeCommitted = true", System.StringComparison.Ordinal),
            Is.LessThan(move.IndexOf("onCommitted?.Invoke()", System.StringComparison.Ordinal)));

        string manager = File.ReadAllText(
            "Assets/Scripts/Actor/Character/CharacterSkillManager.cs");
        Assert.That(manager, Does.Contain("bodyPresenter.PreparationFrameObserved"));
        Assert.That(manager, Does.Contain("bodyPresenter.Restore()"));
    }

    [Test]
    public void RootRendererPresentationRestoreNeverRollsBackActorDisplacement()
    {
        GameObject root = new GameObject("swift-step-root-renderer");
        Texture2D texture = new Texture2D(2, 2);
        Sprite createdSprite = null;
        try
        {
            root.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            SpriteRenderer canonical = root.AddComponent<SpriteRenderer>();
            createdSprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f));
            canonical.sprite = createdSprite;
            canonical.color = new Color(.2f, .3f, .4f, .8f);
            global::Skill.CharacterMobilityBodyPresentationController presenter =
                root.AddComponent<global::Skill.CharacterMobilityBodyPresentationController>();

            InvokePrivate(presenter, "ResolvePrimary", assignField: "primary");
            InvokePrivate(presenter, "Snapshot");
            InvokePrivate(presenter, "PreparePresentationRenderer");
            root.transform.position = new Vector3(3f, 4f, 0f);
            presenter.Restore();

            Assert.That(root.transform.position, Is.EqualTo(new Vector3(3f, 4f, 0f)));
            Assert.That(canonical.enabled, Is.True);
            Assert.That(canonical.sprite, Is.SameAs(createdSprite));
            Assert.That(canonical.color, Is.EqualTo(new Color(.2f, .3f, .4f, .8f)));
            Assert.That(root.transform.Find("SwiftStepBodyProxy_RenderOnly"), Is.Not.Null);
        }
        finally
        {
            Object.DestroyImmediate(root);
            if (createdSprite != null) Object.DestroyImmediate(createdSprite);
            Object.DestroyImmediate(texture);
        }
    }

    [Test]
    public void RootRendererPresentationReusesOneProxyAndLeavesNoVisibleResidue()
    {
        GameObject root = new GameObject("swift-step-proxy-reuse");
        Texture2D texture = new Texture2D(2, 2);
        Sprite createdSprite = null;
        try
        {
            root.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            SpriteRenderer canonical = root.AddComponent<SpriteRenderer>();
            createdSprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f));
            canonical.sprite = createdSprite;
            global::Skill.CharacterMobilityBodyPresentationController presenter =
                root.AddComponent<global::Skill.CharacterMobilityBodyPresentationController>();

            for (int i = 0; i < 2; i++)
            {
                InvokePrivate(presenter, "ResolvePrimary", assignField: "primary");
                InvokePrivate(presenter, "Snapshot");
                InvokePrivate(presenter, "PreparePresentationRenderer");
                presenter.Restore();
            }

            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            int proxies = 0;
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i].name == "SwiftStepBodyProxy_RenderOnly") proxies++;
            Assert.That(proxies, Is.EqualTo(1));
            Transform proxy = root.transform.Find("SwiftStepBodyProxy_RenderOnly");
            Assert.That(proxy.GetComponent<SpriteRenderer>().enabled, Is.False);
            Assert.That(proxy.GetComponent<SpriteRenderer>().sprite, Is.Null);
        }
        finally
        {
            Object.DestroyImmediate(root);
            if (createdSprite != null) Object.DestroyImmediate(createdSprite);
            Object.DestroyImmediate(texture);
        }
    }

    private static object InvokePrivate(
        object target,
        string methodName,
        string assignField = null)
    {
        MethodInfo method = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        object result = method.Invoke(target, null);
        if (!string.IsNullOrEmpty(assignField))
        {
            FieldInfo field = target.GetType().GetField(
                assignField,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, result);
        }
        return result;
    }
}
