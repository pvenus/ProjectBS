

using System.Collections.Generic;
using Item;
using UnityEngine;

namespace Session.SO
{
    /// <summary>
    /// 새 게임 시작 시 StageSession 런타임 데이터에 주입할 초기 설정.
    /// - 시작 골드
    /// - 시작 유물
    /// - 시작 유물 자동 장착 여부
    /// </summary>
    [CreateAssetMenu(
        fileName = "StartProfileSO",
        menuName = "Session/Start Profile")]
    public class StartProfileSO : ScriptableObject
    {
        [Header("Currency")]
        [SerializeField, Min(0)] private int startGold;

        [Header("Relic")]
        [SerializeField] private List<RelicSO> startRelics = new List<RelicSO>();
        [SerializeField] private bool autoEquipStartRelics = true;

        public int StartGold => Mathf.Max(0, startGold);
        public IReadOnlyList<RelicSO> StartRelics => startRelics;
        public bool AutoEquipStartRelics => autoEquipStartRelics;
    }
}