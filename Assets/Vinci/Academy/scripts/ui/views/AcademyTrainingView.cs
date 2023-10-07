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
        _HomeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());
        _trainButton.onClick.AddListener(OnTrainButtonPressed);
    }

    public void OnTrainButtonPressed()
    {
        int stepsToTrain = int.Parse(_stepsInputField.text);
        trainButtonPressed?.Invoke(stepsToTrain);

        Debug.Log(stepsToTrain);
    }
}
