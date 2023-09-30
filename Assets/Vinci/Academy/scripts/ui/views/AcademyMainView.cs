using System;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class AcademyMainView : View
{
    [SerializeField]
    private Button _HomeButton;
    
    [SerializeField]
    private Button _selectAgentButton;

    public event Action homeButtonPressed;
    public event Action selectAgentButtonPressed;


    public override void Initialize()
    {
        Debug.Log("Initialize AcademyMainView");
        _HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
        _selectAgentButton.onClick.AddListener(() => selectAgentButtonPressed?.Invoke());
    }
}
