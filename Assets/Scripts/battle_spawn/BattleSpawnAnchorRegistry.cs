using System.Collections.Generic;
using UnityEngine;

public sealed class BattleSpawnAnchorRegistry
{
    private static BattleSpawnAnchorRegistry instance;
    public static BattleSpawnAnchorRegistry Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new BattleSpawnAnchorRegistry();
            }
            return instance;
        }
    }

    private readonly Dictionary<string, Transform> anchors = new Dictionary<string, Transform>();
    private bool isInitialized;

    public void Initialize()
    {
        anchors.Clear();
        isInitialized = false;

        BattleSpawnAnchor[] foundAnchors = Object.FindObjectsOfType<BattleSpawnAnchor>();
        for (int i = 0; i < foundAnchors.Length; i++)
        {
            BattleSpawnAnchor anchor = foundAnchors[i];
            if (anchor == null) continue;

            string id = anchor.AnchorId;
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError($"[BattleSpawnAnchorRegistry] Anchor가 비어 있는 ID를 가집니다: {anchor.gameObject.name}", anchor);
                continue;
            }

            if (anchors.ContainsKey(id))
            {
                Debug.LogError($"[BattleSpawnAnchorRegistry] 중복된 Anchor ID가 발견되었습니다: {id} (오브젝트: {anchor.gameObject.name})", anchor);
                continue;
            }

            anchors.Add(id, anchor.transform);
        }

        isInitialized = true;
        Debug.Log($"[BattleSpawnAnchorRegistry] 초기화 완료. 등록된 Anchor 개수: {anchors.Count}");
    }

    public Transform GetAnchor(string anchorId)
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (string.IsNullOrEmpty(anchorId))
        {
            Debug.LogError("[BattleSpawnAnchorRegistry] GetAnchor 요청에 빈 ID가 입력되었습니다.");
            return null;
        }

        if (anchors.TryGetValue(anchorId, out Transform targetTransform))
        {
            return targetTransform;
        }

        Debug.LogError($"[BattleSpawnAnchorRegistry] 존재하지 않는 Anchor가 요청되었습니다: '{anchorId}'");
        return null;
    }
}
