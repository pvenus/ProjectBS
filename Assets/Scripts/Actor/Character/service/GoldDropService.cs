using System;
using Character;
using Stat;
using UnityEngine;
using Currency;

namespace Character.Service
{
    /// <summary>
    /// 캐릭터 사망 시 골드 드랍/획득량을 계산하는 서비스.
    ///
    /// 계산 기준:
    /// - 기본 골드: 사망한 캐릭터의 DropGold
    /// - GoldGain: 막타 공격자의 GoldGain% 만큼 기본 골드 증가
    /// - BonusGoldDropChance: 막타 공격자 기준 추가 골드 발생 확률
    /// - BonusGoldDropPercent: 기본 골드 기준 추가 골드 비율
    /// - EliteGoldBonus: 보스 사망 시 막타 공격자 기준 추가 골드 비율
    /// </summary>
    public class GoldDropService
    {
        public struct Result
        {
            public int baseGold;
            public int goldGainBonus;
            public int bonusGold;
            public int bossGoldBonus;
            public int totalGold;
            public bool bonusTriggered;
        }

        public Result DropGold(
            CharacterManager deadCharacter,
            CharacterManager lastHitAttacker,
            Action<int> onGoldGained = null)
        {
            Result result = new Result();

            if (deadCharacter == null
                || deadCharacter.RuntimeData == null)
            {
                return result;
            }

            float dropGold =
                Mathf.Max(
                    0f,
                    deadCharacter.GetStatValue(StatType.DropGold));

            if (dropGold <= 0f)
            {
                return result;
            }

            float goldGainPercent = 0f;
            float bonusGoldDropChance = 0f;
            float bonusGoldDropPercent = 0f;
            float eliteGoldBonusPercent = 0f;

            if (lastHitAttacker != null
                && lastHitAttacker.RuntimeData != null)
            {
                goldGainPercent =
                    Mathf.Max(
                        0f,
                        lastHitAttacker.GetStatValue(StatType.GoldGain));

                bonusGoldDropChance =
                    Mathf.Max(
                        0f,
                        lastHitAttacker.GetStatValue(StatType.BonusGoldDropChance));

                bonusGoldDropPercent =
                    Mathf.Max(
                        0f,
                        lastHitAttacker.GetStatValue(StatType.BonusGoldDropPercent));

                eliteGoldBonusPercent =
                    Mathf.Max(
                        0f,
                        lastHitAttacker.GetStatValue(StatType.EliteGoldBonus));
            }

            float goldGainBonus =
                dropGold * (goldGainPercent / 100f);

            bool bonusTriggered =
                bonusGoldDropChance > 0f
                && UnityEngine.Random.value <= Mathf.Clamp01(bonusGoldDropChance / 100f);

            float bonusGold = bonusTriggered
                ? dropGold * (bonusGoldDropPercent / 100f)
                : 0f;

            float bossGoldBonus = IsBoss(deadCharacter)
                ? dropGold * (eliteGoldBonusPercent / 100f)
                : 0f;

            result.baseGold = Mathf.RoundToInt(dropGold);
            result.goldGainBonus = Mathf.RoundToInt(goldGainBonus);
            result.bonusGold = Mathf.RoundToInt(bonusGold);
            result.bossGoldBonus = Mathf.RoundToInt(bossGoldBonus);
            result.totalGold = Mathf.Max(
                0,
                result.baseGold + result.goldGainBonus + result.bonusGold + result.bossGoldBonus);
            result.bonusTriggered = bonusTriggered;

            if (result.totalGold <= 0)
            {
                return result;
            }

            int normalGold =
                Mathf.Max(
                    0,
                    result.baseGold + result.goldGainBonus);

            if (normalGold > 0)
            {
                SpawnGoldCoin(
                    deadCharacter.transform.position,
                    normalGold,
                    "Gold");
            }

            if (result.bonusTriggered && result.bonusGold > 0)
            {
                SpawnGoldCoin(
                    deadCharacter.transform.position + new Vector3(0.35f, 0.15f, 0f),
                    result.bonusGold,
                    "BonusGold");
            }

            if (result.bossGoldBonus > 0)
            {
                SpawnGoldCoin(
                    deadCharacter.transform.position + new Vector3(-0.35f, 0.15f, 0f),
                    result.bossGoldBonus,
                    "BossGoldBonus");
            }

            onGoldGained?.Invoke(result.totalGold);
            return result;
        }

        private bool IsBoss(CharacterManager characterManager)
        {
            return characterManager != null
                   && characterManager.RuntimeData != null
                   && characterManager.RuntimeData.characterSO.CharacterType == CharacterType.Boss;
        }

        private void SpawnGoldCoin(
            Vector3 position,
            int amount,
            string objectName)
        {
            if (amount <= 0)
            {
                return;
            }

            var coin = new GameObject(objectName);
            coin.transform.position = position;

            ProbCoin coinMono =
                coin.AddComponent<ProbCoin>();

            coinMono.SetGoldAmount(amount);
        }
    }
}