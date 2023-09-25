using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class MainMenuScreen : View
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
        ViewManager.Show<HeadquartersScreen>();
    }
}
