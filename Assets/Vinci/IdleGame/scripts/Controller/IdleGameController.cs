using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;

public class IdleGameController : MonoBehaviour
{
    GameManager _manager;

    private StateBase _activeState;

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameManager.instance;


        SwitchState(new IdleGameState(this));
    }

    private void Update()
    {
        _activeState?.Tick(Time.deltaTime);
    }

    public async void SwitchState(StateBase newState, string sceneName = null)
    {
        _activeState?.OnExitState();
        if (sceneName != null)
        {
            await SceneLoader.instance.LoadScene(sceneName);
        }
        _activeState = newState;
        _activeState?.OnEnterState();
    }
}
