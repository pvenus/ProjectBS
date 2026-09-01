using System;
using System.Collections.Generic;
using Character;
using Character.Helper.Skill;
using Character.Runtime.Skill;
using Skill;
using UnityEngine;

namespace Battle.Presentation.SkillFocus
{
    [DisallowMultipleComponent]
    public sealed class MainCharacterSkillFocusFeature : MonoBehaviour
    {
        private const float PendingReceiptLifetime = 1.5f;
        private static MainCharacterSkillFocusFeature instance;
        private readonly Queue<float> playedTimes = new();
        private readonly Dictionary<int, PendingSkill> pendingByCaster = new();
        private MainCharacterSkillFocusProfileSO profile;
        private SkillFocusCameraControllerMono cameraController;
        private float globalEndTime;
        private float fatigueSuppressionEndTime;
        private int activeCasterId;
        private bool reducedMotion;
        private bool cameraMotionOff;

        private readonly struct PendingSkill
        {
            public PendingSkill(string skillId, float expiresAt)
            {
                SkillId = skillId;
                ExpiresAt = expiresAt;
            }

            public string SkillId { get; }
            public float ExpiresAt { get; }
        }

        public static MainCharacterSkillFocusFeature GetOrCreate(MonoBehaviour fallbackOwner)
        {
            if (instance != null) return instance;
            Camera camera = Camera.main;
            GameObject owner = camera != null ? camera.gameObject : fallbackOwner?.gameObject;
            if (owner == null) return null;
            instance = owner.GetComponent<MainCharacterSkillFocusFeature>()
                ?? owner.AddComponent<MainCharacterSkillFocusFeature>();
            return instance;
        }

        public void ConfigureAccessibility(bool reduceMotion, bool disableTimeDilation, bool disableCameraMotion)
        {
            reducedMotion = reduceMotion;
            cameraMotionOff = disableCameraMotion;
            if (reducedMotion || cameraMotionOff) cameraController?.RestoreImmediate();
        }

        public static bool IsEligible(EquipmentSkillRuntimeData runtime, Transform caster)
        {
            if (runtime == null || caster == null) return false;
            string skillId = CharacterSkillHelper.GetSkillId(runtime);
            if (!MainCharacterSkillFocusProfileSO.IsEligibleSkillId(skillId)
                || runtime.sourceEquipment?.BaseProfileSo == null
                || runtime.sourceEquipment.BaseProfileSo.SkillType != SkillType.Active)
            {
                return false;
            }

            CharacterManager manager = caster.GetComponent<CharacterManager>()
                ?? caster.GetComponentInChildren<CharacterManager>();
            return manager?.RuntimeData?.characterSO != null
                && manager.RuntimeData.characterSO.CharacterType == CharacterType.Player;
        }

        public static void NotifySkillExecuting(MonoBehaviour owner, EquipmentSkillRuntimeData runtime, Transform caster)
        {
            if (!IsEligible(runtime, caster)) return;
            MainCharacterSkillFocusFeature feature = GetOrCreate(owner);
            feature?.RegisterPending(caster, CharacterSkillHelper.GetSkillId(runtime));
        }

        public static void CancelPending(Transform caster)
        {
            if (instance == null || caster == null) return;
            instance.pendingByCaster.Remove(caster.GetInstanceID());
        }

        public static void NotifyProjectileImpact(ProjectileRuntimeData projectile)
        {
            if (instance == null || projectile?.owner == null || projectile.sourceEquipment == null) return;
            string skillId = projectile.sourceEquipment.EquipmentId;
            bool isCharge = IsSeojinChargeId(skillId) && projectile.spawnOrder == 0;
            bool isFinalBarrage = string.Equals(
                    skillId,
                    "skill.character.yujin.2.active_2.hwalbin_barrage",
                    StringComparison.Ordinal)
                && projectile.spawnOrder == Mathf.Max(0, projectile.projectileCount - 1);
            if (!isCharge && !isFinalBarrage) return;
            Vector2 direction = projectile.direction;
            SkillFocusCalibration calibration = instance.ResolveCalibration(skillId);
            Vector2 axis;
            if (direction.sqrMagnitude > .0001f)
            {
                direction.Normalize();
                axis = (new Vector2(-direction.y, direction.x) * .7f) - (direction * .3f);
            }
            else
            {
                axis = Vector2.right;
                calibration = new SkillFocusCalibration(
                    calibration.AmplitudePixels * .5f,
                    calibration.Duration,
                    calibration.Cycles);
            }
            instance.TryPlayReceipt(projectile.owner.transform, skillId, axis, calibration);
        }

