using UnityEngine;
using ProjectBS.Core;
using UIFramework.Data;
using System.Collections.Generic;

namespace UIFramework.Test
{
    [System.Serializable]
    public class MockBeliefData
    {
        public string godName;
        public Sprite godIcon;
        public int currentLevel = 1;
        public int currentExp = 0;
        public int maxExp = 100;
    }

    public class TestBeliefManager : MonoBehaviour, IBeliefManager
    {
        [Header("Mock Belief Data")]
        [SerializeField] private List<MockBeliefData> mockBeliefs = new List<MockBeliefData>();

        public event System.Action<BeliefListViewData> OnBeliefListChanged;

        private BeliefListViewData currentData;

        private void Awake()
        {
            AppManagers.Belief = this;
        }

        private void Start()
        {
            GenerateMockData();
        }

        public BeliefListViewData GetBeliefList()
        {
            if (currentData == null)
            {
                GenerateMockData();
            }
            return currentData;
        }

        [ContextMenu("Refresh Beliefs")]
        public void GenerateMockData()
        {
            currentData = new BeliefListViewData();

            foreach (var b in mockBeliefs)
            {
                currentData.beliefs.Add(new BeliefItemViewData
                {
                    godId = b.godName, // 임시로 이름을 ID로 사용
                    godName = b.godName,
                    godIcon = b.godIcon,
                    currentLevel = b.currentLevel,
                    currentExp = b.currentExp,
                    maxExpForNextLevel = b.maxExp
                });
            }

            OnBeliefListChanged?.Invoke(currentData);
        }

        [ContextMenu("Add Random Exp to All")]
        public void AddRandomExp()
        {
            foreach (var b in mockBeliefs)
            {
                b.currentExp += Random.Range(10, 50);
                if (b.currentExp >= b.maxExp)
                {
                    b.currentExp -= b.maxExp;
                    b.currentLevel++;
                    b.maxExp = (int)(b.maxExp * 1.5f);
                }
            }
            GenerateMockData();
        }
    }
}
