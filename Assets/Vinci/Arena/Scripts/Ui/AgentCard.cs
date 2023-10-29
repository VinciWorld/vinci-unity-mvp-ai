using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class AgentCard : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public event Action<int, Vector2> OnDragAction;
    public event Action<int> OnTapDownAction, OnTapReleaseAction;

    [HideInInspector] public int AgentCardId;
    [HideInInspector] public AgentBlueprint agentConfig;

    public Image portraitImage; //Inspector-set reference
    private CanvasGroup canvasGroup;

    [SerializeField]
    private TextMeshProUGUI priceValue;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    //called by CardManager, it feeds CardData so this card can display the placeable's portrait
    public void InitialiseWithData(AgentBlueprint agentConfig)
    {
        this.agentConfig = agentConfig;
        //portraitImage.sprite = this.agentConfig.agentImage;
        priceValue.text = agentConfig.AgentPrice.ToString();
    }

    public void OnPointerDown(PointerEventData pointerEvent)
    {
        if (OnTapDownAction != null)
            OnTapDownAction(AgentCardId);
    }

    public void OnDrag(PointerEventData pointerEvent)
    {
        if (OnDragAction != null)
            OnDragAction(AgentCardId, pointerEvent.delta);
    }

    public void OnPointerUp(PointerEventData pointerEvent)
    {
        if (OnTapReleaseAction != null)
            OnTapReleaseAction(AgentCardId);
    }

    public void ChangeActiveState(bool isActive)
    {
        canvasGroup.alpha = (isActive) ? .05f : 1f;
    }

    public void ResetImage()
    {
        //portraitImage.sprite = this.agentConfig.agentImageHead;
        ChangeActiveState(false);
        GetComponent<RectTransform>().localPosition = Vector3.zero;

    }
}

