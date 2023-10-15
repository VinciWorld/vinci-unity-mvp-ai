using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vinci.Core.UI;
using System;
using Vinci.Core.Managers;

public class ArenaView : View
{
    [SerializeField]
    private Button _backButton;


    [SerializeField]
    private GameObject _competitionsListRoot;
    [SerializeField]
    private GameObject _competitionsListPrefab;

    [SerializeField]
    private GameObject _competitionsPlayersScoresRoot;
    [SerializeField]
    private GameObject _competitionPlayerScorePrefab;

    [SerializeField]
    private Button _registerCompetitionButton;
    [SerializeField]
    private Button _playCompetitionButton;



    public event Action registerCompetitionButtonPressed;
    public event Action playGameButtonPressed;
    public event Action backButtonPressed;


    public override void Initialize()
    {
        _backButton.onClick.AddListener(() => backButtonPressed.Invoke());
        _registerCompetitionButton.onClick.AddListener(() => registerCompetitionButtonPressed?.Invoke());
        _playCompetitionButton.onClick.AddListener(() => playGameButtonPressed?.Invoke());


    }

    private void OnEnable() {

        CheckIfPlayerIsRegistered();
    }

    public void ShowButtonPlayer()
    {
        _registerCompetitionButton.gameObject.SetActive(false);
        _playCompetitionButton.gameObject.SetActive(true);
    }

    public void PopulatePlayersScores(string name, int score, Sprite image=null)
    {
        RemoveAllChildObjects(_competitionsPlayersScoresRoot.transform);

        GameObject scoreListItemObj = Instantiate(_competitionPlayerScorePrefab, _competitionsPlayersScoresRoot.transform);
        ScoreListItem scoreListItem = scoreListItemObj.GetComponent<ScoreListItem>();
        scoreListItem.SetData(image, name, score);
    }

    void RemoveAllChildObjects(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }
    }

    public void CheckIfPlayerIsRegistered()
    {
        _registerCompetitionButton.gameObject.SetActive(true);
        if (GameManager.instance.playerData.isPlayerRegisteredOnCompetition)
        {
            _registerCompetitionButton.gameObject.SetActive(false);
            _playCompetitionButton.gameObject.SetActive(true);
        }
    }
}
