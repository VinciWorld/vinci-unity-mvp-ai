using System;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class IdleGameMainView : View
{
    [SerializeField]
    PopUpInfo popUpInfo;

    [SerializeField]
    private Button _headquartersButton;
    [SerializeField]
    private Button _academyButton;
    [SerializeField]
    private Button _arenaButton;

    public LoginView loginView;

    public event Action headquartersBtnPressed;
    public event Action academyBtnPressed;
    public event Action arenaBtnPressed;


    public override void Initialize()
    {
        _headquartersButton.onClick.AddListener(() => headquartersBtnPressed?.Invoke());
        _academyButton.onClick.AddListener(() => academyBtnPressed?.Invoke());
        _arenaButton.onClick.AddListener(() => arenaBtnPressed?.Invoke());
    }

    public override void Show()
    {
        base.Show();
    }

    public void ShowLoaderPopup(string title, string message, bool isCloseButtonOn)
    {
        popUpInfo.gameObject.SetActive(true);
        popUpInfo.Show(title, message, isCloseButtonOn);
        popUpInfo.Open();
    }

    public void CloseLoaderPopup()
    {
        popUpInfo.Close();
    }
}
