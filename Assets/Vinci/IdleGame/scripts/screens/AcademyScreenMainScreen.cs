using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class AcademyScreenMainScreen : View
{
    [SerializeField]
    private Button _backButton;
    
    [SerializeField]
    private Button _trainButton;



    public override void Initialize()
    {
        _backButton.onClick.AddListener(() => ViewManager.ShowLast());
        _trainButton.onClick.AddListener(OnBtnTrainButtonPressed);
    }

    void OnBtnTrainButtonPressed()
    {
        ViewManager.Show<HeadquartersScreen>();
    }
}
