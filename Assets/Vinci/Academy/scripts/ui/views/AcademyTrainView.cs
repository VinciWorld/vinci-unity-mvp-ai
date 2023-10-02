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

    public event Action homeButtonPressed;
    public event Action trainButtonPressed;


    public override void Initialize()
    {
        _HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
        _trainButton.onClick.AddListener(() => trainButtonPressed?.Invoke());
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
        meanRewardText.text = meanReward.ToString("F001");
        stdRewardText.text = stdReward.ToString("F001");
    }

    public void UpdateGameInfo(int wins, int loses)
    {
        winsCountText.text = wins.ToString();
        losesCountText.text = loses.ToString();
    }

}
