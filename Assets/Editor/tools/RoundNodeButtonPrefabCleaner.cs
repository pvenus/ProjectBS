#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Stage.EditorTools
{
    public static class RoundNodeButtonPrefabCleaner
    {
        [MenuItem("BS/Tools/Clean Up RoundNodeButton Prefabs")]
        public static void CleanUpPrefabs()
        {
            string[] prefabPaths = new string[]
            {
                "Assets/Resources/stage_new/RoundNodeButtonPrefab.prefab",
                "Assets/Prefabs/Temp/UINodeButton.prefab"
            };

            int cleanedCount = 0;

            foreach (string path in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                string assetPath = AssetDatabase.GetAssetPath(prefab);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

                Button buttonComp = prefabRoot.GetComponent<Button>();
                if (buttonComp != null)
                {
                    Object.DestroyImmediate(buttonComp, true);
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
                    cleanedCount++;
                    Debug.Log($"[RoundNodeButtonCleaner] Removed Button component from prefab at: {path}");
                }

                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[RoundNodeButtonCleaner] Cleaned up {cleanedCount} prefabs.");
        }
    }
}
#endif
