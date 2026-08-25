using System.Collections.Generic;
using Character;
using Character.UI;
using Party.UI;
using UnityEngine;

namespace Battle.Presentation
{
    public static class BattleCharacterAuraInstaller
    {
        private const string PlayerAuraResourcePath =
            "battle/Presentation/PlayerCharacterAura";

        private static BattleCharacterAuraView playerAuraPrefab;
        private static bool hasLoadedPlayerAuraPrefab;

        public static BattleCharacterAuraView EnsureFor(
            CharacterManager characterManager)
        {
            return EnsureForInternal(
                characterManager,
                false,
                default);
        }

        public static BattleCharacterAuraView EnsureFor(
            CharacterManager characterManager,
            Color color)
        {
            return EnsureForInternal(
                characterManager,
                true,
                color);
        }

        private static BattleCharacterAuraView EnsureForInternal(
            CharacterManager characterManager,
            bool applyColor,
            Color color)
        {
            if (!IsPlayerCharacter(characterManager))
            {
                return null;
            }

            BattleCharacterAuraBinding existingBinding =
                characterManager.GetComponent<BattleCharacterAuraBinding>();

            if (existingBinding != null
                && existingBinding.AuraView != null)
            {
                return ApplyColor(
                    existingBinding.AuraView,
                    applyColor,
                    color);
            }

            BattleCharacterAuraView existingChild =
                characterManager.GetComponentInChildren<
                    BattleCharacterAuraView>(true);

            if (existingChild != null)
            {
                BattleCharacterAuraBinding recoveredBinding =
                    existingBinding != null
                        ? existingBinding
                        : characterManager.gameObject
                            .AddComponent<BattleCharacterAuraBinding>();
                recoveredBinding.Bind(existingChild);
                return ApplyColor(
                    existingChild,
                    applyColor,
                    color);
            }

            BattleCharacterAuraView auraPrefab =
                GetPlayerAuraPrefab(characterManager);

            if (auraPrefab == null)
            {
                return null;
            }

            SpriteRenderer[] characterRenderers =
                ResolveCharacterRenderers(characterManager);
            BattleCharacterAuraView auraView =
                Object.Instantiate(
                    auraPrefab,
                    characterManager.transform,
                    false);
            auraView.name =
                $"{characterManager.name}_PlayerCharacterAura";
            auraView.Initialize(characterRenderers);
            ApplyColor(
                auraView,
                applyColor,
                color);

            BattleCharacterAuraBinding binding =
                existingBinding != null
                    ? existingBinding
                    : characterManager.gameObject
                        .AddComponent<BattleCharacterAuraBinding>();
            binding.Bind(auraView);

            return auraView;
        }

        private static BattleCharacterAuraView ApplyColor(
            BattleCharacterAuraView auraView,
            bool applyColor,
            Color color)
        {
            if (applyColor && auraView != null)
            {
                auraView.SetColor(color);
            }

            return auraView;
        }

        private static BattleCharacterAuraView GetPlayerAuraPrefab(
            CharacterManager characterManager)
        {
            if (hasLoadedPlayerAuraPrefab)
            {
                return playerAuraPrefab;
            }

            hasLoadedPlayerAuraPrefab = true;
            playerAuraPrefab =
                Resources.Load<BattleCharacterAuraView>(
                    PlayerAuraResourcePath);

            if (playerAuraPrefab == null)
            {
                Debug.LogWarning(
                    $"[BattleCharacterAuraInstaller] Player aura prefab was not found at Resources/{PlayerAuraResourcePath}.",
                    characterManager);
            }

            return playerAuraPrefab;
        }

        private static bool IsPlayerCharacter(
            CharacterManager characterManager)
        {
            return characterManager != null
                && characterManager.RuntimeData != null
                && characterManager.RuntimeData.characterSO != null
                && characterManager.RuntimeData.characterSO.CharacterType
                    == CharacterType.Player;
        }

        private static SpriteRenderer[] ResolveCharacterRenderers(
            CharacterManager characterManager)
        {
            SpriteRenderer[] childRenderers =
                characterManager.GetComponentsInChildren<SpriteRenderer>(true);
            SpriteRenderer referenceRenderer =
                characterManager.GetComponent<SpriteRenderer>();

            if (referenceRenderer == null)
            {
                referenceRenderer = FindFirstNonUiRenderer(childRenderers);
            }

            if (referenceRenderer == null)
            {
                return new SpriteRenderer[0];
            }

            int characterSortingLayerId =
                referenceRenderer.sortingLayerID;
            List<SpriteRenderer> characterRenderers = new();

            for (int i = 0; i < childRenderers.Length; i++)
            {
                SpriteRenderer renderer = childRenderers[i];

                if (renderer != null
                    && renderer.sortingLayerID
                        == characterSortingLayerId
                    && IsCharacterBodyRenderer(renderer))
                {
                    characterRenderers.Add(renderer);
                }
            }

            return characterRenderers.ToArray();
        }

        private static bool IsCharacterBodyRenderer(
            SpriteRenderer renderer)
        {
            return renderer.GetComponentInParent<CharacterBattleHudUI>()
                    == null
                && renderer.GetComponentInParent<CharacterSkillCooldownUI>()
                    == null
                && renderer.GetComponentInParent<BattleCharacterAuraView>()
                    == null;
        }

        private static SpriteRenderer FindFirstNonUiRenderer(
            SpriteRenderer[] renderers)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];

                if (renderer != null
                    && renderer.sortingLayerName != "UI")
                {
                    return renderer;
                }
            }

            return null;
        }
    }
}
