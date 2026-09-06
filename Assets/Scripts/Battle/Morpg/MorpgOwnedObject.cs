using System;
using UnityEngine;

namespace Battle.Morpg
{
    // Kept on a dead root until destruction, so legacy drops stay suppressed for the whole callback.
    internal sealed class MorpgOwnedObject : MonoBehaviour, IMorpgZoneOwnedHandle
    {
        private MorpgZoneWaveScope scope;
        private bool disposing, died;
        internal Transform Staging { get; private set; }
        public MorpgZoneRuntimeHandle Zone => scope?.Handle;
        public string StableId { get; private set; }
        public MorpgOwnedKind Kind { get; private set; }
        internal bool IsClosed => scope == null || scope.Registry.IsDisposed;
        internal void Bind(MorpgZoneWaveScope scope, string id, MorpgOwnedKind kind, Transform staging = null)
        { this.scope = scope; StableId = id; Kind = kind; Staging = staging; }
        internal void MarkDied() { died = true; }
        internal static MorpgOwnedObject From(GameObject owner) =>
            owner == null ? null : owner.GetComponentInParent<MorpgOwnedObject>();
        internal bool RegisterProjectile(GameObject projectile)
        {
            var marker = projectile.AddComponent<MorpgOwnedObject>();
            marker.Bind(scope, "projectile-" + Guid.NewGuid().ToString("N"), MorpgOwnedKind.HostileProjectile, Staging);
            if (scope.Registry.TryRegister(Zone, marker, out _)) return true;
            marker.disposing = true;
            projectile.SetActive(false);
            Destroy(projectile);
            return false;
        }
        public bool TryPrepareDispose(out string error) { error = null; return true; }
        public void DisposePrepared()
        {
            disposing = true;
            if (this == null) return;
            if (Kind == MorpgOwnedKind.EnemyRoot) EnemyRegistry.Instance.UnregisterEnemy(gameObject);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        private void OnDestroy()
        {
            // Unexpected despawn cannot be mistaken for a kill or allow a hanging wave to clear.
            if (Kind == MorpgOwnedKind.EnemyRoot && !disposing && !died)
                scope?.TryFail(Zone, false);
        }
    }
}
