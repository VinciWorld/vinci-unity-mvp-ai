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
    CategoryNavBar _categoryNavBar;
    
    [SerializeField]
    private Button _createAgent;

    [SerializeField]
    private Button _selectAgent;

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
    public GameObject _agentSlide;

    [SerializeField]
    public GameObject _availableAgentsList;


    public event Action homeButtonPressed;
    public event Action createAgentButtonPressed;
    public event Action selectAgentButtonPressed;


    public override void Initialize()
    {
        _createAgent.onClick.AddListener(() => createAgentButtonPressed?.Invoke());
        _selectAgent.onClick.AddListener(() => selectAgentButtonPressed?.Invoke());
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
        Debug.Log("Remove main listeners");
        base.Hide();
    }

    public void OnHomeButtonPressed()
    {
        homeButtonPressed?.Invoke();
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
}
