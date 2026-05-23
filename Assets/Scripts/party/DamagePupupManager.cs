

using System.Collections.Generic;
using Party.UI;
using UnityEngine;

namespace Party
{
    public class DamagePupupManager : MonoBehaviour
    {
        public static DamagePupupManager Instance { get; private set; }

        [Header("Prefab")]
        [SerializeField] private GameObject damagePopupPrefab;

        [SerializeField] private Transform poolRoot;

        [SerializeField] private int prewarmCount = 16;

        [Header("Default View")]
        [SerializeField] private float popupLife = 0.6f;

        [SerializeField] private float popupRiseSpeed = 1.2f;

        [SerializeField] private int popupFontSize = 58;

        [SerializeField] private float popupCharacterSize = 0.075f;

        [SerializeField] private string popupSortingLayerName = "UI";

        [SerializeField] private int popupSortingOrder = 210;

        [Header("Critical View")]
        [SerializeField] private float criticalScaleMultiplier = 1.35f;

        [SerializeField] private int criticalSortingOrderBonus = 10;

        private readonly Queue<DamagePopupUI> pool = new();

        private void Awake()
        {
            if (Instance != null
                && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            Prewarm();
        }

        public void ShowDamage(
            float damage,
            Vector3 worldPosition)
        {
            ShowDamage(
                damage,
                worldPosition,
                false);
        }

        public void ShowDamage(
            float damage,
            Vector3 worldPosition,
            bool isCritical)
        {
            DamagePopupUI popup =
                GetFromPool();

            if (popup == null)
            {
                return;
            }

            popup.transform.SetParent(null, true);
            popup.transform.position = worldPosition;
            popup.transform.localScale =
                isCritical
                    ? Vector3.one * criticalScaleMultiplier
                    : Vector3.one;
            popup.gameObject.SetActive(true);

            popup.Initialize(
                damage,
                popupLife,
                popupRiseSpeed,
                popupFontSize,
                popupCharacterSize,
                popupSortingLayerName,
                popupSortingOrder + (isCritical ? criticalSortingOrderBonus : 0),
                ReturnToPool,
                isCritical);
        }

        private void Prewarm()
        {
            for (int i = 0;
                 i < prewarmCount;
                 i++)
            {
                DamagePopupUI popup =
                    CreateInstance();

                ReturnToPool(popup);
            }
        }

        private DamagePopupUI GetFromPool()
        {
            while (pool.Count > 0)
            {
                DamagePopupUI popup =
                    pool.Dequeue();

                if (popup != null)
                {
                    return popup;
                }
            }

            return CreateInstance();
        }

        private DamagePopupUI CreateInstance()
        {
            GameObject instance;

            if (damagePopupPrefab != null)
            {
                instance = Instantiate(damagePopupPrefab);
            }
            else
            {
                instance = new GameObject("DamagePopup");
            }

            DamagePopupUI popup =
                instance.GetComponent<DamagePopupUI>();

            if (popup == null)
            {
                popup = instance.AddComponent<DamagePopupUI>();
            }

            instance.SetActive(false);

            return popup;
        }

        private void ReturnToPool(DamagePopupUI popup)
        {
            if (popup == null)
            {
                return;
            }

            popup.gameObject.SetActive(false);

            if (poolRoot != null)
            {
                popup.transform.SetParent(poolRoot, false);
            }
            else
            {
                popup.transform.SetParent(transform, false);
            }

            pool.Enqueue(popup);
        }
    }
}