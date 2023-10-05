using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;
using System;

public class ArenaView : View
{
    [SerializeField]
    private Button _backButton;

    [SerializeField]
    private Button _registerCompetitionButton;
    public event Action registerCompetition;

    public override void Initialize()
    {
        _backButton.onClick.AddListener(() => ViewManager.ShowLast());
        _registerCompetitionButton.onClick.AddListener(() => registerCompetition?.Invoke());
    }
}
