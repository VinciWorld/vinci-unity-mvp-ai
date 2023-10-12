using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.Managers;
using Vinci.Core.UI;

public class AcademyTrainView : View
{
    [SerializeField]
    private GameObject trainSteupSubView;
    [SerializeField]
    private GameObject trainInfoHudView;

    [Header("Setup Sub View")]
    [SerializeField]
    private Button _HomeButton;
    [SerializeField]
    private Button _backButton;
    [SerializeField]
    private Button _trainButton;
    [SerializeField]
    private Button _watchTrainButton;

    [SerializeField]
    private TMP_InputField _stepsInputField;
    [SerializeField]
    private TextMeshProUGUI _stepsTrained;
    [SerializeField]
    private TextMeshProUGUI _warningInputField;

    [SerializeField]
    private TextMeshProUGUI _defenseStatText;
    [SerializeField]
    private TextMeshProUGUI _attackStatText;
    [SerializeField]
    private TextMeshProUGUI _speedStatText;


    [Header("Train Info Sub View")]
    [SerializeField]
    private TextMeshProUGUI episodesCountText;
    [SerializeField]
    private TextMeshProUGUI stepsCountText;
    [SerializeField]
    private TextMeshProUGUI totalStepsCountText;
    [SerializeField]
    private TextMeshProUGUI meanRewardText;
    [SerializeField]
    private TextMeshProUGUI stdRewardText;
    [SerializeField]
    private TextMeshProUGUI winsCountText;
    [SerializeField]
    private TextMeshProUGUI losesCountText;



    public event Action homeButtonPressed;
    public event Action backButtonPressed;
    public event Action<int> trainButtonPressed;
    public event Action watchTrainButtonPressed;


    public override void Initialize()
    {
        CheckIsTrainIsRunning();

        _HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
        _backButton.onClick.AddListener(() => backButtonPressed?.Invoke());
        _trainButton.onClick.AddListener(OnTrainButtonPressed);
        _watchTrainButton.onClick.AddListener(() => watchTrainButtonPressed?.Invoke());

        ShowWarningInputfield("", false);
        UpdateStepsTrained(0);
        UpdateStats(30, 40, 15);
    }

    public void CheckIsTrainIsRunning()
    {
        _trainButton.gameObject.SetActive(true);
        _watchTrainButton.gameObject.SetActive(false);
       // if (GameManager.instance.playerData.currentAgentConfig.modelConfig.isModelTraining)
       // {
       //     _trainButton.gameObject.SetActive(false);
       //     _watchTrainButton.gameObject.SetActive(true);
       // }
    }

    public void OnTrainButtonPressed()
    {
        try
        {
            int stepsToTrain = int.Parse(_stepsInputField.text);
            trainButtonPressed?.Invoke(stepsToTrain);
            Debug.Log("Steps to train: " + stepsToTrain);
        }
        catch(Exception e)
        {
            trainButtonPressed?.Invoke(100);
        }
    }

    public void SetTrainSetupSubViewState(bool state)
    {
        trainSteupSubView.SetActive(state);
    }

    public void SetTrainHudSubViewState(bool state)
    {
        trainInfoHudView.SetActive(state);
    }

    public void UptadeInfo(
        int episodesCount, int stepsCount, int totalStepsCount)
    {
        episodesCountText.text = episodesCount.ToString();
        stepsCountText.text = stepsCount.ToString();
       // totalStepsCountText.text = "0"; //totalStepsCount.ToString();
    }

    public void UpdateMetrics(
        float meanReward,
        float stdReward
    )
    {

        Debug.Log("UPDAE METRICS " + meanReward + " : " + stdReward);
        meanRewardText.text = meanReward.ToString("F3");
        stdRewardText.text = stdReward.ToString("F3");
    }

    public void UpdateGameInfo(int wins, int loses)
    {
        winsCountText.text = wins.ToString();
        losesCountText.text = loses.ToString();
    }

    public void UpdateStepsTrained(int stesTrained)
    {
        _stepsTrained.text = stesTrained.ToString();
    }

    public void ShowWarningInputfield(string message, bool show)
    {
        _warningInputField.text = message;
        _warningInputField.gameObject.SetActive(show);
    }

    public void UpdateStats(int defense, int attack, int speed)
    {
        _defenseStatText.text = defense.ToString();
        _attackStatText.text = attack.ToString();
        _speedStatText.text = speed.ToString();
    }
}
