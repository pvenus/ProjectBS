using System.Collections.Generic;
using UnityEngine;
using Character;

[CreateAssetMenu(fileName = "SpawnNpcPool", menuName = "BS/Spawn/SpawnNpcPool")]
public class SpawnNpcPoolSO : ScriptableObject
{
    [SerializeField] private List<CharacterSO> npcs = new List<CharacterSO>();
    public List<CharacterSO> Npcs => npcs;

#if UNITY_EDITOR
    [ContextMenu("Collect NPCs in Project")]
    public void CollectAllNpcs()
    {
        npcs.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterSO");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            CharacterSO asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterSO>(path);
            if (asset != null && !npcs.Contains(asset))
            {
                npcs.Add(asset);
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[SpawnNpcPoolSO] 총 {npcs.Count}개의 CharacterSO 에셋을 수집했습니다.");
    }
#endif
}
