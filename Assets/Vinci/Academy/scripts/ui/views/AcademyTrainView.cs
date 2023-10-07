using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    private Button _trainButton;

    [Header("Train Info Sub View")]
    [SerializeField]
    private TextMeshProUGUI episodesCountText;
    [SerializeField]
    private TextMeshProUGUI stepsCountText;
    [SerializeField]
    private TextMeshProUGUI meanRewardText;
    [SerializeField]
    private TextMeshProUGUI stdRewardText;
    [SerializeField]
    private TextMeshProUGUI winsCountText;
    [SerializeField]
    private TextMeshProUGUI losesCountText;

    [SerializeField]
    private TMP_InputField _stepsInputField;

    public event Action homeButtonPressed;
    public event Action<int> trainButtonPressed;


    public override void Initialize()
    {
        _HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
        _trainButton.onClick.AddListener(OnTrainButtonPressed);
    }

    public void OnTrainButtonPressed()
    {
        int stepsToTrain = int.Parse(_stepsInputField.text);
        trainButtonPressed?.Invoke(stepsToTrain);

        Debug.Log(stepsToTrain);
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
        int episodesCount, int stepsCount)
    {
        episodesCountText.text = episodesCount.ToString();
        stepsCountText.text = stepsCount.ToString();
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

}
