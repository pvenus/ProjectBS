using UnityEngine;

namespace Battle
{
    [CreateAssetMenu(fileName = "BattleLargeWavePolicySO", menuName = "Battle/Large Wave Policy")]
    public sealed class BattleLargeWavePolicySO : ScriptableObject
    {
        [SerializeField] private string policyId;
        [SerializeField] private bool enabled;
        [SerializeField] private bool soloOriginAtControlHandoff = true;
        [SerializeField] private float reservationCommitTime = 0.25f;
        [SerializeField] private float telegraphDuration = 0.20f;
        [SerializeField] private int hardLivingCap = 28;
        [SerializeField] private float safetyRadius = 4.5f;
        [SerializeField] private float minimumSpawnRadius = 9f;
        [SerializeField] private float maximumSpawnRadius = 14f;
        [SerializeField] private float minimumCameraEdgeDistance = 1.5f;
        [SerializeField] private float maximumCameraEdgeDistance = 2.5f;

        public string PolicyId => policyId;
        public bool Enabled => enabled;
        public bool SoloOriginAtControlHandoff => soloOriginAtControlHandoff;
        public float ReservationCommitTime => reservationCommitTime;
        public float TelegraphDuration => telegraphDuration;
        public int HardLivingCap => hardLivingCap;
        public float SafetyRadius => safetyRadius;
        public float MinimumSpawnRadius => minimumSpawnRadius;
        public float MaximumSpawnRadius => maximumSpawnRadius;
        public float MinimumCameraEdgeDistance => minimumCameraEdgeDistance;
        public float MaximumCameraEdgeDistance => maximumCameraEdgeDistance;
    }
}
