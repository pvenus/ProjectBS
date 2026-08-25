using UnityEngine;
using ProjectBS.Core;

namespace UIFramework.Test
{
    public class TestBattleManager : MonoBehaviour, IBattleManager
    {
        [Header("UI Prefab")]
        public PlaceholderPanel battlePlaceholderPrefab;
        private PlaceholderPanel battlePlaceholderInstance;

        private void Awake()
        {
            AppManagers.Battle = this;
        }

        public void StartBattle(BattleStartRequest request)
        {
            Debug.Log($"[TestBattleManager] StartBattle requested! BattleId: {request.BattleId}");
            var panel = GetOrInstantiate(battlePlaceholderPrefab, ref battlePlaceholderInstance);
            if (panel != null) panel.Show("전투 진입", $"Battle ID: {request.BattleId} 전투 시작...");
        }

        private T GetOrInstantiate<T>(T prefab, ref T instance) where T : MonoBehaviour
        {
            if (instance != null) return instance;
            if (prefab == null) return null;
            Transform parent = GameObject.Find("LobbyUIRoot/PopupLayer")?.transform ?? FindObjectOfType<Canvas>()?.transform ?? transform;
            instance = Instantiate(prefab, parent);
            return instance;
        }
    }
}
