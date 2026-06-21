using UnityEngine;

public class PlayerStatesMachine
{
    public PlayerStates currentStates { get; private set; }

    public void Initialize(PlayerStates _startstates)
    {
        currentStates = _startstates;
        currentStates.Enter();
    }

    public void ChangeStates(PlayerStates _newstates)
    {
        currentStates.Exit();
        currentStates = _newstates;
        currentStates.Enter();

    }

}
