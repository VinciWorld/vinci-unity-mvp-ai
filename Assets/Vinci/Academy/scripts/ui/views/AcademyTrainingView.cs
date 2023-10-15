using System;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class AcademyTrainingView : View
{
    [SerializeField]
    private Button _HomeButton;

    [SerializeField]
    private Button _trainButton;

    [SerializeField]
    private InputField _stepsInputField;

    public event Action homeButtonPressed;
    public event Action<int> trainButtonPressed;


    public override void Initialize()
    {

    }

    public override void Show()
    {
        _HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
        _trainButton.onClick.AddListener(OnTrainButtonPressed);
        base.Show();
    }

    public override void Hide()
    {
        _trainButton.onClick.RemoveAllListeners();
        _HomeButton.onClick.RemoveAllListeners();
        base.Hide();
    }

    public void OnTrainButtonPressed()
    {
        int stepsToTrain = int.Parse(_stepsInputField.text);
        trainButtonPressed?.Invoke(stepsToTrain);

        Debug.Log(stepsToTrain);
    }
}
