

using Battle;
using Skill;
using Skill.Service.Helper;
using UnityEngine;

namespace Item.Service
{
    public class StrategicSkillItemUseService
    {
        private readonly EquipmentSkillResolver skillResolver = new();
        private readonly StrategicSkillCostManager costManager = StrategicSkillCostManager.Instance;

        public bool TryUseFromScreenPosition(
            StrategicSkillItemSO strategicSkillItem,
            Vector2 screenPosition,
            Camera worldCamera,
            MonoBehaviour coroutineRunner,
            bool logDebug = false,
            Object logContext = null)
        {
            if (worldCamera == null)
            {
                Debug.LogWarning("[StrategicSkillItemUseService] World camera is null.", logContext);
                return false;
            }

            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    Mathf.Abs(worldCamera.transform.position.z)));
            worldPosition.z = 0f;

            return TryUse(
                strategicSkillItem,
                worldPosition,
                coroutineRunner,
                logDebug,
                logContext);
        }

        public bool TryUse(
            StrategicSkillItemSO strategicSkillItem,
            Vector3 worldPosition,
            MonoBehaviour coroutineRunner,
            bool logDebug = false,
            Object logContext = null)
        {
            if (!TryBuildRuntimeData(
                    strategicSkillItem,
                    logDebug,
                    logContext,
                    out EquipmentSkillRuntimeData runtimeData))
            {
                return false;
            }

            if (costManager == null)
            {
                Debug.LogWarning("[StrategicSkillItemUseService] StrategicSkillCostManager is null.", logContext);
                return false;
            }

            if (!costManager.TrySpend(strategicSkillItem.gaugeCost))
            {
                if (logDebug)
                {
                    Debug.Log($"[StrategicSkillItemUseService] Not enough strategic skill gauge. item={strategicSkillItem.DisplayName} cost={strategicSkillItem.gaugeCost}", logContext);
                }

                return false;
            }

            SkillUseHelper.UseSkill(new SkillUseContext
            {
                Runtime = runtimeData,
                Caster = null,
                Target = null,
                UsePoint = true,
                TargetPoint = worldPosition,
                CoroutineRunner = coroutineRunner
            });

            if (logDebug)
            {
                Debug.Log($"[StrategicSkillItemUseService] Strategic skill executed. item={strategicSkillItem.DisplayName} pos={worldPosition}", logContext);
            }

            return true;
        }

        private bool TryBuildRuntimeData(
            StrategicSkillItemSO strategicSkillItem,
            bool logDebug,
            Object logContext,
            out EquipmentSkillRuntimeData runtimeData)
        {
            runtimeData = null;

            if (strategicSkillItem == null)
            {
                return false;
            }

            if (strategicSkillItem.skillSo == null)
            {
                Debug.LogWarning($"[StrategicSkillItemUseService] SkillSO is null. item={strategicSkillItem.DisplayName}", logContext);
                return false;
            }

            EquipmentSkillInstanceData instanceData = new EquipmentSkillInstanceData
            {
                equipmentId = strategicSkillItem.skillSo.EquipmentId,
            };

            runtimeData = skillResolver.Resolve(
                strategicSkillItem.skillSo,
                instanceData);

            if (runtimeData == null)
            {
                Debug.LogWarning($"[StrategicSkillItemUseService] RuntimeData is null. item={strategicSkillItem.DisplayName}", logContext);
                return false;
            }

            if (logDebug)
            {
                Debug.Log($"[StrategicSkillItemUseService] Skill runtime data built. item={strategicSkillItem.DisplayName}", logContext);
            }

            return true;
        }
    }
}