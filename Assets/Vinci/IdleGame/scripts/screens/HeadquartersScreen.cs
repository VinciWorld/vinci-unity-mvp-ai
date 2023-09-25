using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadquartersScreen : Screen
{
    [SerializeField]
    private Button _btnBack;

    public override void Initialize()
    {
        _btnBack.onClick.AddListener(() => ScreenManager.ShowLast());
    }
}
