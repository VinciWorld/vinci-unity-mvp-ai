using System;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vinci.Core.Managers;
using Vinci.Core.UI;

public class AcademyMainView : View
{
    //[SerializeField]
    //PopupAdvance _loaderPopup;

    [SerializeField]
    CategoryNavBar _categoryNavBar;

    [SerializeField]
    private Button _createAgentButton;
    [SerializeField]
    private Button _selectAgentButton;
    [SerializeField]
    private Button _watchTrainButton;


    [Header("Agent Info")]
    [SerializeField]
    private TextMeshProUGUI _agentType;
    [SerializeField]
    private TextMeshProUGUI _agentName;
    [SerializeField]
    private TextMeshProUGUI _agentDescription;
    [SerializeField]
    private TextMeshProUGUI _processingPoswerStatText;
    [SerializeField]
    private TextMeshProUGUI _structuralIntegrityStatText;
    [SerializeField]
    private TextMeshProUGUI _sensorRangeStatText;
    [SerializeField]
    private TextMeshProUGUI _powerEfficiencyText;

    [Header("Section Model info")]
    [SerializeField]
    private KeyValueText _trainStatus;
    [SerializeField]
    private KeyValueText _trainsCount;
    [SerializeField]
    private KeyValueTextWithChange _totalTrainStepsKeyValue;

    [Header("Section Evaluation Model")]
    [SerializeField]
    private TextMeshProUGUI _evaluationActionText;
    [SerializeField]
    private Button _evaluateModelButton;
    [SerializeField]
    private GameObject _evaluatLayoutMessage;
    [SerializeField]
    private GameObject _evaluatLayotMetrics;
    [SerializeField]
    public GameObject slotKeyTexChangePrefab;
    [SerializeField]
    public Transform parentLayoutModelCommonMetrics;
    [SerializeField]
    public Transform parentLayoutModelEnvMetrics;

    [SerializeField]
    public GameObject _agentSlide;

    [SerializeField]
    public GameObject _availableAgentsList;

    [SerializeField]
    private KeyValueTextWithChange keyValueTextWithChangePrefab;
    private Dictionary<string, KeyValueTextWithChange> instantiatedPrefabsCommonMetrics = new Dictionary<string, KeyValueTextWithChange>();
    private Dictionary<string, KeyValueTextWithChange> instantiatedPrefabsEnvMetrics = new Dictionary<string, KeyValueTextWithChange>();

    public event Action homeButtonPressed;
    public event Action createAgentButtonPressed;
    public event Action selectAgentButtonPressed;
    public event Action watchTrainingButtonPressed;
    public event Action evaluateModelButtonPressed;


    public override void Initialize()
    {
        _createAgentButton.onClick.AddListener(() => createAgentButtonPressed?.Invoke());
        _selectAgentButton.onClick.AddListener(() => selectAgentButtonPressed?.Invoke());
        _watchTrainButton.onClick.AddListener(() => watchTrainingButtonPressed?.Invoke());
        _evaluateModelButton.onClick.AddListener(() => evaluateModelButtonPressed?.Invoke());
    }

