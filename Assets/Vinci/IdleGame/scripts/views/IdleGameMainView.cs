using System;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class IdleGameMainView : View
{
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

    public void ShowLoginView()
    {
        loginView.gameObject.SetActive(true);
    }
}
