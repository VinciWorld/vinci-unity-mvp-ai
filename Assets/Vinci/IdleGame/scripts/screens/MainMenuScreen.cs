using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : Screen
{
    [SerializeField]
    private Button _btnHeadquarters;
    [SerializeField]
    private Button _btnAcademy;
    [SerializeField]
    private Button _bntArena;

    public override void Initialize()
    {
        _btnHeadquarters.onClick.AddListener(OnBtnHeadquartersPressed);
    }

    void OnBtnHeadquartersPressed()
    {
        ScreenManager.Show<HeadquartersScreen>();
    }
}
