using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Prop.SO
{
    [CreateAssetMenu(
        fileName = "BattleProp",
        menuName = "Battle/Prop/Battle Prop")]
    public class BattlePropSO : ScriptableObject
    {
        [Serializable]
        public class PropStateVisualEntry
        {
            public BattlePropState state = BattlePropState.Normal;
            public AnimationClip animationClip;
            public GameObject effectPrefab;
        }

        [Header("Identity")]
        public string propId;

        [Header("Role")]
        public BattlePropRole role = BattlePropRole.None;

        [Header("Prefab")]
        public GameObject prefab;

        [Header("Skills")]
        public List<ScriptableObject> skills = new();

        [Header("State Visuals")]
        public List<PropStateVisualEntry> stateVisuals = new();

        [Header("Spawn On Hit")]
        public int spawnHitThreshold = 10;
        public BattlePropSO spawnPropOnHit;
        public bool destroyAfterSpawnOnHit = true;

        [Header("Spawn Sequence")]
        public SpawnSequenceSO spawnSequence;
        public bool playSpawnSequenceOnInitialize;
    }
}
