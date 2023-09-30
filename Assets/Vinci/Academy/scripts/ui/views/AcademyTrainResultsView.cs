using System;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class AcademyTrainResultsView : View
{
    [SerializeField]
    private Button _HomeButton;

    [SerializeField]
    private Button _mintModelButton;

    [SerializeField]
    GameObject popupResults;

    public event Action homeButtonPressed;
    public event Action mintModelButtonPressed;


    public override void Initialize()
    {
        //_HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
        _mintModelButton.onClick.AddListener(() => mintModelButtonPressed?.Invoke());

        SetPopupState(false);
    }

    public void SetPopupState(bool state)
    {
        popupResults.SetActive(state);
    }
}
