using System;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class AcademyTrainView : View
{
    [SerializeField]
    private Button _HomeButton;
    
    [SerializeField]
    private Button _trainButton;

    [SerializeField]
    private GameObject trainSteupSubView;

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

}
