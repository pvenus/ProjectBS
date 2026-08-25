using UnityEngine;

namespace Battle.Presentation
{
    [DisallowMultipleComponent]
    public sealed class BattleCharacterAuraBinding : MonoBehaviour
    {
        [SerializeField] private BattleCharacterAuraView auraView;

        public BattleCharacterAuraView AuraView => auraView;

        public void Bind(BattleCharacterAuraView view)
        {
            auraView = view;
        }
    }
}
