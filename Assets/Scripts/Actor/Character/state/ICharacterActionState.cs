namespace Character
{
    /// <summary>
    /// Character action state.
    ///
    /// The character AI decides which action state to enter.
    /// Once selected, the state owns the actual behavior until it finishes,
    /// fails, or the controller decides to transition to another state.
    /// </summary>
    public interface ICharacterActionState
    {
        void Enter(CharacterActionContext context);

        void Tick(CharacterActionContext context, float deltaTime);

        void Exit(CharacterActionContext context);

        bool IsFinished { get; }
    }
}