

using System;
using UnityEngine;

namespace Party.UI
{
    public class DamagePopupUI : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TextMesh textMesh;

        [SerializeField] private Color textColor = new(1f, 0.9f, 0.2f, 1f);

        [SerializeField] private Color criticalTextColor = new(1f, 0.25f, 0.1f, 1f);

        [Header("Motion")]
        [SerializeField] private float defaultLife = 0.6f;

        [SerializeField] private float defaultRiseSpeed = 1.2f;

        private float life;
        private float riseSpeed;
        private float elapsedTime;

        private Action<DamagePopupUI> onComplete;

        private void Awake()
        {
            if (textMesh == null)
            {
                textMesh = GetComponent<TextMesh>();
            }
        }

        public void Initialize(
            float damage,
            float popupLife,
            float popupRiseSpeed,
            int fontSize,
            float characterSize,
            string sortingLayerName,
            int sortingOrder,
            Action<DamagePopupUI> completeCallback,
            bool isCritical = false)
        {
            life = Mathf.Max(0.05f, popupLife > 0f ? popupLife : defaultLife);
            riseSpeed = Mathf.Max(0.1f, popupRiseSpeed > 0f ? popupRiseSpeed : defaultRiseSpeed);
            elapsedTime = 0f;
            onComplete = completeCallback;

            EnsureTextMesh();

            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = Mathf.Max(32, fontSize);
            textMesh.characterSize = Mathf.Max(0.04f, characterSize);
            textMesh.text = isCritical
                ? $"CRIT {damage:0}"
                : $"{damage:0}";

            textMesh.color = isCritical
                ? criticalTextColor
                : textColor;

            MeshRenderer meshRenderer =
                textMesh.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                meshRenderer.sortingLayerName = sortingLayerName;
                meshRenderer.sortingOrder = sortingOrder;
            }
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;

            float ratio =
                Mathf.Clamp01(elapsedTime / life);

            transform.position +=
                Vector3.up * (riseSpeed * Time.deltaTime);

            if (textMesh != null)
            {
                Color color = textMesh.color;
                color.a = Mathf.Lerp(1f, 0f, ratio);
                textMesh.color = color;
            }

            if (elapsedTime >= life)
            {
                Action<DamagePopupUI> completeCallback = onComplete;
                onComplete = null;

                completeCallback?.Invoke(this);
            }
        }

        private void EnsureTextMesh()
        {
            if (textMesh != null)
            {
                return;
            }

            textMesh = GetComponent<TextMesh>();

            if (textMesh == null)
            {
                textMesh = gameObject.AddComponent<TextMesh>();
            }
        }
    }
}