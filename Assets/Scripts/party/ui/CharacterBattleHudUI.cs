using Character;
using Stat;
using UnityEngine;

namespace Party.UI
{
    [DisallowMultipleComponent]
    public class CharacterBattleHudUI : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private CharacterManager characterManager;

        [Header("Root")]
        [SerializeField] private GameObject hudRoot;

        [SerializeField] private Vector3 worldOffset = new(0f, 1.2f, 0f);

        [Header("Hp Bar")]
        [SerializeField] private Transform hpFill;
        [SerializeField] private SpriteRenderer hpBackgroundRenderer;

        [SerializeField] private SpriteRenderer hpFillRenderer;

        [Header("Shield Bar")]
        [SerializeField] private Transform shieldFill;

        [SerializeField] private SpriteRenderer shieldFillRenderer;

        [SerializeField] private Color shieldColor = new(0.45f, 0.75f, 1f, 0.85f);

        [SerializeField] private Color highHpColor = new(0.2f, 1f, 0.25f, 1f);

        [SerializeField] private Color midHpColor = new(1f, 0.85f, 0.1f, 1f);

        [SerializeField] private Color lowHpColor = new(1f, 0.2f, 0.15f, 1f);

        [Header("Options")]
        [SerializeField] private bool hideWhenFullHp = true;

        [SerializeField] private bool hideWhenDead = true;

        private Vector3 initialFillScale = Vector3.one;
        private Vector3 initialShieldScale = Vector3.one;
        private static Sprite sharedBarSprite;

        private void Awake()
        {
            EnsureVisuals();
            if (hpFill != null)
            {
                initialFillScale = hpFill.localScale;
            }

            if (shieldFill != null)
            {
                initialShieldScale = shieldFill.localScale;
            }

            ResolveTarget();

            Refresh(force: true);
        }

        private void LateUpdate()
        {
            Refresh();
        }

        public void Initialize(CharacterManager target)
        {
            characterManager = target;

            ResolveTarget();

            Refresh(force: true);
        }

        public static CharacterBattleHudUI CreateFor(
            CharacterManager target,
            Transform parent = null)
        {
            if (target == null)
            {
                return null;
            }

            GameObject hudObject = new("CharacterBattleHudUI");

            Transform hudParent = parent != null
                ? parent
                : target.transform;

            hudObject.transform.SetParent(hudParent, false);
            hudObject.transform.localPosition = Vector3.zero;
            hudObject.transform.localRotation = Quaternion.identity;
            hudObject.transform.localScale = Vector3.one;

            CharacterBattleHudUI hud =
                hudObject.AddComponent<CharacterBattleHudUI>();

            hud.Initialize(target);
            return hud;
        }

        public static CharacterBattleHudUI EnsureFor(
            CharacterManager target,
            Transform parent = null)
        {
            if (target == null)
            {
                return null;
            }

            CharacterBattleHudUI existing =
                target.GetComponentInChildren<CharacterBattleHudUI>(true);

            if (existing != null)
            {
                existing.Initialize(target);
                return existing;
            }

            return CreateFor(
                target,
                parent);
        }

        public void Refresh(bool force = false)
        {
            if (characterManager == null)
            {
                SetVisible(false);
                return;
            }

            float maxHp =
                characterManager.GetStatValue(StatType.MaxHp);

            float currentHp =
                characterManager.GetStatValue(StatType.Hp);

            float currentShield =
                characterManager.GetStatValue(StatType.Shield);

            float ratio =
                maxHp > 0f
                    ? Mathf.Clamp01(currentHp / maxHp)
                    : 0f;

            float totalVisibleValue =
                maxHp > 0f
                    ? Mathf.Max(maxHp, currentHp + currentShield)
                    : currentHp + currentShield;

            float hpVisibleRatio =
                totalVisibleValue > 0f
                    ? Mathf.Clamp01(currentHp / totalVisibleValue)
                    : 0f;

            float shieldVisibleRatio =
                totalVisibleValue > 0f
                    ? Mathf.Clamp01(currentShield / totalVisibleValue)
                    : 0f;

            bool isDead =
                characterManager.RuntimeData != null
                && characterManager.RuntimeData.isDead;

            bool visible = true;

            if (hideWhenDead && isDead)
            {
                visible = false;
            }

            if (hideWhenFullHp && ratio >= 0.999f)
            {
                visible = false;
            }

            if (hpFill != null)
            {
                ApplyLeftAnchoredFill(
                    hpFill,
                    initialFillScale,
                    hpVisibleRatio,
                    0f);
            }

            if (shieldFill != null)
            {
                ApplyLeftAnchoredFill(
                    shieldFill,
                    initialShieldScale,
                    shieldVisibleRatio,
                    hpVisibleRatio);
            }

            if (hpFillRenderer != null)
            {
                hpFillRenderer.color =
                    EvaluateHpColor(ratio);
            }

            if (shieldFillRenderer != null)
            {
                shieldFillRenderer.color = shieldColor;
                shieldFillRenderer.enabled = currentShield > 0f;
            }

            transform.position =
                characterManager.transform.position
                + worldOffset;

            SetVisible(visible);
        }

