using UnityEngine;

namespace Character
{
    public class CharacterDamageRequest
    {
        public GameObject attacker;

        public GameObject target;

        public float attackDamagePercent = 100f;

        public float flatBonusDamage;
    }
}