    public override void Show()
    {
        _categoryNavBar.SetTitles("Agent Setup", "Academy");
        _categoryNavBar.homeButtonPressed += OnHomeButtonPressed;
        _categoryNavBar.SetNavigationButtons(
            false,
            true
        );

        UpdateStats(30, 40, 15, 20);

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


    public void SetButtonWatchState(bool state)
    {
        _watchTrainButton.gameObject.SetActive(state);
    }

    public void SetAgentInfo(string type, string name, string description)
    {
        _agentType.text = type;
        _agentName.text = name;
        _agentDescription.text = description;
    }

    public void UpdateStats(
        int processsingPower,
        int structuralIntetrity,
        int sensorRange,
        int powerEfficiency
    )
    {
        _processingPoswerStatText.text = processsingPower.ToString();
        _structuralIntegrityStatText.text = structuralIntetrity.ToString();
        _sensorRangeStatText.text = sensorRange.ToString();
        _powerEfficiencyText.text = powerEfficiency.ToString();
    }

    public void CheckIfAgentIsCreated()
    {
        _createAgentButton.gameObject.SetActive(true);
        _selectAgentButton.gameObject.SetActive(false);
        if (GameManager.instance.playerData.agents.Count >= 1)
        {
            _createAgentButton.gameObject.SetActive(false);
            _selectAgentButton.gameObject.SetActive(true);
        }
    }

    public void HideAllButtons()
    {
        _createAgentButton.gameObject.SetActive(false);
        _selectAgentButton.gameObject.SetActive(false);
        _watchTrainButton.gameObject.SetActive(false);
        _evaluateModelButton.gameObject.SetActive(false);
    }

    public void ShowCreateButton()
    {
        _createAgentButton.gameObject.SetActive(true);
        _selectAgentButton.gameObject.SetActive(false);
        _watchTrainButton.gameObject.SetActive(false);
        _evaluateModelButton.gameObject.SetActive(false);
    }

    public void ShowWTrainButton()
    {
        _createAgentButton.gameObject.SetActive(false);
        _selectAgentButton.gameObject.SetActive(true);
        _watchTrainButton.gameObject.SetActive(false);
        _evaluateModelButton.gameObject.SetActive(false);
    }

    public void ShowWatchButton()
    {
        _createAgentButton.gameObject.SetActive(false);
        _selectAgentButton.gameObject.SetActive(false);
        _watchTrainButton.gameObject.SetActive(true);
        _evaluateModelButton.gameObject.SetActive(false);
    }

/*

    public void ShowLoaderPopup(string messange)
    {
        _loaderPopup.gameObject.SetActive(true);
        _loaderPopup.SetTitle(messange);
        _loaderPopup.Open();
    }

    public void UpdateLoaderMessage(string messange)
    {
        _loaderPopup.SetTitle(messange);
    }

    public void CloseLoaderPopup()
    {
        _loaderPopup.Close();
    }
*/

    public void SetLastJobStatus(string status, string hexColor = "EFF1F5")
    {
        _trainStatus.SetValue(status, hexColor);
    }

    public void SetTrainsCount(int trainsCount)
    {
        _trainsCount.SetValue(trainsCount.ToString());
    }


    public void SetTotalStepsTrained(int totalSteps, int lastTrainSteps)
    {
        int totalStepsConverted = totalSteps / 1000;
        int lastTrainStepsConverted = lastTrainSteps / 1000;
        _totalTrainStepsKeyValue.SetValue(
            totalStepsConverted.ToString() + " k", "+ " + lastTrainStepsConverted.ToString() + " k", ChangeStatus.Better
        );
    }

    #region Section-Model-Evaluation

    public void ShowEvalauteMessage(string message, bool showButton = false)
    {
        _evaluatLayoutMessage.SetActive(true);
        _evaluatLayotMetrics.SetActive(false);
        _evaluationActionText.text = message;
        if(showButton)
        {
            ShowEvaluateButton();
        }
    }

    public void ShowEvaluateMetrics(
        Dictionary<string, MetricChange> commonMetrics,
        Dictionary<string, MetricChange> envMetrics
    )
    {
        _evaluatLayotMetrics.SetActive(true);
        _evaluatLayoutMessage.SetActive(false);

        UpdateEvaluationCommonMetrics(commonMetrics);
    }

    private void ShowEvaluateButton()
    {
        _createAgentButton.gameObject.SetActive(false);
        _selectAgentButton.gameObject.SetActive(false);
        _watchTrainButton.gameObject.SetActive(false);
        _evaluateModelButton.gameObject.SetActive(true);
    }

    public void UpdateEvaluationCommonMetrics(Dictionary<string, MetricChange> metrics)
    {
        foreach (var metric in metrics)
        {
            if (!instantiatedPrefabsCommonMetrics.TryGetValue(metric.Key, out KeyValueTextWithChange keyValueTextInstance))
            {
                keyValueTextInstance = Instantiate(keyValueTextWithChangePrefab, parentLayoutModelCommonMetrics);

                keyValueTextInstance.SetKeyAndValue(
                    metric.Key,
                    metric.Value.GetValueWithSymbol(),
                    metric.Value.GetValueChangeWithSymbol(),
                    metric.Value.status
                );

                instantiatedPrefabsCommonMetrics[metric.Key] = keyValueTextInstance;
            }
            else
            {
                keyValueTextInstance.SetValue(
                    metric.Value.GetValueWithSymbol(),
                    metric.Value.GetValueChangeWithSymbol(),
                    metric.Value.status
                );
            }
        }
    }

    #endregion
}
