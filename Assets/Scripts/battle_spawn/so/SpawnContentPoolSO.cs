using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnContentPool", menuName = "BS/Spawn/SpawnContentPool")]
public class SpawnContentPoolSO : ScriptableObject
{
    [SerializeField] private List<SpawnContentSO> contents = new List<SpawnContentSO>();
    public List<SpawnContentSO> Contents => contents;

#if UNITY_EDITOR
    [ContextMenu("Collect Contents in Project")]
    public void CollectAllContents()
    {
        contents.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SpawnContentSO");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            SpawnContentSO asset = UnityEditor.AssetDatabase.LoadAssetAtPath<SpawnContentSO>(path);
            if (asset != null && !contents.Contains(asset))
            {
                contents.Add(asset);
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[SpawnContentPoolSO] 총 {contents.Count}개의 SpawnContentSO 에셋을 수집했습니다.");
    }
#endif
}
