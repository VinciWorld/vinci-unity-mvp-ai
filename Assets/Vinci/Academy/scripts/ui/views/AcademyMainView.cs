using System;
using TMPro;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vinci.Core.Managers;
using Vinci.Core.UI;

public class AcademyMainView : View
{
    [SerializeField]
    LoaderPopup _loaderPopup;

    [SerializeField]
    CategoryNavBar _categoryNavBar;

    [SerializeField]
    private Button _createAgentButton;
    [SerializeField]
    private Button _selectAgentButton;
    [SerializeField]
    private Button _watchTrainButton;
    [SerializeField]
    private Button _evaluateModelButton;

    [Header("Agent Info")]
    [SerializeField]
    private TextMeshProUGUI _agentType;
    [SerializeField]
    private TextMeshProUGUI _agentName;
    [SerializeField]
    private TextMeshProUGUI _agentDescription;
    [SerializeField]
    private TextMeshProUGUI _defenseStatText;
    [SerializeField]
    private TextMeshProUGUI _attackStatText;
    [SerializeField]
    private TextMeshProUGUI _speedStatText;

    [Header("Trained Model info")]
    [SerializeField]
    private KeyValueText _trainsCount;
    [SerializeField]
    private KeyValueText _totalStepsTrainedText;
    [SerializeField]
    private TextMeshProUGUI _lastTrainJobStatus;
    [SerializeField]
    private KeyValueText _lastTrainSteps;

    [SerializeField]
    public GameObject _agentSlide;

    [SerializeField]
    public GameObject _availableAgentsList;


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

       //CheckIfAgentIsCreated();
        UpdateStats(30, 40, 15);

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

    public void UpdateStats(int defense, int attack, int speed)
    {
        _defenseStatText.text = defense.ToString();
        _attackStatText.text = attack.ToString();
        _speedStatText.text = speed.ToString();
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


    public void ShowEvaluateButton()
    {
        _createAgentButton.gameObject.SetActive(false);
        _selectAgentButton.gameObject.SetActive(false);
        _watchTrainButton.gameObject.SetActive(false);
        _evaluateModelButton.gameObject.SetActive(true);
    }

    public void ShowLoaderPopup(string messange)
    {
        _loaderPopup.gameObject.SetActive(true);
        _loaderPopup.SetProcessingMEssage(messange);
        _loaderPopup.Open();
    }

    public void UpdateLoaderMessage(string messange)
    {
        _loaderPopup.SetProcessingMEssage(messange);
    }

    public void CloseLoaderPopup()
    {
        _loaderPopup.Close();
    }

    public void SetLastJobStatus(string status, string hexColor = "EFF1F5")
    {
        if (!ColorUtility.TryParseHtmlString(hexColor, out Color color))
        {
            color = Color.white;
        }

        _lastTrainJobStatus.color = color;
        _lastTrainJobStatus.text = status;
    }

    public void SetTrainsCount(int trainsCount)
    {
        _trainsCount.SetValue(trainsCount.ToString());
    }

    public void SetLastTrainSteps(int trainSteps)
    {
        int lasttotlaSteps = trainSteps / 1000;
        _lastTrainSteps.SetValue(lasttotlaSteps.ToString() + " k");
    }

    public void SetTotalStepsTrained(int totalSteps)
    {
        int totlaSteps = totalSteps / 1000;
        _totalStepsTrainedText.SetValue( totlaSteps.ToString() + " k");
    }
}