        private static bool IsSeojinChargeId(string skillId)
        {
            return string.Equals(skillId, "skill.character.seojin.1.active_1.active_1", StringComparison.Ordinal)
                || string.Equals(skillId, "skill.character.seojin.2.active_1.charge", StringComparison.Ordinal)
                || string.Equals(skillId, "skill.character.seojin.3.active_1.charge", StringComparison.Ordinal);
        }

        public static void NotifyHealApplied(CharacterManager sourceCharacter)
        {
            if (instance == null || sourceCharacter == null) return;
            SkillFocusCalibration calibration = instance.ResolveCalibration(
                "skill.character.jihan.2.active_2.ten_tonic_soup");
            instance.TryPlayReceipt(
                sourceCharacter.transform,
                "skill.character.jihan.2.active_2.ten_tonic_soup",
                new Vector2(.3f, .7f),
                calibration);
        }

        private void RegisterPending(Transform caster, string skillId)
        {
            if (caster == null || string.IsNullOrEmpty(skillId)) return;
            pendingByCaster[caster.GetInstanceID()] = new PendingSkill(
                skillId,
                Time.unscaledTime + PendingReceiptLifetime);
        }

        private void TryPlayReceipt(
            Transform caster,
            string expectedSkillId,
            Vector2 axis,
            SkillFocusCalibration calibration)
        {
            if (caster == null || reducedMotion || cameraMotionOff || Time.timeScale <= 0f) return;
            int casterId = caster.GetInstanceID();
            if (!pendingByCaster.TryGetValue(casterId, out PendingSkill pending)
                || pending.ExpiresAt < Time.unscaledTime
                || !string.Equals(pending.SkillId, expectedSkillId, StringComparison.Ordinal))
            {
                return;
            }
            pendingByCaster.Remove(casterId);

            EnsureProfile();
            float now = Time.unscaledTime;
            TrimFatigue(now);
            if (cameraController != null && cameraController.IsActive) return;
            if (now < globalEndTime || now < fatigueSuppressionEndTime) return;
            if (playedTimes.Count >= profile.MaxFocusesPerWindow)
            {
                fatigueSuppressionEndTime = now + profile.FatigueSuppression;
                return;
            }

            Camera camera = Camera.main;
            if (camera == null || !camera.orthographic) return;
            cameraController = camera.GetComponent<SkillFocusCameraControllerMono>()
                ?? camera.gameObject.AddComponent<SkillFocusCameraControllerMono>();
            if (!cameraController.TryPlay(camera, calibration, axis)) return;

            activeCasterId = casterId;
            playedTimes.Enqueue(now);
            globalEndTime = now + profile.GlobalCooldown;
        }

        private void EnsureProfile()
        {
            if (profile == null) profile = MainCharacterSkillFocusProfileSO.CreateRuntimeDefault();
        }

        private SkillFocusCalibration ResolveCalibration(string skillId)
        {
            EnsureProfile();
            return profile.Resolve(skillId);
        }

        private void TrimFatigue(float now)
        {
            while (playedTimes.Count > 0 && now - playedTimes.Peek() >= profile.FatigueWindow)
                playedTimes.Dequeue();
        }

        private void OnEnable() => CharacterManager.OnAnyCharacterDied += HandleCharacterDied;

        private void Update()
        {
            if (Time.timeScale <= 0f) cameraController?.RestoreImmediate();
        }

        private void HandleCharacterDied(CharacterManager character)
        {
            if (character == null) return;
            int id = character.transform.GetInstanceID();
            pendingByCaster.Remove(id);
            if (id == activeCasterId) cameraController?.RestoreImmediate();
        }

        private void RestoreAll()
        {
            cameraController?.RestoreImmediate();
            pendingByCaster.Clear();
            activeCasterId = 0;
        }

        private void OnDisable()
        {
            CharacterManager.OnAnyCharacterDied -= HandleCharacterDied;
            RestoreAll();
        }

        private void OnDestroy()
        {
            RestoreAll();
            if (profile != null && (profile.hideFlags & HideFlags.DontSave) != 0) Destroy(profile);
            if (ReferenceEquals(instance, this)) instance = null;
        }
    }
}
