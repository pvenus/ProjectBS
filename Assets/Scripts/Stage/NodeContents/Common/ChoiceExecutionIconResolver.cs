using System;
using System.Collections.Generic;
using Common;
using Shrine;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// PopupEventSO의 Choice Execution Config를 분석해 NodeIconType을 결정하는 Resolver.
    /// RoundNodeSO.overrideIconType이 false인 경우 이 Resolver가 아이콘 타입을 자동 계산한다.
    /// overrideIconType이 true인 경우에는 RoundNodeSO.iconType을 직접 사용한다.
    /// </summary>
    public sealed class ChoiceExecutionIconResolver
    {
        private readonly Func<ChoiceExecutionType, Sprite>
            typeIconProvider;
        private readonly Func<Sprite> multiIconProvider;

        public ChoiceExecutionIconResolver(
            Func<ChoiceExecutionType, Sprite> typeIconProvider,
            Func<Sprite> multiIconProvider)
        {
            this.typeIconProvider = typeIconProvider;
            this.multiIconProvider = multiIconProvider;
        }

        public static ChoiceExecutionIconResolver CreateDefault()
        {
            return new ChoiceExecutionIconResolver(
                GetDefaultTypeIcon,
                () => null);
        }

        /// <summary>
        /// PopupEventSO의 Choice Execution Config를 분석해 NodeIconType을 반환한다.
        /// NextEvent 체인은 재귀적으로 따라가며 말단 ExecutionType을 수집한다.
        /// 말단 타입이 모두 동일한 경우 해당 타입에 매핑된 NodeIconType을 반환한다.
        /// 말단 타입이 혼재하거나 분석 실패 시 fallback을 반환한다.
        /// </summary>
        public static NodeIconType ResolveIconType(
            PopupEventSO popupEvent,
            NodeIconType fallback = NodeIconType.Story)
        {
            if (popupEvent == null)
            {
                return fallback;
            }

            List<ChoiceExecutionConfig> terminals = new();
            HashSet<PopupEventSO> visiting = new();
            bool valid =
                CollectTerminalConfigs(
                    popupEvent,
                    visiting,
                    terminals);

            if (!valid || terminals.Count == 0)
            {
                return fallback;
            }

            ChoiceExecutionType executionType =
                terminals[0].executionType;

            for (int i = 1; i < terminals.Count; i++)
            {
                if (terminals[i].executionType != executionType)
                {
                    // 말단 타입이 혼재하는 경우 fallback 반환
                    return fallback;
                }
            }

            return MapExecutionTypeToIconType(executionType, fallback);
        }

        /// <summary>
        /// ChoiceExecutionType을 NodeIconType으로 매핑한다.
        /// </summary>
        public static NodeIconType MapExecutionTypeToIconType(
            ChoiceExecutionType executionType,
            NodeIconType fallback = NodeIconType.Story)
        {
            return executionType switch
            {
                ChoiceExecutionType.Battle => NodeIconType.Battle,
                ChoiceExecutionType.Shop   => NodeIconType.Shop,
                ChoiceExecutionType.Shrine => NodeIconType.Shrine,
                ChoiceExecutionType.CompleteEvent => NodeIconType.Story,
                _ => fallback
            };
        }

        // ── 아래는 Sprite 반환이 필요한 경우를 위해 유지된 레거시 API ──

        public Sprite Resolve(
            PopupEventSO popupEvent,
            Sprite fallbackIcon = null)
        {
            List<ChoiceExecutionConfig> terminals = new();
            HashSet<PopupEventSO> visiting = new();
            bool valid =
                CollectTerminalConfigs(
                    popupEvent,
                    visiting,
                    terminals);

            if (!valid || terminals.Count == 0)
            {
                return fallbackIcon;
            }

            ChoiceExecutionType executionType =
                terminals[0].executionType;

            for (int i = 1; i < terminals.Count; i++)
            {
                if (terminals[i].executionType != executionType)
                {
                    return ResolveMulti(fallbackIcon);
                }
            }

            if (executionType == ChoiceExecutionType.Shrine)
            {
                return ResolveShrineGroup(
                    terminals,
                    fallbackIcon);
            }

            return Resolve(terminals[0], fallbackIcon);
        }

        public Sprite Resolve(
            ChoiceExecutionConfig config,
            Sprite fallbackIcon = null)
        {
            if (config == null || config.data == null)
            {
                return fallbackIcon;
            }

            if (config.data is ShrineExecutionData shrineData)
            {
                return shrineData.god?.Icon ?? fallbackIcon;
            }

            Sprite icon =
                typeIconProvider?.Invoke(config.executionType);
            return icon != null ? icon : fallbackIcon;
        }

        private Sprite ResolveShrineGroup(
            IReadOnlyList<ChoiceExecutionConfig> configs,
            Sprite fallbackIcon)
        {
            ShrineGodType godType = ShrineGodType.None;
            ChoiceExecutionConfig first = null;

            foreach (ChoiceExecutionConfig config in configs)
            {
                if (config.data
                    is not ShrineExecutionData shrineData
                    || shrineData.god == null)
                {
                    return fallbackIcon;
                }

                if (first == null)
                {
                    first = config;
                    godType = shrineData.god.GodType;
                    continue;
                }

                if (shrineData.god.GodType != godType)
                {
                    return ResolveMulti(fallbackIcon);
                }
            }

            return Resolve(first, fallbackIcon);
        }

        private Sprite ResolveMulti(Sprite fallbackIcon)
        {
            Sprite icon = multiIconProvider?.Invoke();
            return icon != null ? icon : fallbackIcon;
        }

        private static bool CollectTerminalConfigs(
            PopupEventSO popupEvent,
            HashSet<PopupEventSO> visiting,
            List<ChoiceExecutionConfig> terminals)
        {
            if (popupEvent == null
                || popupEvent.choices == null
                || !visiting.Add(popupEvent))
            {
                return false;
            }

            bool valid = true;

            foreach (PopupEventChoice choice in popupEvent.choices)
            {
                if (choice == null)
                {
                    valid = false;
                    continue;
                }

                ChoiceExecutionConfig config =
                    choice.executionConfig;

                if (config?.executionType
                        == ChoiceExecutionType.NextEvent
                    && config.data
                        is NextEventExecutionData nextData)
                {
                    valid &=
                        CollectTerminalConfigs(
                            nextData.nextEvent,
                            visiting,
                            terminals);
                    continue;
                }

                if (config != null)
                {
                    if (ChoiceExecutionConfigValidator
                            .Validate(config).Count > 0)
                    {
                        valid = false;
                        continue;
                    }

                    terminals.Add(config);
                    continue;
                }

                valid = false;
            }

            visiting.Remove(popupEvent);
            return valid;
        }

        private static Sprite GetDefaultTypeIcon(
            ChoiceExecutionType executionType)
        {
            return null;
        }
    }
}
