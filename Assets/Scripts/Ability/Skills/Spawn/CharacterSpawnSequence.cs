using System.Collections;
using Character;
using Character.Helper;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Skill
{
    /// <summary>
    /// 투사체를 사용하지 않는 캐릭터 소환의 전체 수명주기를 담당한다.
    /// 소환 연출 재생 -> 캐릭터 생성 -> 지정 시간 후 소환 해제 순서로 처리한다.
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterSpawnSequence : MonoBehaviour
    {
        private const float DefaultVisualScale = 0.25f;

        private SpriteRenderer visualRenderer;
        private Animator visualAnimator;
        private GameObject spawnedCharacter;
        private Coroutine sequenceRoutine;
        private PlayableGraph visualGraph;

        public void Initialize(
            CharacterSO characterSo,
            BaseVisualSO baseVisual,
            Vector3 spawnPosition,
            Quaternion spawnRotation,
            float spawnLifeTime,
            float visualScale)
        {
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            visualRenderer = gameObject.AddComponent<SpriteRenderer>();
            visualAnimator = gameObject.AddComponent<Animator>();
            transform.localScale = Vector3.one *
                Mathf.Max(0.01f, visualScale * DefaultVisualScale);

            AnimationClip summonClip = ResolveSummonClip(baseVisual);
            sequenceRoutine = StartCoroutine(
                RunSequence(
                    characterSo,
                    summonClip,
                    spawnPosition,
                    spawnRotation,
                    Mathf.Max(0f, spawnLifeTime)));
        }

        private IEnumerator RunSequence(
            CharacterSO characterSo,
            AnimationClip summonClip,
            Vector3 spawnPosition,
            Quaternion spawnRotation,
            float spawnLifeTime)
        {
            if (summonClip != null)
            {
                yield return PlayClipOnce(summonClip);
            }

            if (visualRenderer != null)
            {
                visualRenderer.enabled = false;
            }

            spawnedCharacter = CharacterBuilder.CreateOrBuildPlayerObject(
                null,
                characterSo.name,
                null,
                spawnPosition,
                spawnRotation,
                null,
                true);

            CharacterManager characterManager = spawnedCharacter != null
                ? spawnedCharacter.GetComponent<CharacterManager>()
                : null;

            characterManager?.InitializeFromSO(characterSo);

            if (spawnedCharacter == null)
            {
                Destroy(gameObject);
                yield break;
            }

            if (spawnLifeTime > 0f)
            {
                yield return new WaitForSeconds(spawnLifeTime);
                Destroy(spawnedCharacter);
            }

            sequenceRoutine = null;
            Destroy(gameObject);
        }

        private IEnumerator PlayClipOnce(AnimationClip clip)
        {
            if (clip == null || visualAnimator == null)
            {
                yield break;
            }

            DestroyVisualGraph();

            visualGraph = PlayableGraph.Create(
                $"CharacterSpawnSequence_{clip.name}");
            visualGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            AnimationClipPlayable clipPlayable =
                AnimationClipPlayable.Create(visualGraph, clip);
            clipPlayable.SetApplyFootIK(false);
            clipPlayable.SetApplyPlayableIK(false);
            clipPlayable.SetTime(0d);

            AnimationPlayableOutput output =
                AnimationPlayableOutput.Create(
                    visualGraph,
                    "CharacterSpawnVisual",
                    visualAnimator);
            output.SetSourcePlayable(clipPlayable);

            visualGraph.Play();
            visualGraph.Evaluate(0f);

            yield return new WaitForSeconds(
                Mathf.Max(0.01f, clip.length));

            DestroyVisualGraph();
        }

        private void DestroyVisualGraph()
        {
            if (visualGraph.IsValid())
            {
                visualGraph.Destroy();
            }
        }

        private static AnimationClip ResolveSummonClip(BaseVisualSO baseVisual)
        {
            if (baseVisual == null || baseVisual.AnimationClips == null)
            {
                return null;
            }

            SkillAnimationClipType[] priorities =
            {
                SkillAnimationClipType.Cast,
                SkillAnimationClipType.ProjectileLoop,
                SkillAnimationClipType.Idle,
                SkillAnimationClipType.Attack,
                SkillAnimationClipType.Hit
            };

            for (int priorityIndex = 0; priorityIndex < priorities.Length; priorityIndex++)
            {
                for (int clipIndex = 0; clipIndex < baseVisual.AnimationClips.Length; clipIndex++)
                {
                    AnimationClipEntry entry = baseVisual.AnimationClips[clipIndex];
                    if (entry != null &&
                        entry.ClipType == priorities[priorityIndex] &&
                        entry.Clip != null)
                    {
                        return entry.Clip;
                    }
                }
            }

            return null;
        }

        private void OnDestroy()
        {
            DestroyVisualGraph();

            if (sequenceRoutine != null)
            {
                StopCoroutine(sequenceRoutine);
                sequenceRoutine = null;
            }
        }
    }
}
