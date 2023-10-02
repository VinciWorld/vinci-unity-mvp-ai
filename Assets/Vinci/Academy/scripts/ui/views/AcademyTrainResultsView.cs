using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class AcademyTrainResultsView : View
{

    [Header("Results")]
    [SerializeField]
    GameObject popupResults;

    [SerializeField]
    private Button _HomeButton;
    [SerializeField]
    private Button _mintModelButton;
    [SerializeField]
    private Button _trainAgainButton;
    [SerializeField]
    private Button _testModelButton;
    [SerializeField]
    private TextMeshProUGUI _stepTrainedCount;
    [SerializeField]
    private TextMeshProUGUI _meanRweard;

    [Header("Test Model")]
    [SerializeField]
    GameObject popUpTestModel;
    [SerializeField]
    private Button _stopTestModelButton;
    [SerializeField]
    private TextMeshProUGUI _goalCompletedCountText;
    [SerializeField]
    private TextMeshProUGUI _goalFailedCountText;
    [SerializeField]
    private TextMeshProUGUI _winRationText;


    public event Action homeButtonPressed;
    public event Action mintModelButtonPressed;
    public event Action trainAgianButtonPressed;
    public event Action testModelButton;


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
