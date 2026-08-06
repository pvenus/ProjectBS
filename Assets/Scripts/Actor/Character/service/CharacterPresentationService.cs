

using System;
using Party.UI;
using UnityEngine;

namespace Character.Service
{
    /// <summary>
    /// CharacterManager의 HUD 생성 및 데미지 표시 책임을 분리한 서비스.
    ///
    /// 실제 HUD 초기화/데미지 표시 API는 프로젝트마다 다를 수 있으므로,
    /// CharacterManager에서 콜백을 넘겨 기존 UI 메서드를 그대로 사용할 수 있게 한다.
    /// </summary>
    public class CharacterPresentationService
    {
        public CharacterBattleHudUI CreateBattleHud(
            CharacterBattleHudUI prefab,
            Transform ownerTransform,
            Transform parent,
            Action<CharacterBattleHudUI> onCreated = null)
        {
            if (prefab == null || ownerTransform == null)
            {
                return null;
            }

            CharacterBattleHudUI hud = parent != null
                ? UnityEngine.Object.Instantiate(prefab, parent)
                : UnityEngine.Object.Instantiate(prefab);

            if (hud == null)
            {
                return null;
            }

            hud.transform.position = ownerTransform.position;
            onCreated?.Invoke(hud);

            return hud;
        }

        public void PlayDamagePresentation(
            CharacterBattleHudUI hud,
            float damage,
            bool isCritical,
            Action<CharacterBattleHudUI, float, bool> onPlayDamage = null)
        {
            if (hud == null || damage <= 0f)
            {
                return;
            }

            onPlayDamage?.Invoke(
                hud,
                damage,
                isCritical);
        }

        public void DestroyBattleHud(
            CharacterBattleHudUI hud)
        {
            if (hud == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(hud.gameObject);
        }
    }
}