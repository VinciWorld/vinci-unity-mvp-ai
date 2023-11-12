using UnityEngine;
using UnityEngine.UI;
using Ricimi;
using TMPro;
using System;


public enum PopupMode
{
    INFO,
    LOADING
}

public class PopupAdvance : Popup
{
    [SerializeField]
    private TextMeshProUGUI _titleText;
    [SerializeField]
    private TextMeshProUGUI _messageText;
    [SerializeField]
    private GameObject _loaderSpin;

    [SerializeField]
    private Button _closeButton;

    [SerializeField]
    private Button _primaryButton;
    [SerializeField]
    private TextMeshProUGUI _primaryButtonText;

    [SerializeField]
    private Button _secondaryButton;
    [SerializeField]
    private TextMeshProUGUI _secondaryButtonText;

    public event Action closeButtonPressed;

    private void OnEnable() 
    {
        _closeButton.onClick.AddListener(ClosePopup);
        _primaryButton.onClick.AddListener(ClosePopup);
        _secondaryButton.onClick.AddListener(ClosePopup);
    }

    private void OnDisable() {
        _closeButton.onClick.RemoveListener(ClosePopup);
        _primaryButton.onClick.RemoveListener(ClosePopup);
        _secondaryButton.onClick.RemoveListener(ClosePopup);
    }

    public void Show(
    string title,
        string message,
        PopupMode mode,
        bool showCloseButton = true,
        Action closeButtonCallback = null,
        bool showPrimaryButton = false,
        string primaryButtonName = "Ok",
        Action primaryButtonCallback = null,
        bool showSecondaryButton = false,
        string secondaryButtonName = "Close",
        Action secondaryButtonCallback = null
    )
    {   
        if(mode == PopupMode.INFO)
        {
            _loaderSpin.SetActive(false);
        }
        else if(mode == PopupMode.LOADING)
        {
            _loaderSpin.SetActive(true);
        }

        _Show(
            title,
            message,
            showCloseButton,
            closeButtonCallback,
            showPrimaryButton,
            primaryButtonName,
            primaryButtonCallback,
            showSecondaryButton,
            secondaryButtonName,
            secondaryButtonCallback
        );
    }

    private void _Show(
        string title,
        string message,
        bool showCloseButton = true,
        Action closeButtonCallback = null,
        bool showPrimaryButton = false,
        string primaryButtonName = "Ok",
        Action primaryButtonCallback = null,
        bool showSecondaryButton = false,
        string secondaryButtonName = "Close",
        Action secondaryButtonCallback = null
    )
    {
        _primaryButtonText.text = primaryButtonName;
        _secondaryButtonText.text = secondaryButtonName;


        _closeButton.gameObject.SetActive(showCloseButton);
        _primaryButton.gameObject.SetActive(showPrimaryButton);
        _secondaryButton.gameObject.SetActive(showSecondaryButton);

        if (closeButtonCallback != null)
        {
            _closeButton.onClick.AddListener(() => closeButtonCallback?.Invoke());
        }

        if (primaryButtonCallback != null)
        {
            _primaryButton.onClick.AddListener(() => primaryButtonCallback?.Invoke());
        }

        if (secondaryButtonCallback != null)
        {
            _secondaryButton.onClick.AddListener(() => secondaryButtonCallback?.Invoke());
           
        }

        _titleText.text = title;
        _messageText.text = message;

        Open();
    }

    public void ClosePopup()
    {
        _primaryButton.onClick.RemoveAllListeners();
        _secondaryButton.onClick.RemoveAllListeners();
        _closeButton.onClick.RemoveAllListeners();
        closeButtonPressed?.Invoke();
        Close();
    }

    public void ShowPrimaryButton(bool state)
    {
        _primaryButton.gameObject.SetActive(state);
    }
    public void ShowSecondaryButton(bool state)
    {
        _secondaryButton.gameObject.SetActive(state);
    }

    public void SetTitle(string title)
    {
        _titleText.text = title;
    }

    public void SetMessage(string message)
    {
        _messageText.text = message;
    }
}