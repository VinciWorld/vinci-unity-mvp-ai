using UnityEngine;
using UnityEngine.UI;
using Ricimi;
using TMPro;
using System;

public class LoaderPopup : Popup
{
    [SerializeField]
    public TextMeshProUGUI _processingMessage;
    [SerializeField]
    public TextMeshProUGUI _processingSubtitle;

    [SerializeField]
    private Button _closeButton;

    public event Action closeButtonPressed;



    private void OnEnable() {
        _closeButton.onClick.AddListener(OnClose);
    }

    public void OnClose()
    {
        closeButtonPressed?.Invoke();
        Close();
    }


    public void Show(string message, string subMessage, bool closeButton = false)
    {
        _processingMessage.text = message;
        _processingSubtitle.text = subMessage;

        if(_closeButton)
        {
            _closeButton.gameObject.SetActive(false);
        }
    }

    public void SetProcessingMEssage(string processingMessage)
    {
        _processingMessage.text = processingMessage;
    }

    public void SetProcessingSubtitle(string processingSubtitle)
    {
        _processingSubtitle.text = processingSubtitle;
    }
}