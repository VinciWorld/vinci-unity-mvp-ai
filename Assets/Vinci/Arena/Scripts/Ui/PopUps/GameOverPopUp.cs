using System;
using Ricimi;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPopUp : Popup 
{
    [SerializeField]
    TextMeshProUGUI wavesSurvivedText;
    [SerializeField]
    TextMeshProUGUI deathsText;
    [SerializeField]
    TextMeshProUGUI killsText;
    [SerializeField]
    TextMeshProUGUI scoreTitleText;
    [SerializeField]
    TextMeshProUGUI scoreValueText;

    [SerializeField]
    Button registerOnBlockchainButton;
    [SerializeField]
    Button retryButton;
    [SerializeField]
    Button homeButton;


    public event Action retryButtonPressed;
    public event Action homeButtonPressed;
    public event Action registerOnBlockchainButtonPressed;

    public void Initialize(int wavesSurvived, int deaths, int kills, bool isHighScore, int score)
    {
        string wavesSurvide = $"You survived {wavesSurvived} waves!";
        if(wavesSurvived == 1)
        {
            wavesSurvide = $"You survived {wavesSurvived} wave!";
        }
        wavesSurvivedText.text = wavesSurvide;

        deathsText.text = deaths.ToString();
        killsText.text = kills.ToString();

        scoreTitleText.text = "You already have a best score. Try Again!";
        registerOnBlockchainButton.gameObject.SetActive(false);
        if (isHighScore)
        {
            scoreTitleText.text = "New High Score!";
            registerOnBlockchainButton.gameObject.SetActive(true);
        }
        scoreValueText.text = score.ToString();

        retryButton.onClick.AddListener(() => retryButtonPressed?.Invoke());
        registerOnBlockchainButton.onClick.AddListener(() => registerOnBlockchainButtonPressed?.Invoke());
        homeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
    }
}