using System.Collections;
using Character;
using Stat;
using UnityEngine;

namespace Character.Service
{
    /// <summary>
    /// CharacterManager의 사망 처리 책임을 분리한 서비스.
    /// - 중복 사망 처리 방지
    /// - 물리/충돌 비활성화
    /// - 선택적 사망 연출 콜백 실행
    /// - 지연 제거 코루틴 제공
    /// </summary>
    public class CharacterDeathService
    {
        public struct Context
        {
            public CharacterManager characterManager;
            public CharacterRuntimeData runtimeData;
            public CharacterManager lastHitAttacker;
            public MonoBehaviour coroutineOwner;
            public GameObject gameObject;
            public Rigidbody2D rigidbody2D;
            public Collider2D[] colliders;
            public float destroyDelay;
            public System.Action<CharacterManager> onDeath;
            public System.Action onBeforeDestroy;
        }

        private bool isHandlingDeath;

        public bool IsHandlingDeath => isHandlingDeath;

        public void Reset()
        {
            isHandlingDeath = false;
        }

        public void HandleDeath(Context context)
        {
            if (isHandlingDeath)
            {
                return;
            }

            if (context.characterManager == null
                || context.runtimeData == null
                || context.gameObject == null)
            {
                return;
            }

            if (TryReviveWithToken(context))
            {
                return;
            }

            isHandlingDeath = true;
            context.runtimeData.isDead = true;

            DisablePhysics(context.rigidbody2D);
            DisableColliders(context.colliders);

            context.onDeath?.Invoke(context.lastHitAttacker);

            if (context.coroutineOwner != null
                && context.gameObject.activeInHierarchy)
            {
                context.coroutineOwner.StartCoroutine(
                    DestroyAfterDelay(context));
            }
            else
            {
                context.onBeforeDestroy?.Invoke();
                Object.Destroy(context.gameObject);
            }
        }

        private bool TryReviveWithToken(Context context)
        {
            CharacterManager characterManager = context.characterManager;

            if (characterManager == null)
            {
                return false;
            }

            float tokenCount = characterManager.GetStatValue(
                StatType.ResurrectionToken);

            if (tokenCount <= 0f)
            {
                return false;
            }

            characterManager.AddStat(
                StatType.ResurrectionToken,
                -1f);

            float maxHp = characterManager.GetStatValue(
                StatType.MaxHp);
            float reviveHp = Mathf.Max(1f, maxHp * 0.3f);

            context.runtimeData.currentHp = reviveHp;
            context.runtimeData.isDead = false;

            isHandlingDeath = false;

            return true;
        }

        private void DisablePhysics(Rigidbody2D rigidbody2D)
        {
            if (rigidbody2D == null)
            {
                return;
            }

            rigidbody2D.linearVelocity = Vector2.zero;
            rigidbody2D.angularVelocity = 0f;
            rigidbody2D.simulated = false;
        }

        private void DisableColliders(Collider2D[] colliders)
        {
            if (colliders == null)
            {
                return;
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D collider = colliders[i];

                if (collider == null)
                {
                    continue;
                }

                collider.enabled = false;
            }
        }

        private IEnumerator DestroyAfterDelay(Context context)
        {
            float delay = Mathf.Max(0f, context.destroyDelay);

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            context.onBeforeDestroy?.Invoke();

            if (context.gameObject != null)
            {
                Object.Destroy(context.gameObject);
            }
        }
    }
}