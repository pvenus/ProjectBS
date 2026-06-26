using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnPatternPool", menuName = "BS/Spawn/SpawnPatternPool")]
public class SpawnPatternPoolSO : ScriptableObject
{
    [SerializeField] private List<ScriptableObject> patterns = new List<ScriptableObject>();
    public List<ScriptableObject> Patterns => patterns;

#if UNITY_EDITOR
    [ContextMenu("Collect Patterns in Project")]
    public void CollectAllPatterns()
    {
        patterns.Clear();
        
        // SpawnPattern (FixedPatternSO 및 RandomPatternSO) 에셋 통합 수집
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SpawnPattern");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            SpawnPattern asset = UnityEditor.AssetDatabase.LoadAssetAtPath<SpawnPattern>(path);
            if (asset != null && !patterns.Contains(asset))
            {
                patterns.Add(asset);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[SpawnPatternPoolSO] 총 {patterns.Count}개의 패턴 에셋을 수집했습니다. (FixedPatternSO & RandomPatternSO 통합)");
    }
#endif
}
