using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class PlayerCharacterAuraLoopContractTests
{
    private const string Root =
        "Assets/ImagesGenerated/Battle/character/player-character-aura-loop-f";

    [Test]
    public void SelectedLoopIsExactSixPairedFramesWithExpectedImporter()
    {
        var guids = new HashSet<string>();
        AssertValidGuidMeta("Assets/ImagesGenerated/Battle/character/player-character-aura-loop-f.meta", guids);
        AssertValidGuidMeta($"{Root}/back.meta", guids);
        AssertValidGuidMeta($"{Root}/front.meta", guids);
        for (int frame = 1; frame <= 6; frame++)
        {
            string name = $"frame_{frame:00}.png";
            Assert.That(File.Exists($"{Root}/back/{name}"), Is.True);
            Assert.That(File.Exists($"{Root}/front/{name}"), Is.True);

            string backMeta = File.ReadAllText($"{Root}/back/{name}.meta");
            string frontMeta = File.ReadAllText($"{Root}/front/{name}.meta");
            AssertImporter(backMeta);
            AssertImporter(frontMeta);
            AssertValidGuidMeta($"{Root}/back/{name}.meta", guids);
            AssertValidGuidMeta($"{Root}/front/{name}.meta", guids);
            AssertTexture($"{Root}/back/{name}");
            AssertTexture($"{Root}/front/{name}");
        }
    }

    private static void AssertValidGuidMeta(string path, HashSet<string> guids)
    {
        string meta = File.ReadAllText(path);
        Match match = Regex.Match(meta, @"(?m)^guid: ([0-9a-f]{32})$");
        Assert.That(match.Success, Is.True, $"invalid Unity GUID: {path}");
        string guid = match.Groups[1].Value;
        Assert.That(guids.Add(guid), Is.True, $"duplicate aura GUID: {guid}");
        Assert.That(AssetDatabase.GUIDToAssetPath(guid), Is.Not.Empty,
            $"unresolved aura GUID: {guid}");
    }

    [Test]
    public void PrefabBindsExactSixFramesAtPointOneSixSecondLoopCadence()
    {
        string prefab = File.ReadAllText(
            "Assets/Resources/battle/Presentation/PlayerCharacterAura.prefab");

        Assert.That(Count(prefab, "backLoopFrames:"), Is.EqualTo(1));
        Assert.That(Count(prefab, "frontLoopFrames:"), Is.EqualTo(1));
        Assert.That(Count(prefab, "fileID: 21300000"), Is.EqualTo(14));
        Assert.That(prefab, Does.Contain("loopFrameDuration: 0.16"));
        Assert.That(prefab, Does.Not.Contain("fed8909b1f1e47b5bf71a383a2a50870"));
        Assert.That(prefab, Does.Not.Contain("8d999b956ebb4c87b15aae1b7612c6d8"));
        Assert.That(prefab, Does.Contain("m_Sprite: {fileID: 21300000, guid: 0b6651605802d09810aa12f5fb35b978, type: 3}"));
        Assert.That(prefab, Does.Contain("m_Sprite: {fileID: 21300000, guid: 641d97593183c1cb1fb25816b1431343, type: 3}"));
        Assert.That(prefab, Does.Contain("visualScale: {x: 0.2058, y: 0.15435}"));
        Assert.That(prefab, Does.Contain("positionOffset: {x: 0, y: -0.25, z: 0}"));
        Assert.That(Count(prefab, "m_LocalPosition: {x: 0, y: -0.25, z: 0}"), Is.EqualTo(2));
        Assert.That(Count(prefab, "m_LocalScale: {x: 0.2058, y: 0.15435, z: 1}"), Is.EqualTo(2));
    }

    [Test]
    public void RuntimeRestartsAtFrameZeroAndFailsBackToStaticPair()
    {
        string source = File.ReadAllText(
            "Assets/Scripts/Battle/Presentation/CharacterAura/BattleCharacterAuraView.cs");

        Assert.That(source, Does.Contain("private void OnEnable()"));
        Assert.That(source, Does.Contain("RestartSelectionLoop();"));
        Assert.That(source, Does.Contain("ApplySelectionLoopFrame(0);"));
        Assert.That(source, Does.Contain("backLoopFrames.Length != 6"));
        Assert.That(source, Does.Contain("frontLoopFrames.Length != 6"));
        Assert.That(source, Does.Contain("ApplyStaticFallbackSprites();"));
        Assert.That(source, Does.Contain("Time.unscaledDeltaTime"));
        Assert.That(source, Does.Contain("public void SetSelectionActive(bool selected)"));
        Assert.That(source, Does.Contain("SetRendererVisibility(false);"));
        Assert.That(source, Does.Not.Contain("SkillAnimation"));

        string installer = File.ReadAllText(
            "Assets/Scripts/Battle/Presentation/CharacterAura/BattleCharacterAuraInstaller.cs");
        Assert.That(installer, Does.Contain("auraView.SetSelectionActive(true);"));
        Assert.That(installer, Does.Contain("auraView.HasAuthoredSelectionLoop"));
        Assert.That(installer, Does.Contain("auraView.SetColor(Color.white);"));
    }

    [Test]
    public void SeojinDirectionHudDoublesOnlyItsPresentationFootprint()
    {
        string source = File.ReadAllText(
            "Assets/Scripts/Actor/Character/Control/SeojinDualDirectionHud.cs");

        Assert.That(source, Does.Contain("internal const float FootprintScale=2f;"));
        Assert.That(source, Does.Contain("CreateArc(\"MouseRed\",redSprite,1.15f"));
        Assert.That(source, Does.Contain("CreateArc(\"WasdBlue\",blueSprite,1f"));
        Assert.That(Count(source, "*FootprintScale/5.12f"), Is.EqualTo(2));
        Assert.That(source, Does.Not.Contain("actor.transform.localScale"));
        Assert.That(source, Does.Not.Contain("aura.VisualScale="));
    }

    private static void AssertImporter(string meta)
    {
        Assert.That(meta, Does.Contain("spritePixelsToUnits: 100"));
        Assert.That(meta, Does.Contain("spritePivot: {x: 0.5, y: 0.5}"));
        Assert.That(meta, Does.Contain("filterMode: 1"));
        Assert.That(meta, Does.Contain("wrapU: 1"));
        Assert.That(meta, Does.Contain("alphaIsTransparency: 1"));
    }

    private static void AssertTexture(string path)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        Assert.That(texture, Is.Not.Null, path);
        Assert.That(texture.width, Is.EqualTo(249), path);
        Assert.That(texture.height, Is.EqualTo(144), path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        Assert.That(importer, Is.Not.Null, path);
        Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(100f), path);
        Assert.That(importer.spritePivot, Is.EqualTo(new Vector2(.5f, .5f)), path);
        Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear), path);
        Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp), path);
        Assert.That(importer.alphaIsTransparency, Is.True, path);
    }

    private static int Count(string source, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(value, index, System.StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
