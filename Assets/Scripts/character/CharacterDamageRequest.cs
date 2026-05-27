using UnityEngine;

namespace Character
{
    public class CharacterDamageRequest
    {
        public GameObject attacker;

        public GameObject target;

        public float baseDamage;

        public float attackDamagePercent = 1f;
    }
}
