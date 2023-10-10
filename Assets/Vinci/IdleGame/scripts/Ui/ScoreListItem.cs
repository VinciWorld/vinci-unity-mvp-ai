using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreListItem : MonoBehaviour
{
    [SerializeField]
    Image _avatar;
    [SerializeField]
    TextMeshProUGUI _playerNameText;
    [SerializeField]
    TextMeshProUGUI _scoreText;


    public void SetData(Sprite avatar, string playerName, int score)
    {
        _avatar.sprite = avatar;
        _playerNameText.text = playerName;
        _scoreText.text = score.ToString();
    }
}
