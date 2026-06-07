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

        public ICharacterActionState CurrentState => _currentState;

        public bool HasState => _currentState != null;

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

            AnimationMono animationMono =
                GetComponent<AnimationMono>()
                ?? GetComponentInChildren<AnimationMono>();

            _context = new CharacterActionContext
            {
                Owner = gameObject,
                OwnerTransform = transform,
                CharacterManager = GetComponent<CharacterManager>(),
                StateManager = this,
                SkillExecutor = skillExecutor,
                MovementController = movementController,
                MovementExecutionService = new CharacterMovementExecutionService(),
                AnimationMono = animationMono
            };

            _decisionEngine = new CharacterDecisionEngine(targetMask);
        }

        private void Start()
        {
            CharacterManager characterManager = _context.CharacterManager;

            if (characterManager?.RuntimeData?.characterSO == null)
            {
                return;
            }

            if (characterManager.RuntimeData.characterSO.characterType == CharacterType.Player)
            {
                _context.SkillExecutor?.SetManualExecutionMode(true);
            }
        }

        private void Update()
        {
            if (_context.CharacterManager?.RuntimeData.characterSO.characterType != CharacterType.Player)            
            {
                return;
            }

            _context.DeltaTime = Time.deltaTime;

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
