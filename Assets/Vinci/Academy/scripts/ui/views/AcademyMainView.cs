using System;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class AcademyMainView : View
{
    [SerializeField]
    private Button _HomeButton;
    
    [SerializeField]
    private Button _trainButton;

    public event Action homeButtonPressed;
    public event Action selectAgentButtonPressed;


    public override void Initialize()
    {
        _HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
        _trainButton.onClick.AddListener(() => selectAgentButtonPressed?.Invoke());
    }
}
