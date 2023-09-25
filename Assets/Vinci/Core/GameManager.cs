using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vinci.Core.StateMachine;
using Vinci.Core.Utils;

public class GameManager : PersistentSingleton<GameManager>
{
    public PlayerData playerData;

    public UiManager uiManager;

    private StateBase _activeState;

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData();

        SwitchState(new IdleGameState(this));
    }

    private void Update()
    {
        _activeState?.Tick(Time.deltaTime);
    }

    public void SwitchState(StateBase newState)
    {
        _activeState?.OnExitState();
        _activeState = newState;
        _activeState?.OnEnterState();
    }
}
