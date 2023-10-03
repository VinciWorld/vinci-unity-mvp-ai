using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class AcademyTrainResultsView : View
{

    [Header("Results")]
    [SerializeField]
    GameObject resultsSubView;

    [SerializeField]
    private Button _HomeButton;
    [SerializeField]
    private Button _mintModelButton;
    [SerializeField]
    private Button _trainAgainButton;
    [SerializeField]
    private Button _evaluateButton;
    [SerializeField]
    private TextMeshProUGUI _stepTrainedCount;
    [SerializeField]
    private TextMeshProUGUI _meanRweard;

    [Header("Test Model")]
    [SerializeField]
    GameObject popUpTestModel;
    [SerializeField]
    private Button _stopTestModelButton;
    [SerializeField]
    private TextMeshProUGUI _goalCompletedCountTextHud;
    [SerializeField]
    private TextMeshProUGUI _goalFailedCountTextHud;
    [SerializeField]
    private TextMeshProUGUI _goalSuccessRationTextHud;

    [SerializeField]
    private TextMeshProUGUI _goalCompletedCountText;
    [SerializeField]
    private TextMeshProUGUI _goalFailedCountText;
    [SerializeField]
    private TextMeshProUGUI _goalSuccessRationText;


    public event Action homeButtonPressed;
    public event Action mintModelButtonPressed;
    public event Action trainAgainButtonPressed;
    public event Action evaluateModelButtonPressed;
    public event Action stopEvaluateModelButtonPressed;


    public override void Initialize()
    {
        //_HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
        _mintModelButton.onClick.AddListener(() => mintModelButtonPressed?.Invoke());
        _trainAgainButton.onClick.AddListener(() => trainAgainButtonPressed?.Invoke());
        _evaluateButton.onClick.AddListener(() => evaluateModelButtonPressed?.Invoke());
        _stopTestModelButton.onClick.AddListener(() => stopEvaluateModelButtonPressed?.Invoke());

        ShowResultsSubView();
    }

    public void ShowResultsSubView()
    {
        resultsSubView.SetActive(true);
        popUpTestModel.SetActive(false);
    }

    public void ShowTestModelMetrics()
    {
        resultsSubView.SetActive(false);
        popUpTestModel.SetActive(true);
    }

    public void UpdateTrainResults(int stepsTrained, float meanReward)
    {
        _stepTrainedCount.text = stepsTrained.ToString();
        _meanRweard.text = meanReward.ToString("F00");
    }

    public void UpdateTestMetrics(int goalCompletedCount, int goalFailedCount, float goalSuccessRatio)
    {
        _goalCompletedCountText.text = goalCompletedCount.ToString();
        _goalFailedCountText.text = goalFailedCount.ToString();
        _goalSuccessRationText.text = goalSuccessRatio.ToString("F0");

        _goalCompletedCountTextHud.text = goalCompletedCount.ToString();
        _goalFailedCountTextHud.text = goalFailedCount.ToString();
        _goalSuccessRationTextHud.text = goalSuccessRatio.ToString("F0");
    }
}