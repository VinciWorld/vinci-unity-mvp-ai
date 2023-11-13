using System;
using UnityEngine;
using Vinci.Core.Utils;


public class PopupManager : PersistentSingleton<PopupManager>
{
    [SerializeField]
    PopupAdvance popoup;
    [SerializeField]
    GameObject canvas;

    private bool _isLoaderPopupActive;

    void Start()
    {
        popoup.closeButtonPressed += OnPopupClose;
    }

    public void ShowLoader(
        string title,
        string message,

        bool showPrimaryButton = false,
        string primaryButtonName = "Ok",
        Action primaryButtonCallback = null,
        bool showSecondaryButton = false,
        string secondaryButtonName = "Close",
        Action secondaryButtonCallback = null,
        bool showCloseButton = true,
        Action closeButtonCallback = null
    )
    {
        if (_isLoaderPopupActive)
            return;

        _isLoaderPopupActive = true;

        canvas.SetActive(true);
        Debug.Log(canvas.activeSelf);

        popoup.Show(
            title,
            message,
            PopupMode.LOADING,
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

    public void ShowInfo(
        string title,
        string message,

        bool showPrimaryButton = false,
        string primaryButtonName = "Ok",
        Action primaryButtonCallback = null,
        bool showSecondaryButton = false,
        string secondaryButtonName = "Close",
        Action secondaryButtonCallback = null,
        bool showCloseButton = true,
        Action closeButtonCallback = null

    )
    {

        if (_isLoaderPopupActive)
            return;

        canvas.SetActive(true);
        popoup.Show(
            title,
            message,
            PopupMode.INFO,
            showCloseButton,
            closeButtonCallback,
            showPrimaryButton,
            primaryButtonName,
            primaryButtonCallback,
            showSecondaryButton,
            secondaryButtonName,
            secondaryButtonCallback
        );
        _isLoaderPopupActive = true;
    }

    public void UpdateMessage(
        string title,
        string message,
        bool showActionButton = false,
        bool showCloseButton = false
    )
    {
        if(!_isLoaderPopupActive)
        {
            Debug.Log("Popup is not active");
        }

        popoup.SetTitle(title);
        popoup.SetMessage(message);
        popoup.ShowPrimaryButton(showActionButton);
        popoup.ShowSecondaryButton(showCloseButton);
    }

    public void Close()
    {
        if (!_isLoaderPopupActive)
            return;

        popoup.Close();        
        canvas.SetActive(false);
        _isLoaderPopupActive = false;
    }

    private void OnPopupClose()
    {
        _isLoaderPopupActive = false;
    }
}