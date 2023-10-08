using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vinci.Academy.Environement;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;

public class AcademyController : MonoBehaviour
{
    public GameManager manager;
    public EnvManager envManager;
    public AcademyData academyData;
    public AcademySession session = new AcademySession();

    private StateBase _activeState;

    void Start()
    {
        manager = GameManager.instance;
#if UNITY_EDITOR || UNITY_WEBGL
        SwitchState(new AcademyMainState(this));

#elif !UNITY_EDITOR && UNITY_SERVER
        SwitchState(new AcademyServerInstanceState(this));

#endif

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
