using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;

public class HeadquartersView : View
{
    [SerializeField]
    private Button _backButton;

    public override void Initialize()
    {
        _backButton.onClick.AddListener(() => ViewManager.ShowLast());
    }
}
