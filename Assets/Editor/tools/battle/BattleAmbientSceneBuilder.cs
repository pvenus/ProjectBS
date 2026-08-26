using Battle.Presentation.Ambient;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BattleAmbientSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/BattleScene.unity";
    private const string ImageRoot = "Assets/ImagesGenerated/Battle/ambient/animation/";
    private const string AutoBuildSessionKey = "BS.BattleAmbientSceneBuilder.AutoBuilt.v5";

    [InitializeOnLoadMethod]
    private static void QueueInitialBuild()
    {
        EditorApplication.delayCall += TryInitialBuild;
    }

    private static void TryInitialBuild()
    {
        if (SessionState.GetBool(AutoBuildSessionKey, false)
            || EditorApplication.isPlayingOrWillChangePlaymode
            || EditorApplication.isCompiling)
        {
            return;
        }

        Build();
        SessionState.SetBool(AutoBuildSessionKey, true);
    }

    [MenuItem("Tools/BS/Battle/Build Ambient Decorations")]
    public static void Build()
    {
        Sprite[] birdFlock = ImportFrames("bird_flock");
        Sprite[] windGust = ImportFrames("wind_gust");
        Sprite[] dryLeaves = ImportFrames("dry_leaves");
        Sprite[] grassTuft = ImportFrames("grass_tuft");

        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedForBuild = !scene.IsValid() || !scene.isLoaded;
        if (openedForBuild)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        Battle.BattleManager manager = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            manager = root.GetComponentInChildren<Battle.BattleManager>(true);
            if (manager != null)
            {
                break;
            }
        }

        if (manager == null)
        {
            Debug.LogError("[BattleAmbientSceneBuilder] BattleManager not found in BattleScene.");
            if (openedForBuild)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
            return;
        }

        BattleAmbientController controller = manager.GetComponent<BattleAmbientController>();
        if (controller == null)
        {
            controller = Undo.AddComponent<BattleAmbientController>(manager.gameObject);
        }

        Undo.RecordObject(controller, "Configure Battle Ambient Decorations");
        controller.Configure(birdFlock, windGust, dryLeaves, grassTuft);
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (openedForBuild)
        {
            EditorSceneManager.CloseScene(scene, true);
        }

        Debug.Log("[BattleAmbientSceneBuilder] Battle ambient sprites imported and BattleScene configured.");
    }

    private static Sprite ImportSprite(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"[BattleAmbientSceneBuilder] TextureImporter not found: {path}");
            return null;
        }

        bool changed = importer.textureType != TextureImporterType.Sprite
            || importer.spriteImportMode != SpriteImportMode.Single
            || !importer.alphaIsTransparency;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.spritePixelsPerUnit = 100f;
        if (changed)
        {
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite[] ImportFrames(string folderName)
    {
        Sprite[] frames = new Sprite[4];
        for (int index = 0; index < frames.Length; index++)
        {
            frames[index] = ImportSprite(
                $"{ImageRoot}{folderName}/frame_{index + 1:00}.png");
        }

        return frames;
    }
}
