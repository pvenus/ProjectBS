using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬 안의 SkillUpgradeMono 대상을 찾아서,
/// 스킬 슬롯(0~3)과 업그레이드 효과를 선택해 테스트 적용할 수 있는 간단한 디버그 UI.
///
/// 별도 uGUI 세팅 없이 바로 쓸 수 있도록 OnGUI 기반으로 구성한다.
/// 화면 하단에 고정되어 표시된다.
/// </summary>
[DisallowMultipleComponent]
public class UIUpgradeBottomMono : MonoBehaviour
{
    private enum UpgradeEffectType
    {
        Damage,
        Range,
        Cooldown,
        ProjectileScale,
        ProjectileSpeed,
        ProjectileLifetime,
        ProjectileCount,
        KnockbackForce
    }

    [Header("Visibility")]
    [SerializeField] private bool visibleOnStart = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F8;

    [Header("Layout")]
    [SerializeField, Min(300f)] private float panelWidth = 920f;
    [SerializeField, Min(160f)] private float panelHeight = 210f;
    [SerializeField, Min(4f)] private float panelPadding = 12f;
    [SerializeField, Min(4f)] private float buttonHeight = 28f;
    [SerializeField, Min(4f)] private float rowSpacing = 6f;

    [Header("Behavior")]
    [SerializeField, Min(1)] private int upgradeAmount = 1;
    [SerializeField] private bool autoRefreshTargets = true;
    [SerializeField, Min(0.1f)] private float autoRefreshInterval = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool debugLog;

    private readonly List<SkillUpgradeMono> _targets = new List<SkillUpgradeMono>();
    private readonly string[] _slotLabels = { "0 Basic", "1 Skill1", "2 Skill2", "3 Skill3" };
    private readonly UpgradeEffectType[] _effectValues = (UpgradeEffectType[])Enum.GetValues(typeof(UpgradeEffectType));

    private bool _isVisible;
    private int _selectedTargetIndex;
    private int _selectedSlotIndex;
    private UpgradeEffectType _selectedEffect;
    private float _nextRefreshTime;
    private Vector2 _scroll;
    private GUIStyle _panelStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _selectedButtonStyle;
    private GUIStyle _buttonStyle;

    private void Awake()
    {
        _isVisible = visibleOnStart;
        RefreshTargets();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            _isVisible = !_isVisible;

        if (!autoRefreshTargets)
            return;

        if (Time.unscaledTime < _nextRefreshTime)
            return;

        _nextRefreshTime = Time.unscaledTime + Mathf.Max(0.1f, autoRefreshInterval);
        RefreshTargets();
    }

    private void OnGUI()
    {
        if (!_isVisible)
            return;

        EnsureGuiStyles();

        Rect area = new Rect(
            panelPadding,
            Screen.height - panelHeight - panelPadding,
            Mathf.Min(panelWidth, Screen.width - panelPadding * 2f),
            Mathf.Min(panelHeight, Screen.height - panelPadding * 2f)
        );

        GUILayout.BeginArea(area, GUIContent.none, _panelStyle);

        DrawHeader();
        GUILayout.Space(rowSpacing);
        DrawTargetRow();
        GUILayout.Space(rowSpacing);
        DrawSlotRow();
        GUILayout.Space(rowSpacing);
        DrawEffectRow();
        GUILayout.Space(rowSpacing);
        DrawActionRow();
        GUILayout.Space(rowSpacing);
        DrawInfoRow();

        GUILayout.EndArea();
    }

    private void DrawHeader()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Skill Upgrade Test UI", _titleStyle, GUILayout.Height(buttonHeight));

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Refresh Targets", GUILayout.Height(buttonHeight), GUILayout.Width(120f)))
            RefreshTargets();

        if (GUILayout.Button(_isVisible ? "Hide" : "Show", GUILayout.Height(buttonHeight), GUILayout.Width(80f)))
            _isVisible = !_isVisible;

