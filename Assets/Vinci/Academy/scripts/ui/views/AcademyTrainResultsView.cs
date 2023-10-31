using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class AcademyTrainResultsView : View
{

    [SerializeField]
    CategoryNavBar _categoryNavBar;

    [Header("Results")]
    [SerializeField]
    GameObject resultsSubView;

    [SerializeField]
    private LoaderPopup _loaderPopup;

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
    [SerializeField]
    private TextMeshProUGUI _stdRweard;

    [Header("Evaluate Model")]
    [SerializeField]
    GameObject popUpTestModel;
    [SerializeField]
    private Button _stopTestModelButton;
    public KeyValueText keyValueTextPrefab;
    public Transform parentTransformEvaluationHud;
    public Transform parentTransformEvaluatioResults;

    private Dictionary<string, KeyValueText> instantiatedPrefabs = new Dictionary<string, KeyValueText>();
    private Dictionary<string, KeyValueText> instantiatedPrefabsEvaluate = new Dictionary<string, KeyValueText>();


    public event Action homeButtonPressed;
    public event Action mintModelButtonPressed;
    public event Action trainAgainButtonPressed;
    public event Action evaluateModelButtonPressed;
    public event Action stopEvaluateModelButtonPressed;


    public override void Initialize()
    {
        _mintModelButton.onClick.AddListener(() => mintModelButtonPressed?.Invoke());
        _trainAgainButton.onClick.AddListener(() => trainAgainButtonPressed?.Invoke());
        _evaluateButton.onClick.AddListener(() => evaluateModelButtonPressed?.Invoke());
        _stopTestModelButton.onClick.AddListener(() => stopEvaluateModelButtonPressed?.Invoke());
    }

    public override void Show()
    {
        _categoryNavBar.SetTitles("Model Evaluation", "Academy");
        _categoryNavBar.homeButtonPressed += OnHomeButtonPressed;
        _categoryNavBar.SetNavigationButtons(
            false,
            true
        );

        ShowTestModelMetrics();
        base.Show();
    }

    public override void Hide()
    {
        _categoryNavBar.homeButtonPressed -= OnHomeButtonPressed;
        _categoryNavBar.RemoveListeners();
        base.Hide();
    }

    public void OnHomeButtonPressed()
    {
        homeButtonPressed?.Invoke();
    }

    public void ShowResults()
    {
        _categoryNavBar.SetTitles("Train results", "Academy");
        resultsSubView.SetActive(true);
        popUpTestModel.SetActive(false);
    }

    public void ShowTestModelMetrics()
    {
        resultsSubView.SetActive(false);
        popUpTestModel.SetActive(true);
    }

    public void UpdateTrainResults(int stepsTrained, float meanReward, float stdReward)
    {
        _stepTrainedCount.text = stepsTrained.ToString();
        _meanRweard.text = meanReward.ToString("F3");
        _stdRweard.text = stdReward.ToString("F3");
    }

    public void ShowLoaderPopup(string messange)
    {
        _loaderPopup.gameObject.SetActive(true);
        _loaderPopup.SetProcessingMEssage(messange);
        _loaderPopup.Open();
    }

    public void UpdatePopupMessange(string message)
    {
        _loaderPopup.SetProcessingMEssage(message);
    }

    public void CloseLoaderPopup()
    {
        _loaderPopup.Close();
    }

    public void UpdateEvaluationCommonMetrics(Dictionary<string, MetricValue> metrics)
    {
        foreach (var metric in metrics)
        {
            if (!instantiatedPrefabs.TryGetValue(metric.Key, out KeyValueText keyValueTextInstance))
            {
                keyValueTextInstance = Instantiate(keyValueTextPrefab, parentTransformEvaluatioResults);

                keyValueTextInstance.SetKeyAndValue(metric.Key, metric.Value.GetValueWithSymbol());
                instantiatedPrefabs[metric.Key] = keyValueTextInstance;
            }
            else
            {
                keyValueTextInstance.SetValue(metric.Value.GetValueWithSymbol());
            }
        }
    }

    public void UpdateEvaluationMetrics(Dictionary<string, MetricValue> metrics)
    {
        foreach (var metric in metrics)
        {
            if (!instantiatedPrefabsEvaluate.TryGetValue(metric.Key, out KeyValueText keyValueTextInstance))
            {
                keyValueTextInstance = Instantiate(keyValueTextPrefab, parentTransformEvaluationHud);

                keyValueTextInstance.SetKeyAndValue(metric.Key, metric.Value.GetValueWithSymbol());
                instantiatedPrefabsEvaluate[metric.Key] = keyValueTextInstance;
            }
            else
            {
                keyValueTextInstance.SetValue(metric.Value.GetValueWithSymbol());
            }
        }
    }
}