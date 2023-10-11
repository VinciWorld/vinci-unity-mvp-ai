using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.Managers;
using Vinci.Core.UI;

public class AcademyMainView : View
{
    [SerializeField]
    private Button _HomeButton;
    
    [SerializeField]
    private Button _createAgent;

    [SerializeField]
    private Button _selectAgent;

    [SerializeField]
    private TextMeshProUGUI _agentTitle;
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
        _HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
        _createAgent.onClick.AddListener(() => createAgentButtonPressed?.Invoke());
        _selectAgent.onClick.AddListener(() => selectAgentButtonPressed?.Invoke());

        CheckIfAgentIsCreated();

        UpdateStats(30, 40, 15);
    }

    void OnEnable()
    {
        CheckIfAgentIsCreated();
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
