using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class GameHudView : View 
{
    [SerializeField]
    private Button _HomeButton;

    [SerializeField]
    private PopUpInfo _popupInfo;

    public TextMeshProUGUI _currentCoins;
    public TextMeshProUGUI _currentWave;
    public TextMeshProUGUI _deathsText;
    public TextMeshProUGUI _killsText;

    [Header("StatsInfo")]
    [SerializeField]
    private Slider _defenseStatSlider;
    [SerializeField]
    private Slider _attackStatSlider;
    [SerializeField]
    private Slider _speedStatSlider;

    [Header("Upgrades")]
    [SerializeField]
    private Button _upgradeDefenseButton;
    [SerializeField]
    private Button _upgradeAttackButton;
    [SerializeField]
    private Button _upgradeSpeedButton;
    [SerializeField]
    public TextMeshProUGUI _upgradeDefenseCost;
    [SerializeField]
    public TextMeshProUGUI _upgradeAttackCost;
    [SerializeField]
    public TextMeshProUGUI _upgradeSpeedCost;

    [Header("PopUps")]
    [SerializeField]
    GameOverPopUp gameOverPopUp;


    public event Action homeButtonPressed;
    public event Action retryButtonPressed;
    public event Action registerScoreOnBlockchainPressed;
    public event Action upgradeDefenseButtonPressed;
    public event Action upgradeAttackButtonPessed;
    public event Action upgradeSpeedButtonPressed;


    public override void Initialize()
    {
        _upgradeDefenseButton.onClick.AddListener(() => upgradeDefenseButtonPressed?.Invoke());
        _upgradeAttackButton.onClick.AddListener(() => upgradeSpeedButtonPressed?.Invoke());
        _upgradeSpeedButton.onClick.AddListener(() => upgradeSpeedButtonPressed?.Invoke());
        _HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());

        gameOverPopUp.homeButtonPressed += OnHomeButtonPressedFromGameOver;

        _popupInfo.Show(
            "Info",
            "The previously trained model was saved on Arweave. This current model is for demonstration only. \nDrag the agent below to the battlefield.",
            true
        );
    }

    public void OnUpgradeDefenseClicked()
    {
        upgradeDefenseButtonPressed?.Invoke();
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

    public void OnHomeButtonPressedFromGameOver()
    {
        gameOverPopUp.Close();
        
        homeButtonPressed?.Invoke();
    }

    public void UpdateCurrentCoins(int totalCoins)
    {
        _currentCoins.text = totalCoins.ToString();
    }

    public void UpdateWavesCount(int totalWavesCount)
    {
        _currentWave.text = $"Wave {totalWavesCount}";
    }

    public void UpdateKills(int totalKills)
    {
        _killsText.text = totalKills.ToString();
    }

    public void UpdateDeaths(int totalDeaths)
    {
        _deathsText.text = totalDeaths.ToString();
    }

    public void SetInitialUpgradesCost(int defenseStatCost, int attackStatCost, int speedStatCost)
    {
        _upgradeDefenseCost.text = defenseStatCost.ToString();
        _upgradeAttackCost.text = attackStatCost.ToString();
        _upgradeSpeedCost.text = speedStatCost.ToString();
    }

    public void UpdateStats(int defense, int attack, int speed)
    {
        _defenseStatSlider.value = defense / 100f;
        _attackStatSlider.value = attack / 100f;
        _speedStatSlider.value = speed / 100f;
    }

    public void ShowGameOver(int wavesSurvived, int totalKills, int totalDeaths, bool isHighScore, int score)
    {
        gameOverPopUp.gameObject.SetActive(true);
        gameOverPopUp.Open();
        gameOverPopUp.retryButtonPressed += OnRetryButtonPressed;
        gameOverPopUp.registerOnBlockchainButtonPressed += OnRegisterScoreOnBlockchainPressed;
        gameOverPopUp.Initialize(wavesSurvived, totalDeaths, totalKills, isHighScore, score);
    }

    public void ShowNotEnoughCoins()
    {
        Debug.Log("Not enough Coins");
    }

    public void MaxUpgradeReached()
    {
        Debug.Log("Max upgrade level reach");
    }
}