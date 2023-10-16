using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
using Vinci.Core.Managers;



public class CategoryNavBar : MonoBehaviour
{
    [SerializeField]
    private Button _homeButton;
    [SerializeField]
    private Button _backButton;

    [SerializeField]
    private TextMeshProUGUI _title;
    [SerializeField]
    private TextMeshProUGUI _categoryLabel;

    [SerializeField]
    private TextMeshProUGUI _stepsAvailableAmount;


    public event Action backButtonPressed;
    public event Action homeButtonPressed;

    void OnEnable()
    {
        GameManager.instance.playerData.stepsAvailableChange += OnStepsAvailableChange;
        _stepsAvailableAmount.text = (GameManager.instance.playerData.availableSteps / 1000).ToString();
    }

    private void OnDisable() 
    {
        GameManager.instance.playerData.stepsAvailableChange -= OnStepsAvailableChange;
    }

    void OnStepsAvailableChange(int steps)
    {
        _stepsAvailableAmount.text = (steps / 1000).ToString();
    }

    public void SetTitles(string mainTitle, string categoryLabel)
    {
        _title.text = mainTitle;
        _categoryLabel.text = categoryLabel;
    }

    public void SetMainTitle(string mainTitle)
    {
        _title.text = mainTitle;
    }

    public void SetNavigationButtons(bool back_button=true, bool homeButton=true)
    {
        _backButton.gameObject.SetActive(back_button);
        _homeButton.gameObject.SetActive(homeButton);

        _backButton.onClick.AddListener(() => backButtonPressed?.Invoke());
        _homeButton.onClick.AddListener(() => homeButtonPressed?.Invoke());

    }

    public void RemoveListeners()
    {
        _backButton.onClick.RemoveAllListeners();
        _homeButton.onClick.RemoveAllListeners();
    }

    public void UpdateStepsAvailable(int steps)
    {
        _stepsAvailableAmount.text = steps.ToString();
    }
}