        private void ResolveTarget()
        {
            if (characterManager == null)
            {
                characterManager =
                    GetComponentInParent<CharacterManager>();
            }

            if (hpFillRenderer == null
                && hpFill != null)
            {
                hpFillRenderer =
                    hpFill.GetComponent<SpriteRenderer>();
            }

            if (shieldFillRenderer == null
                && shieldFill != null)
            {
                shieldFillRenderer =
                    shieldFill.GetComponent<SpriteRenderer>();
            }

            if (hpBackgroundRenderer == null)
            {
                hpBackgroundRenderer =
                    GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void EnsureVisuals()
        {
            if (sharedBarSprite == null)
            {
                Texture2D texture = new(1, 1);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();

                sharedBarSprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f),
                    1f);
            }

            if (hudRoot == null)
            {
                GameObject rootObject =
                    new("BattleHud_Root");

                rootObject.transform.SetParent(transform, false);
                rootObject.transform.localPosition = Vector3.zero;

                hudRoot = rootObject;
            }

            if (hpBackgroundRenderer == null)
            {
                GameObject backgroundObject =
                    new("HpBar_Background");

                backgroundObject.transform.SetParent(hudRoot.transform, false);
                backgroundObject.transform.localPosition = Vector3.zero;
                backgroundObject.transform.localScale = new Vector3(1.1f, 0.16f, 1f);

                hpBackgroundRenderer =
                    backgroundObject.AddComponent<SpriteRenderer>();

                hpBackgroundRenderer.sprite = sharedBarSprite;
                hpBackgroundRenderer.color = new Color(0f, 0f, 0f, 0.75f);
                hpBackgroundRenderer.sortingLayerName = "UI";
                hpBackgroundRenderer.sortingOrder = 200;
            }

            if (hpFill == null)
            {
                GameObject fillObject =
                    new("HpBar_Fill");

                fillObject.transform.SetParent(hudRoot.transform, false);
                fillObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                fillObject.transform.localScale = new Vector3(1f, 0.1f, 1f);

                hpFill = fillObject.transform;
            }

            if (shieldFill == null)
            {
                GameObject shieldObject =
                    new("ShieldBar_Fill");

                shieldObject.transform.SetParent(hudRoot.transform, false);
                shieldObject.transform.localPosition = new Vector3(0f, 0.015f, -0.02f);
                shieldObject.transform.localScale = new Vector3(1f, 0.12f, 1f);

                shieldFill = shieldObject.transform;
            }

            if (hpFillRenderer == null)
            {
                hpFillRenderer =
                    hpFill.GetComponent<SpriteRenderer>();

                if (hpFillRenderer == null)
                {
                    hpFillRenderer =
                        hpFill.gameObject.AddComponent<SpriteRenderer>();
                }

                hpFillRenderer.sprite = sharedBarSprite;
                hpFillRenderer.sortingLayerName = "UI";
                hpFillRenderer.sortingOrder = 201;
            }

            if (shieldFillRenderer == null)
            {
                shieldFillRenderer =
                    shieldFill.GetComponent<SpriteRenderer>();

                if (shieldFillRenderer == null)
                {
                    shieldFillRenderer =
                        shieldFill.gameObject.AddComponent<SpriteRenderer>();
                }

                shieldFillRenderer.sprite = sharedBarSprite;
                shieldFillRenderer.color = shieldColor;
                shieldFillRenderer.sortingLayerName = "UI";
                shieldFillRenderer.sortingOrder = 202;
                shieldFillRenderer.enabled = false;
            }
        }

        private void SetVisible(bool visible)
        {
            if (hudRoot == null)
            {
                return;
            }

            if (hudRoot == gameObject)
            {
                return;
            }

            hudRoot.SetActive(visible);
        }

        private Color EvaluateHpColor(float ratio)
        {
            if (ratio <= 0.35f)
            {
                return Color.Lerp(
                    lowHpColor,
                    midHpColor,
                    ratio / 0.35f);
            }

            return Color.Lerp(
                midHpColor,
                highHpColor,
                (ratio - 0.35f) / 0.65f);
        }

        private void ApplyLeftAnchoredFill(
            Transform fill,
            Vector3 initialScale,
            float ratio,
            float startRatio)
        {
            float fillScaleX =
                initialScale.x * Mathf.Clamp01(ratio);

            float startOffsetX =
                initialScale.x * Mathf.Clamp01(startRatio);

            fill.localScale = new Vector3(
                fillScaleX,
                initialScale.y,
                initialScale.z);

            fill.localPosition = new Vector3(
                -(initialScale.x - fillScaleX) * 0.5f + startOffsetX,
                fill.localPosition.y,
                fill.localPosition.z);
        }
    }
}