using System;
using UnityEngine;
using Party.UI;
using Character.UI;

namespace Character.Helper
{
    public static class CharacterBuilder
    {
        private const string EnemyLayerName = "Enemy";
        private const string PartyLayerName = "Party";
        private const float EnemyColliderRadius = 0.2f;
        private const float PartyColliderRadius = 0.1f;

        public static GameObject CreateNpcObject(
            string objectName = "Npc",
            Transform parent = null,
            Sprite sprite = null)
        {
            return CreateCharacterObject(
                objectName,
                parent,
                Vector3.zero,
                Quaternion.identity,
                EnemyLayerName,
                sprite,
                true);
        }

        public static GameObject CreateNpcObject(
            string objectName,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            string layerName,
            Sprite sprite,
            bool includeCollider)
        {
            return CreateCharacterObject(
                objectName,
                parent,
                position,
                rotation,
                layerName,
                sprite,
                includeCollider);
        }

        public static GameObject CreatePlayerObject(
            string objectName = "Player",
            Transform parent = null,
            Sprite sprite = null)
        {
            return CreateCharacterObject(
                objectName,
                parent,
                Vector3.zero,
                Quaternion.identity,
                PartyLayerName,
                sprite,
                true);
        }

        public static GameObject CreatePlayerObject(
            string objectName,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Sprite sprite,
            bool includeCollider)
        {
            return CreateCharacterObject(
                objectName,
                parent,
                position,
                rotation,
                PartyLayerName,
                sprite,
                includeCollider);
        }

        public static GameObject CreateOrBuildNpcObject(
            GameObject prefab,
            string objectName,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            string layerName,
            Sprite sprite,
            bool includeCollider)
        {
            GameObject target = prefab != null
                ? UnityEngine.Object.Instantiate(prefab, position, rotation, parent)
                : CreateRawObject(objectName, parent);

            SetupTransform(
                target,
                position,
                rotation);

            BuildCharacter(
                target,
                layerName,
                sprite,
                includeCollider);

            return target;
        }

        public static GameObject CreateOrBuildPlayerObject(
            GameObject prefab,
            string objectName,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Sprite sprite,
            bool includeCollider)
        {
            return CreateOrBuildNpcObject(
                prefab,
                objectName,
                parent,
                position,
                rotation,
                PartyLayerName,
                sprite,
                includeCollider);
        }

        public static void BuildNpc(GameObject target)
        {
            BuildCharacter(
                target,
                EnemyLayerName,
                null,
                true);
        }

        public static void BuildNpc(
            GameObject target,
            string layerName,
            Sprite sprite,
            bool includeCollider)
        {
            BuildCharacter(
                target,
                layerName,
                sprite,
                includeCollider);
        }

        public static void BuildPlayer(GameObject target)
        {
            BuildPlayer(
                target,
                null,
                true);
        }

        public static void BuildPlayer(
            GameObject target,
            Sprite sprite,
            bool includeCollider)
        {
            BuildCharacter(
                target,
                PartyLayerName,
                sprite,
                includeCollider);
        }

        public static T EnsureComponent<T>(GameObject target)
            where T : Component
        {
            if (target == null)
            {
                return null;
            }

            T component = target.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            return target.AddComponent<T>();
        }

        private static GameObject CreateCharacterObject(
            string objectName,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            string layerName,
            Sprite sprite,
            bool includeCollider)
        {
            GameObject target = CreateRawObject(objectName, parent);

            SetupTransform(
                target,
                position,
                rotation);

            BuildCharacter(
                target,
                layerName,
                sprite,
                includeCollider);

            return target;
        }

        private static GameObject CreateRawObject(
            string objectName,
            Transform parent)
        {
            GameObject target = new GameObject(
                string.IsNullOrWhiteSpace(objectName)
                    ? "Character"
                    : objectName);

            if (parent != null)
            {
                target.transform.SetParent(parent, false);
            }

            return target;
        }

        private static void BuildCharacter(
            GameObject target,
            string layerName,
            Sprite sprite,
            bool includeCollider)
        {
            if (target == null)
            {
                return;
            }

            SetLayer(target, layerName);
            target.transform.localScale = Vector3.one;

            SetupRigidbody(
                target,
                layerName);

            if (includeCollider)
            {
                SetupCollider(
                    target,
                    layerName);
            }

            SetupSpriteRenderer(
                target,
                sprite);

            CharacterManager characterManager =
                EnsureComponent<CharacterManager>(target);

            CharacterBattleHudUI.EnsureFor(characterManager);

            if (IsPartyLayer(layerName))
            {
                CharacterSkillCooldownUI.EnsureFor(characterManager);
            }
        }

        private static void SetupTransform(
            GameObject target,
            Vector3 position,
            Quaternion rotation)
        {
            if (target == null)
            {
                return;
            }

            target.transform.position = position;
            target.transform.rotation = rotation;
            target.transform.localScale = Vector3.one;
        }

        private static void SetupRigidbody(
            GameObject target,
            string layerName)
        {
            Rigidbody2D rigidbody = EnsureComponent<Rigidbody2D>(target);
            rigidbody.gravityScale = 0f;
            rigidbody.mass = ResolveRigidbodyMass(layerName);
            rigidbody.linearDamping = ResolveRigidbodyLinearDamping(layerName);
            rigidbody.freezeRotation = true;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private static void SetupCollider(
            GameObject target,
            string layerName)
        {
            CircleCollider2D circleCollider = EnsureComponent<CircleCollider2D>(target);
            circleCollider.isTrigger = false;
            circleCollider.radius = ResolveColliderRadius(layerName);
        }

        private static void SetupSpriteRenderer(
            GameObject target,
            Sprite sprite)
        {
            SpriteRenderer spriteRenderer = EnsureComponent<SpriteRenderer>(target);
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }

        private static float ResolveColliderRadius(string layerName)
        {
            return IsPartyLayer(layerName)
                ? PartyColliderRadius
                : EnemyColliderRadius;
        }

        private static float ResolveRigidbodyMass(string layerName)
        {
            return IsPartyLayer(layerName)
                ? 5f
                : 1f;
        }

        private static float ResolveRigidbodyLinearDamping(string layerName)
        {
            return IsPartyLayer(layerName)
                ? 4f
                : 0f;
        }

        private static bool IsPartyLayer(string layerName)
        {
            return string.Equals(layerName, PartyLayerName, StringComparison.Ordinal);
        }

        private static void SetLayer(GameObject target, string layerName)
        {
            if (target == null || string.IsNullOrWhiteSpace(layerName))
            {
                return;
            }

            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                Debug.LogWarning($"[CharacterBuilder] Layer not found. layerName={layerName}", target);
                return;
            }

            target.layer = layer;
        }
    }
}