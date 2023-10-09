using System;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class GameHudView : View 
{
    [SerializeField]
    private Button _HomeButton;

    [SerializeField]
    private Button _selectAgentButton;

    public event Action homeButtonPressed;
    public event Action retryButtonPressed;
    public event Action registerScoreOnBlockchainPressed;


    [Header("PopUps")]
    [SerializeField]
    GameOverPopUp gameOverPopUp;


    public override void Initialize()
    {
       // _HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
    }

    public void OnRetryButtonPressed()
    {
        gameOverPopUp.Close();
        retryButtonPressed?.Invoke();
    }

    public void OnRegisterScoreOnBlockchainPressed()
    {
        gameOverPopUp.Close();
        registerScoreOnBlockchainPressed?.Invoke();
    }

    public void OnHomeButtonPressed()
    {

    }

    public void UpdateWavesCount(int totalWavesCount)
    {

    }

    public void UpdateKills(int totalKills)
    {

    }

    public void UpdateDeaths(int totalKills)
    {

    }

    public void UpdateStats(int health, int attack, int speed)
    {

    }

    public void ShowGameOver(int wavesSurvived, int totalKills, int totalDeaths, bool isHighScore, int score)
    {
        gameOverPopUp.Open();
        gameOverPopUp.retryButtonPressed += OnRetryButtonPressed;
        gameOverPopUp.registerOnBlockchainButtonPressed += OnRegisterScoreOnBlockchainPressed;
        gameOverPopUp.Initialize(wavesSurvived, totalKills, totalDeaths, isHighScore, score);
    }

    public void ShowNotEnoughCoins()
    {
        Debug.Log("Not enough Coins");
    }


}