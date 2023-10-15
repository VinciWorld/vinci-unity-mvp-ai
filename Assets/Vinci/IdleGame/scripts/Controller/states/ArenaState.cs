
using System;
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
        arenaView.backButtonPressed += OnBackButtonPressed;

        PopulatePlayersScores();
    }



    public override void OnExitState()
    {
        arenaView.registerCompetitionButtonPressed -= OnRegisterOnCompetionButtonPressed;
        arenaView.playGameButtonPressed -= OnPlayTournamentButtonPressed;
        arenaView.backButtonPressed -= OnBackButtonPressed;
    }

    public override void Tick(float deltaTime)
    {

    }

    private void OnBackButtonPressed()
    {
        _controller.SwitchState(new IdleGameState(_controller));
    }

    async public void OnRegisterOnCompetionButtonPressed()
    {
        await BlockchainManager.instance.RegisterPlayerOnCompetition();
        arenaView.ShowButtonPlayer();
    }

    public void OnPlayTournamentButtonPressed()
    {
        OnExitState();
        SceneLoader.instance.LoadSceneDelay("Arena");
    }

    async public void PopulatePlayersScores()
    {
        int score = await BlockchainManager.instance.GetPlayeresScores();

        arenaView.PopulatePlayersScores(
            GameManager.instance.UserData.username,
            score
        );
    }


}