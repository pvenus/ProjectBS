using UnityEngine;

namespace Battle.Morpg
{
    [DefaultExecutionOrder(32000)]
    internal sealed class MorpgTransitionLateView : MonoBehaviour
    {
        internal MorpgDashTransitionView Owner;
        private void LateUpdate()=>Owner?.ApplyPresentation();
        private void OnDisable()=>Owner?.Cancelled?.Invoke();
        private void OnDestroy()=>Owner?.Cancelled?.Invoke();
    }
}
