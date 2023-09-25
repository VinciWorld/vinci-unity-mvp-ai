using UnityEngine;
using Vinci.Core.StateMachine;

public class IdleGameState : StateBase
{
    GameManager _manager;

    public IdleGameState(GameManager manager)
    {
        _manager = manager;
    }

    public override void OnEnterState()
    {
        
    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }
}