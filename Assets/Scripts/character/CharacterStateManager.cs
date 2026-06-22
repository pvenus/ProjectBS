using UnityEngine;
using Character.Skill;

namespace Character
{
    /// <summary>
    /// Main MonoBehaviour that owns and executes the current character action state.
    ///
    /// The decision layer decides which state to enter.
    /// This manager only handles state lifecycle:
    /// - Exit previous state
    /// - Enter next state
    /// - Tick current state
    /// </summary>
    public class CharacterStateManager : MonoBehaviour
    {
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private bool debugStateLog = true;
        private ICharacterActionState _currentState;
        private CharacterActionContext _context;
        private CharacterDecisionEngine _decisionEngine;
        private void Awake()
        {
            MovementController movementController =
                GetComponent<MovementController>()
                ?? GetComponentInChildren<MovementController>();

            if (movementController == null)
            {
                movementController = gameObject.AddComponent<MovementController>();
            }

            SkillExecutorMono skillExecutor =
                GetComponent<SkillExecutorMono>()
                ?? GetComponentInChildren<SkillExecutorMono>();

            CharacterSkillManager skillManager =
                GetComponent<CharacterSkillManager>()
                ?? GetComponentInChildren<CharacterSkillManager>();

            _context = new CharacterActionContext
            {
                Owner = gameObject,
                OwnerTransform = transform,
                CharacterManager = GetComponent<CharacterManager>(),
                StateManager = this,
                SkillExecutor = skillExecutor,
                SkillManager = skillManager,
                MovementController = movementController,
                MovementExecutionService = new CharacterMovementExecutionService(),
                AnimationMono = null
            };

            _decisionEngine = new CharacterDecisionEngine(targetMask);
        }

        private void Start()
        {
            ResolveLateComponents();
            CharacterManager characterManager = _context.CharacterManager;

            if (characterManager?.RuntimeData?.characterSO == null)
            {
                return;
            }

            //if (characterManager.RuntimeData.characterSO.CharacterType == CharacterType.Player)
            {
                _context.SkillExecutor?.SetManualExecutionMode(true);
            }
        }

        private void ResolveLateComponents()
        {
            if (_context == null)
            {
                return;
            }

            _context.AnimationMono =
                GetComponent<AnimationMono>()
                ?? GetComponentInChildren<AnimationMono>();

            if (_context.SkillExecutor == null)
            {
                _context.SkillExecutor =
                    GetComponent<SkillExecutorMono>()
                    ?? GetComponentInChildren<SkillExecutorMono>();
            }

            if (_context.SkillManager == null)
            {
                _context.SkillManager =
                    GetComponent<CharacterSkillManager>()
                    ?? GetComponentInChildren<CharacterSkillManager>();
            }

            if (_context.MovementController == null)
            {
                _context.MovementController =
                    GetComponent<MovementController>()
                    ?? GetComponentInChildren<MovementController>();
            }

            if (_context.CharacterManager == null)
            {
                _context.CharacterManager =
                    GetComponent<CharacterManager>()
                    ?? GetComponentInChildren<CharacterManager>();
            }
        }

        private void Update()
        {
            ResolveLateComponents();
            _context.DeltaTime = Time.deltaTime;
            TryGetForcedTarget(out _);
            TryGetLurePoint(out _);

            if (_currentState == null || _currentState.IsFinished)
            {
                DecideAndChangeState();
            }

            if (_currentState == null)
            {
                return;
            }

            _currentState.Tick(
                _context,
                Time.deltaTime);
        }

        private void DecideAndChangeState()
        {
            if (_decisionEngine == null)
            {
                _decisionEngine = new CharacterDecisionEngine(targetMask);
            }

            ICharacterActionState nextState =
                _decisionEngine.Decide(_context);
            LogStateMessage(
                $"Decision result: {GetStateName(nextState)} " +
                $"Current={GetStateName(_currentState)} " +
                $"Target={GetTargetName(_context.CurrentTarget)}");

            if (nextState == null)
            {
                return;
            }

            if (_context.SkillManager != null &&
                _context.SkillManager.IsSkillExecuting &&
                _currentState != null)
            {
                LogStateMessage(
                    $"Decision blocked: skill executing Current={GetStateName(_currentState)}");
                return;
            }

            ChangeState(nextState);
        }

        public void ChangeState(ICharacterActionState nextState)
        {
            if (_currentState == nextState)
            {
                return;
            }

            LogStateMessage(
                $"ChangeState: {GetStateName(_currentState)} -> {GetStateName(nextState)} " +
                $"Target={GetTargetName(_context.CurrentTarget)}");
            _currentState?.Exit(_context);
            _currentState = nextState;
            _currentState?.Enter(_context);
        }

        public void ClearState()
        {
            if (_currentState == null)
            {
                return;
            }

            LogStateMessage(
                $"ClearState: {GetStateName(_currentState)} " +
                $"Target={GetTargetName(_context.CurrentTarget)}");
            _currentState.Exit(_context);
            _currentState = null;
        }

        public bool IsCurrentStateFinished()
        {
            return _currentState != null && _currentState.IsFinished;
        }

        public void ApplyForcedTarget(
            Transform target,
            float duration)
        {
            _context?.ApplyForcedTarget(
                target,
                duration);
        }

        public void ClearForcedTarget()
        {
            _context?.ClearForcedTarget();
        }

        public void ClearForcedTarget(
            Transform target)
        {
            if (_context == null || _context.ForcedTarget != target)
            {
                return;
            }

            _context.ClearForcedTarget();
        }

        public bool TryGetForcedTarget(out Transform target)
        {
            if (_context == null)
            {
                target = null;
                return false;
            }

            return _context.TryGetForcedTarget(out target);
        }

        public bool IsForcedTarget(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            return TryGetForcedTarget(out Transform forcedTarget)
                   && forcedTarget == target;
        }

        public void ApplyLurePoint(
            Vector2 lurePoint,
            float duration,
            float lureRadius,
            float lureMoveSpeedMultiplier)
        {
            _context?.ApplyLurePoint(
                lurePoint,
                duration,
                lureRadius,
                lureMoveSpeedMultiplier);
        }

        public void ClearLurePoint()
        {
            _context?.ClearLurePoint();
        }

        public bool TryGetLurePoint(out Vector2 lurePoint)
        {
            if (_context == null)
            {
                lurePoint = Vector2.zero;
                return false;
            }

            return _context.TryGetLurePoint(out lurePoint);
        }

        public void LogStateMessage(string message)
        {
            if (!debugStateLog)
            {
                return;
            }

            Debug.Log(
                $"[CharacterState] {name} {message}",
                this);
        }

        private static string GetStateName(ICharacterActionState state)
        {
            return state == null
                ? "null"
                : state.GetType().Name;
        }

        private static string GetTargetName(Transform target)
        {
            return target == null
                ? "null"
                : target.name;
        }
    }
}
