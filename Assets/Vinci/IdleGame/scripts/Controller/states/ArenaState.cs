
using UnityEngine;
using UnityEngine.Rendering;
using Vinci.Core.Managers;
using Vinci.Core.StateMachine;
using Vinci.Core.UI;

public class ArenaState : StateBase
{
    IdleGameController _controller;
    ArenaView arenaView;

    public ArenaState(IdleGameController controller)
    {
        _controller = controller;
    }

    public override void OnEnterState()
    {
        arenaView = ViewManager.GetView<ArenaView>();
        ViewManager.Show(arenaView);

        arenaView.registerCompetitionButtonPressed += OnRegisterOnCompetionButtonPressed;
        arenaView.playGameButtonPressed += OnPlayTournamentButtonPressed;

        PopulatePlayersScores();
    }

    public override void OnExitState()
    {

    }

    public override void Tick(float deltaTime)
    {

    }

    public void OnRegisterOnCompetionButtonPressed()
    {
        bool result = BlockchainManager.instance.RegisterPlayerOnCompetition();

        if(result)
        {
            arenaView.ShowButtonPlayer();
        }
    }

    public void OnPlayTournamentButtonPressed()
    {
        SceneLoader.instance.LoadSceneDelay("Arena");
    }

    public void PopulatePlayersScores()
    {
        BlockchainManager.instance.GetPlayeresScores();

        arenaView.PopulatePlayersScores("costa", 2000);
    }


}