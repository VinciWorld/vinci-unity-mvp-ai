using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class ArenaState : StateBase
{
    IdleGameController _controller;

    public ArenaState(IdleGameController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        ArenaView arenaView = ViewManager.GetView<ArenaView>();
        ViewManager.Show(arenaView);
    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }



}