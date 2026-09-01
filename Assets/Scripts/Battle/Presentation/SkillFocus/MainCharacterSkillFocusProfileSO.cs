using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Presentation.SkillFocus
{
    [CreateAssetMenu(fileName = "Main3SkillFocusProfile", menuName = "BS/Battle/Presentation/Main3 Skill Focus Profile")]
    public sealed class MainCharacterSkillFocusProfileSO : ScriptableObject
    {
        private static readonly HashSet<string> EligibleSkillIds = new(StringComparer.Ordinal)
        {
            "skill.character.seojin.1.active_1.active_1",
            "skill.character.seojin.2.active_1.charge",
            "skill.character.seojin.2.active_2.crane_wing_formation",
            "skill.character.seojin.3.active_1.charge",
            "skill.character.seojin.3.active_2.crane_wing_formation",
            "skill.character.seojin.3.active_3.turtle_ship_assault",
            "skill.character.jihan.1.active_1.medicine_prescription",
            "skill.character.jihan.2.active_1.medicine_prescription",
            "skill.character.jihan.2.active_2.ten_tonic_soup",
            "skill.character.jihan.3.active_1.medicine_prescription",
            "skill.character.jihan.3.active_2.ten_tonic_soup",
            "skill.character.jihan.3.active_3.divine_acupuncture",
            "skill.character.yujin.1.active_1.multi_shot",
            "skill.character.yujin.2.active_1.multi_shot",
            "skill.character.yujin.2.active_2.hwalbin_barrage",
            "skill.character.yujin.3.active_1.multi_shot",
            "skill.character.yujin.3.active_2.hwalbin_barrage",
            "skill.character.yujin.3.active_3.outlaw_appearance"
        };

        [SerializeField] private float globalCooldown = .35f;
        [SerializeField] private float sameCasterCooldown = .35f;
        [SerializeField] private float fatigueWindow = 2f;
        [SerializeField] private int maxFocusesPerWindow = 3;
        [SerializeField] private float fatigueSuppression = 1f;

        public float GlobalCooldown => globalCooldown;
        public float SameCasterCooldown => sameCasterCooldown;
        public float FatigueWindow => fatigueWindow;
        public int MaxFocusesPerWindow => maxFocusesPerWindow;
        public float FatigueSuppression => fatigueSuppression;

        public bool Contains(string skillId) => !string.IsNullOrEmpty(skillId) && EligibleSkillIds.Contains(skillId);
        public static bool IsEligibleSkillId(string skillId) => !string.IsNullOrEmpty(skillId) && EligibleSkillIds.Contains(skillId);
        public static int EligibleSkillCount => EligibleSkillIds.Count;

        public SkillFocusCalibration Resolve(string skillId)
        {
            if (string.Equals(skillId, "skill.character.yujin.2.active_2.hwalbin_barrage", StringComparison.Ordinal))
                return new SkillFocusCalibration(3f, .140f, 2f);
            if (string.Equals(skillId, "skill.character.jihan.2.active_2.ten_tonic_soup", StringComparison.Ordinal))
                return new SkillFocusCalibration(2.5f, .140f, 2f);

            int grade = ResolveGrade(skillId);
            if (grade <= 1) return new SkillFocusCalibration(3f, .140f, 2f);
            if (grade == 2) return new SkillFocusCalibration(4f, .160f, 2.5f);
            return new SkillFocusCalibration(5f, .180f, 3f);
        }

        private static int ResolveGrade(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return 1;
            if (skillId.Contains(".3.", StringComparison.Ordinal)) return 3;
            if (skillId.Contains(".2.", StringComparison.Ordinal)) return 2;
            return 1;
        }

        public static MainCharacterSkillFocusProfileSO CreateRuntimeDefault()
        {
            MainCharacterSkillFocusProfileSO profile = CreateInstance<MainCharacterSkillFocusProfileSO>();
            profile.hideFlags = HideFlags.HideAndDontSave;
            return profile;
        }
    }

    public readonly struct SkillFocusCalibration
    {
        public SkillFocusCalibration(float amplitudePixels, float duration, float cycles)
        {
            AmplitudePixels = Mathf.Clamp(amplitudePixels, 0f, 6f);
            Duration = Mathf.Clamp(duration, 0f, .19f);
            Cycles = Mathf.Clamp(cycles, 0f, 3f);
            Normalization = ResolveNormalization(Cycles);
        }

        public float AmplitudePixels { get; }
        public float Duration { get; }
        public float Cycles { get; }
        public float Normalization { get; }

        private static float ResolveNormalization(float cycles)
        {
            if (cycles <= 2.0001f) return 1.1886652f;
            if (cycles <= 2.5001f) return 1.1476477f;
            return 1.1212341f;
        }
    }
}
