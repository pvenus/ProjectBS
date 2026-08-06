using System;
using Battle.Prop.SO;
using Battle;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Battle.Prop
{
    public class BattlePropController : MonoBehaviour
    {
        [SerializeField]
        private string runtimeId;

        [SerializeField]
        private BattlePropRole role = BattlePropRole.None;

        [SerializeField]
        private Animator animator;

        private PlayableGraph animationGraph;
        private AnimationClipPlayable currentClipPlayable;

        private BattlePropSO propSO;
        private BattlePropState currentState = BattlePropState.None;

        [SerializeField]
        private int hitCount;

        private bool spawnOnHitCompleted;

        private SpawnSequenceRunner spawnSequenceRunner;
        private SpawnSequenceRuntime spawnSequenceRuntime;

        public string RuntimeId => runtimeId;
        public BattlePropRole Role => role;
        public int HitCount => hitCount;

        public bool IsTargetable()
        {
            if (propSO == null)
            {
                return true;
            }

            if (propSO.spawnPropOnHit == null)
            {
                return true;
            }

            int threshold = Mathf.Max(1, propSO.spawnHitThreshold);
            return hitCount < threshold;
        }

        private void Awake()
        {
            if (string.IsNullOrEmpty(runtimeId))
            {
                runtimeId = Guid.NewGuid().ToString("N");
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }
        private void OnDestroy()
        {
            StopSpawnSequence();
            DestroyAnimationGraph();
        }

        private void Update()
        {
            if (spawnSequenceRunner != null &&
                spawnSequenceRuntime != null &&
                spawnSequenceRuntime.IsRunning)
            {
                spawnSequenceRunner.Tick(Time.deltaTime);
            }
        }

        private void Start()
        {
            SetState(BattlePropState.Normal);
        }

        private void OnEnable()
        {
            BattlePropManager.Instance?.Register(this);
        }

        private void OnDisable()
        {
            BattlePropManager.Instance?.Unregister(this);
        }

        public void Initialize(
            string id,
            BattlePropSO sourcePropSO)
        {
            propSO = sourcePropSO;

            runtimeId = string.IsNullOrEmpty(id)
                ? Guid.NewGuid().ToString("N")
                : id;

            role = propSO != null
                ? propSO.role
                : BattlePropRole.None;

            hitCount = 0;
            spawnOnHitCompleted = false;

            SetState(BattlePropState.Normal);
            TryPlaySpawnSequence();
        }

        public void OnProjectileHit()
        {
            hitCount++;

            TrySpawnPropOnHitThreshold();
        }

        private void TrySpawnPropOnHitThreshold()
        {
            if (spawnOnHitCompleted || propSO == null)
            {
                return;
            }

            if (propSO.spawnPropOnHit == null)
            {
                return;
            }

            int threshold = Mathf.Max(1, propSO.spawnHitThreshold);

            if (hitCount < threshold)
            {
                return;
            }

            spawnOnHitCompleted = true;

            if (BattlePropManager.Instance != null)
            {
                BattlePropManager.Instance.SpawnProp(
                    propSO.spawnPropOnHit,
                    transform.position,
                    transform.rotation,
                    $"{runtimeId}_spawned");
            }

            if (propSO.destroyAfterSpawnOnHit)
            {
                Destroy(gameObject);
            }
        }

        private void TryPlaySpawnSequence()
        {
            if (propSO == null || !propSO.playSpawnSequenceOnInitialize)
            {
                return;
            }

            if (propSO.spawnSequence == null)
            {
                return;
            }

            StopSpawnSequence();

            spawnSequenceRuntime =
                new SpawnSequenceRuntime(propSO.spawnSequence);

            for (int i = 0; i < spawnSequenceRuntime.StepRuntimes.Count; i++)
            {
                SpawnContentRuntime stepRuntime =
                    spawnSequenceRuntime.StepRuntimes[i];

                if (stepRuntime != null)
                {
                    stepRuntime.AnchorPosition = transform.position;
                }
            }

            spawnSequenceRunner = new SpawnSequenceRunner();
            spawnSequenceRunner.StartSequence(
                spawnSequenceRuntime,
                () => Debug.Log($"[BattlePropController] SpawnSequence completed. runtimeId={runtimeId}"));
        }

        private void StopSpawnSequence()
        {
            if (spawnSequenceRunner != null)
            {
                spawnSequenceRunner.StopSequence();
                spawnSequenceRunner = null;
            }

            spawnSequenceRuntime = null;
        }

        public void SetState(BattlePropState state)
        {
            currentState = state;

            BattlePropSO.PropStateVisualEntry visual =
                FindStateVisual(state);

            if (visual == null)
            {
                return;
            }

            if (visual.animationClip != null)
            {
                PlayClip(visual.animationClip);
            }

            if (visual.effectPrefab != null)
            {
                Instantiate(
                    visual.effectPrefab,
                    transform.position,
                    transform.rotation,
                    transform);
            }
        }

        private void PlayClip(AnimationClip clip)
        {
            if (clip == null || animator == null)
            {
                return;
            }

            DestroyAnimationGraph();

            animationGraph = PlayableGraph.Create(
                $"BattleProp_{runtimeId}_{clip.name}");
            animationGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            currentClipPlayable = AnimationClipPlayable.Create(
                animationGraph,
                clip);
            currentClipPlayable.SetApplyFootIK(false);

            AnimationPlayableOutput output =
                AnimationPlayableOutput.Create(
                    animationGraph,
                    "Animation",
                    animator);
            output.SetSourcePlayable(currentClipPlayable);

            animationGraph.Play();
        }

        private void DestroyAnimationGraph()
        {
            if (animationGraph.IsValid())
            {
                animationGraph.Destroy();
            }
        }

        private BattlePropSO.PropStateVisualEntry FindStateVisual(
            BattlePropState state)
        {
            if (propSO == null || propSO.stateVisuals == null)
            {
                return null;
            }

            for (int i = 0; i < propSO.stateVisuals.Count; i++)
            {
                BattlePropSO.PropStateVisualEntry visual =
                    propSO.stateVisuals[i];

                if (visual != null && visual.state == state)
                {
                    return visual;
                }
            }

            return null;
        }
    }
}