        GUILayout.EndHorizontal();
    }

    private void DrawTargetRow()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Upgrade Target", _labelStyle);

        if (_targets.Count == 0)
        {
            GUILayout.Label("No SkillUpgradeMono found in scene.", _labelStyle, GUILayout.Height(buttonHeight));
            GUILayout.EndVertical();
            return;
        }

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(buttonHeight + 12f));
        GUILayout.BeginHorizontal();

        for (int i = 0; i < _targets.Count; i++)
        {
            SkillUpgradeMono target = _targets[i];
            string label = BuildTargetLabel(target, i);
            GUIStyle style = i == _selectedTargetIndex ? _selectedButtonStyle : _buttonStyle;

            if (GUILayout.Button(label, style, GUILayout.Height(buttonHeight), GUILayout.MinWidth(150f)))
                _selectedTargetIndex = i;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawSlotRow()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Skill Slot", _labelStyle);
        GUILayout.BeginHorizontal();

        for (int i = 0; i < _slotLabels.Length; i++)
        {
            GUIStyle style = i == _selectedSlotIndex ? _selectedButtonStyle : _buttonStyle;
            if (GUILayout.Button(_slotLabels[i], style, GUILayout.Height(buttonHeight)))
                _selectedSlotIndex = i;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawEffectRow()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Upgrade Effect", _labelStyle);
        GUILayout.BeginHorizontal();

        for (int i = 0; i < _effectValues.Length; i++)
        {
            UpgradeEffectType effect = _effectValues[i];
            GUIStyle style = effect == _selectedEffect ? _selectedButtonStyle : _buttonStyle;
            if (GUILayout.Button(effect.ToString(), style, GUILayout.Height(buttonHeight)))
                _selectedEffect = effect;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawActionRow()
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label("Amount", _labelStyle, GUILayout.Width(60f));

        if (GUILayout.Button("-", GUILayout.Width(32f), GUILayout.Height(buttonHeight)))
            upgradeAmount = Mathf.Max(1, upgradeAmount - 1);

        GUILayout.Label(upgradeAmount.ToString(), _labelStyle, GUILayout.Width(40f), GUILayout.Height(buttonHeight));

        if (GUILayout.Button("+", GUILayout.Width(32f), GUILayout.Height(buttonHeight)))
            upgradeAmount = Mathf.Max(1, upgradeAmount + 1);

        GUILayout.Space(12f);

        bool canApply = GetSelectedTarget() != null;
        GUI.enabled = canApply;

        if (GUILayout.Button("Apply Upgrade", GUILayout.Height(buttonHeight), GUILayout.Width(140f)))
            ApplySelectedUpgrade();

        if (GUILayout.Button("Reset Selected Slot", GUILayout.Height(buttonHeight), GUILayout.Width(150f)))
            ResetSelectedSlot();

        if (GUILayout.Button("Reset All", GUILayout.Height(buttonHeight), GUILayout.Width(100f)))
            ResetAll();

        GUI.enabled = true;
        GUILayout.EndHorizontal();
    }

    private void DrawInfoRow()
    {
        SkillUpgradeMono target = GetSelectedTarget();
        if (target == null)
        {
            GUILayout.Label("Selected Target: None", _labelStyle);
            return;
        }

        ScriptableObject selectedSkill = target.GetSkillBySlot(_selectedSlotIndex);
        SkillUpgradeMono.SkillUpgradeData data = target.GetUpgradeDataBySlot(_selectedSlotIndex);
        string skillName = selectedSkill != null ? selectedSkill.name : "(empty)";

        GUILayout.Label(
            $"Target: {target.name} | Slot: {_selectedSlotIndex} | Skill: {skillName} | " +
            $"Damage +{data.damageAdd:0.##}, Range +{data.rangeAdd:0.##}, Cooldown +{data.cooldownAdd:0.##}, " +
            $"Scale +{data.projectileScaleAdd:0.##}, Speed +{data.projectileSpeedAdd:0.##}, Lifetime +{data.projectileLifetimeAdd:0.##}, Count +{data.projectileCountAdd:0.##}, Knockback +{data.knockbackForceAdd:0.##}",
            _labelStyle
        );
    }

    private void ApplySelectedUpgrade()
    {
        SkillUpgradeMono target = GetSelectedTarget();
        if (target == null)
            return;

        int amount = Mathf.Max(1, upgradeAmount);

        switch (_selectedEffect)
        {
            case UpgradeEffectType.Damage:
                target.AddDamageUpgradeBySlot(_selectedSlotIndex, amount);
                break;
            case UpgradeEffectType.Range:
                target.AddRangeUpgradeBySlot(_selectedSlotIndex, amount);
                break;
            case UpgradeEffectType.Cooldown:
                target.AddCooldownUpgradeBySlot(_selectedSlotIndex, amount);
                break;
            case UpgradeEffectType.ProjectileScale:
                target.AddProjectileScaleUpgradeBySlot(_selectedSlotIndex, amount);
                break;
            case UpgradeEffectType.ProjectileSpeed:
                target.AddProjectileSpeedUpgradeBySlot(_selectedSlotIndex, amount);
                break;
            case UpgradeEffectType.ProjectileLifetime:
                target.AddProjectileLifetimeUpgradeBySlot(_selectedSlotIndex, amount);
                break;
            case UpgradeEffectType.ProjectileCount:
                target.AddProjectileCountUpgradeBySlot(_selectedSlotIndex, amount);
                break;
            case UpgradeEffectType.KnockbackForce:
                target.AddKnockbackForceUpgradeBySlot(_selectedSlotIndex, amount);
                break;
        }

        if (debugLog)
            Debug.Log($"[UIUpgradeBottomMono] Apply {_selectedEffect} x{amount} target={target.name} slot={_selectedSlotIndex}", this);
    }

    private void ResetSelectedSlot()
    {
        SkillUpgradeMono target = GetSelectedTarget();
        if (target == null)
            return;

        target.ResetUpgradeBySlot(_selectedSlotIndex);

        if (debugLog)
            Debug.Log($"[UIUpgradeBottomMono] Reset slot target={target.name} slot={_selectedSlotIndex}", this);
    }

    private void ResetAll()
    {
        SkillUpgradeMono target = GetSelectedTarget();
        if (target == null)
            return;

        target.ResetAllUpgrades();

        if (debugLog)
            Debug.Log($"[UIUpgradeBottomMono] Reset all target={target.name}", this);
    }

    private SkillUpgradeMono GetSelectedTarget()
    {
        if (_targets.Count == 0)
            return null;

        _selectedTargetIndex = Mathf.Clamp(_selectedTargetIndex, 0, _targets.Count - 1);
        return _targets[_selectedTargetIndex];
    }

    private void RefreshTargets()
    {
        _targets.Clear();

        SkillUpgradeMono[] found = FindObjectsByType<SkillUpgradeMono>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != null)
                _targets.Add(found[i]);
        }

        if (_targets.Count == 0)
            _selectedTargetIndex = 0;
        else
            _selectedTargetIndex = Mathf.Clamp(_selectedTargetIndex, 0, _targets.Count - 1);

        if (debugLog)
            Debug.Log($"[UIUpgradeBottomMono] RefreshTargets count={_targets.Count}", this);
    }

    private string BuildTargetLabel(SkillUpgradeMono target, int index)
    {
        if (target == null)
            return $"Target {index}";

        return $"{index}: {target.name}";
    }

    private void EnsureGuiStyles()
    {
        if (_panelStyle == null)
        {
            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
        }

        if (_titleStyle == null)
        {
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
        }

        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true
            };
        }

        if (_buttonStyle == null)
            _buttonStyle = new GUIStyle(GUI.skin.button);

        if (_selectedButtonStyle == null)
        {
            _selectedButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold
            };
        }
    }
}