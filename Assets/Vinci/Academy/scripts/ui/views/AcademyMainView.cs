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
    private Button _createAgent;
    [SerializeField]
    private Button _selectAgent;
    [SerializeField]
    private Button _watchTrain;

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

    [SerializeField]
    private TextMeshProUGUI _stepsTrainedText;
    [SerializeField]
    private TextMeshProUGUI _jobStatusText;

    [SerializeField]
    public GameObject _agentSlide;

    [SerializeField]
    public GameObject _availableAgentsList;


    public event Action homeButtonPressed;
    public event Action createAgentButtonPressed;
    public event Action selectAgentButtonPressed;
    public event Action watchTrainingButtonPressed;


    public override void Initialize()
    {
        _createAgent.onClick.AddListener(() => createAgentButtonPressed?.Invoke());
        _selectAgent.onClick.AddListener(() => selectAgentButtonPressed?.Invoke());
        _watchTrain.onClick.AddListener(() => watchTrainingButtonPressed?.Invoke());
    }

    public override void Show()
    {
        _categoryNavBar.SetTitles("Agent Setup", "Academy");
        _categoryNavBar.homeButtonPressed += OnHomeButtonPressed;
        _categoryNavBar.SetNavigationButtons(
            false,
            true
        );

        CheckIfAgentIsCreated();
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
        _watchTrain.gameObject.SetActive(state);
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
        _createAgent.gameObject.SetActive(true);
        _selectAgent.gameObject.SetActive(false);
        if (GameManager.instance.playerData.agents.Count >= 1)
        {
            _createAgent.gameObject.SetActive(false);
            _selectAgent.gameObject.SetActive(true);
        }
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

    public void SetJobStatus(string status)
    {
        _jobStatusText.text = "Status: " +  status;
    }

    public void SetStepsTrained(int steps)
    {
        int totlaSteps = steps / 1000;
        _stepsTrainedText.text = "Steps trained: " + totlaSteps.ToString() + " k";
    }
}
