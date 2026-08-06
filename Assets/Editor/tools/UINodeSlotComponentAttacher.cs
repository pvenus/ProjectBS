#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Stage.UI;

namespace Stage.EditorTools
{
    public static class UINodeSlotComponentAttacher
    {
        [MenuItem("BS/Tools/Attach UINodeSlot Components To Prefabs")]
        public static void AttachComponents()
        {
            string[] targetPrefabPaths = new string[]
            {
                "Assets/Prefabs/UI/Child/Slot/UINodeSlot_Battle.prefab",
                "Assets/Prefabs/UI/Child/Slot/UINodeSlot_Event.prefab",
                "Assets/Prefabs/UI/Child/Slot/UINodeSlot_Market.prefab",
                "Assets/Prefabs/UI/Child/Slot/UINodeSlot_Shrine.prefab",
            };

            int updatedCount = 0;

            foreach (string path in targetPrefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogWarning($"[UINodeSlotAttacher] Prefab not found at path: {path}");
                    continue;
                }

                // 프리팹 인스턴스 오픈
                string assetPath = AssetDatabase.GetAssetPath(prefab);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

                bool isShrine = path.Contains("Shrine");
                UINodeSlot existingSlot = prefabRoot.GetComponent<UINodeSlot>();

                if (existingSlot == null)
                {
                    if (isShrine)
                    {
                        prefabRoot.AddComponent<UINodeSlot_Shrine>();
                    }
                    else
                    {
                        prefabRoot.AddComponent<UINodeSlot>();
                    }

                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
                    updatedCount++;
                    Debug.Log($"[UINodeSlotAttacher] Added {(isShrine ? "UINodeSlot_Shrine" : "UINodeSlot")} component to prefab at: {path}");
                }

                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[UINodeSlotAttacher] Completed component attachment to {updatedCount} prefabs.");
        }
    }
}
#endif
