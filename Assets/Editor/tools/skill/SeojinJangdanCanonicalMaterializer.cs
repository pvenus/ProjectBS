#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Character;
using ResourceTools;
using ResourceTools.Skill;
using Skill;
using UnityEditor;
using UnityEngine;

public static class SeojinJangdanCanonicalMaterializer
{
    private const string ManifestPath =
        "Artifacts/GraphicsRemediation/SkillAnimation/SeojinJangdanSkillset/selected/revision-01/manifest.json";
    private const string ManifestSha =
        "bef9a55d9f8de7e63ae8f7b72748c6097511d191e6fbe50361bccb793bf569ea";
    private static readonly string[] SkillIds =
    {
        "skill.character.seojin.1.active_5.jangdan_dung",
        "skill.character.seojin.1.active_6.jangdan_gi",
        "skill.character.seojin.1.active_7.jangdan_deok",
        "skill.character.seojin.1.active_8.deoreoreoreo"
    };

    [MenuItem("Tools/ProjectBS/Jangdan/Materialize Seojin G1 Complete76")]
    public static void Materialize()
    {
        ValidatePhysicalUnit();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        EquipmentSkillSO[] skills = SkillIds
            .Select(id => EquipmentSkillJsonGenerator.GenerateFromJsonPath(
                $"Assets/Contents/Skill/json/{id}.json"))
            .ToArray();
        if (skills.Any(skill => skill == null || skill.CastSo == null ||
                                skill.BaseVisualSo == null || skill.Icon == null))
            throw new InvalidOperationException("Jangdan exact4 materialization is incomplete; CharacterSO was not changed.");

        CharacterSO character = ResourceTools.Character.CharacterJsonGenerator.GenerateFromJsonPath(
            "Assets/Contents/Character/json/character.seojin.1.json");
        if (character == null)
            throw new InvalidOperationException("Jangdan CharacterSO materialization failed.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("[SeojinJangdanCanonicalMaterializer] COMPLETE76 exact4 materialization PASS.");
    }

    private static void ValidatePhysicalUnit()
    {
        string absoluteManifest = Path.GetFullPath(ManifestPath);
        if (!File.Exists(absoluteManifest) || !string.Equals(Sha256(absoluteManifest), ManifestSha, StringComparison.Ordinal))
            throw new InvalidOperationException("Jangdan corrected manifest is missing or has changed.");

        int icons = Directory.GetFiles("Assets/ImagesGenerated/Skill/icon",
                "skill.character.seojin.1.active_*.icon.png", SearchOption.TopDirectoryOnly)
            .Count(path => SkillIds.Any(id => path.EndsWith(id + ".icon.png", StringComparison.Ordinal)));
        int body = Directory.GetFiles(
            "Assets/ImagesGenerated/Character/animation/character.seojin.1/jangdan",
            "*.png", SearchOption.AllDirectories).Length;
        int vfx = SkillIds.Sum(id => Directory.Exists($"Assets/ImagesGenerated/Skill/animation/{id}")
            ? Directory.GetFiles($"Assets/ImagesGenerated/Skill/animation/{id}", "*.png").Length : 0);
        if (icons != 4 || body != 36 || vfx != 36)
            throw new InvalidOperationException($"Jangdan COMPLETE76 required; found icon/body/vfx={icons}/{body}/{vfx}.");
    }

    private static string Sha256(string path)
    {
        using SHA256 sha = SHA256.Create();
        using FileStream stream = File.OpenRead(path);
        return string.Concat(sha.ComputeHash(stream).Select(value => value.ToString("x2")));
    }
}
#endif
