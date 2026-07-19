using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBS/UI/Popup View Registry")]
public class PopupViewRegistrySO : ScriptableObject
{
    [SerializeField] private List<PopupViewConfig> configs = new();

    public IReadOnlyList<PopupViewConfig> Configs => configs;

    public bool TryGetConfig(PopupType type, out PopupViewConfig config)
    {
        config = null;

        for (int i = 0; i < configs.Count; i++)
        {
            if (configs[i] == null)
                continue;

            if (configs[i].type == type)
            {
                config = configs[i];
                return true;
            }
        }

        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (configs == null)
            return;

        HashSet<PopupType> seen = new HashSet<PopupType>();

        for (int i = 0; i < configs.Count; i++)
        {
            PopupViewConfig cfg = configs[i];

            if (cfg == null)
            {
                Debug.LogWarning($"[PopupViewRegistrySO] configs[{i}] is null.", this);
                continue;
            }

            if (cfg.type == PopupType.None)
            {
                Debug.LogWarning($"[PopupViewRegistrySO] configs[{i}] has PopupType.None — 유효하지 않은 타입입니다.", this);
                continue;
            }

            if (cfg.prefab == null)
            {
                Debug.LogWarning($"[PopupViewRegistrySO] configs[{i}] ({cfg.type}) prefab이 null입니다.", this);
            }

            if (!seen.Add(cfg.type))
            {
                Debug.LogWarning($"[PopupViewRegistrySO] 중복 타입 발견: {cfg.type} (index {i}). 첫 번째 항목만 사용됩니다.", this);
            }
        }
    }
#endif
}
