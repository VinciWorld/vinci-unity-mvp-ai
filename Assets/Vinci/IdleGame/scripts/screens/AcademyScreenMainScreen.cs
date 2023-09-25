using UnityEngine;
using UnityEngine.UI;

public class AcademyScreenMainScreen : Screen
{
    [SerializeField]
    private Button _backButton;
    
    [SerializeField]
    private Button _trainButton;



    public override void Initialize()
    {
        _backButton.onClick.AddListener(() => ScreenManager.ShowLast());
        _trainButton.onClick.AddListener(OnBtnTrainButtonPressed);
    }

    void OnBtnTrainButtonPressed()
    {
        ScreenManager.Show<HeadquartersScreen>();
    }
}
