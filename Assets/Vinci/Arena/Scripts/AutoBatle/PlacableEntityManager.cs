using System;
using System.Collections.Generic;
using UnityEngine;
using Vinci.Core.Managers;

public class PlacableEntityManager : MonoBehaviour 
{
    public LayerMask playingFieldMask;
    public GameObject agentCardPrefab;
    public MeshRenderer forbiddenAreaRenderer;

    public event Action<AgentConfig, Vector3> agentDeployed;

    public RectTransform cardsParent;

    public GameObject agentPrefab;

    ArenaGameController _arena;

    public List<AgentCard> cards = new List<AgentCard>();

    private bool cardIsActive = false;
    private GameObject previewHolder;

    private List<GameObject> agentCardsSlots = new();

    void Awake()
    {
        previewHolder = new GameObject("PreviewHolder");
    }

    public void Init(ArenaGameController arena)
    {
        _arena = arena;
    }

    public void LoadAgentsCard()
    {
        List<AgentConfig> agentsConfig = GameManager.instance.playerData.agents;

        Debug.Log("LoadAgentsCard");

        foreach (var agentConfig in agentsConfig)
        {

            GameObject agentCard = Instantiate(agentCardPrefab, cardsParent);
            agentCardsSlots.Add(agentCard);

            AgentCard card = agentCard.GetComponentInChildren<AgentCard>();

            cards.Add(card);

            card.InitialiseWithData(agentConfig);

            card.OnTapDownAction += CardTapped;
            card.OnDragAction += CardDragged;
            card.OnTapReleaseAction += CardReleased;
        }
    }

    public void RemoveCards()
    {
        cardIsActive = false;
        foreach (var agentCard in agentCardsSlots)
        {
            Destroy(agentCard);
        }
        cards.Clear();
    }

    private void CardTapped(int cardId)
    {
        if (!CheckCardPrice()) return;
        
        cards[cardId].GetComponent<RectTransform>().SetAsLastSibling();
        //forbiddenAreaRenderer.enabled = true;
    }

    private void CardDragged(int cardId, Vector2 dragAmount)
    {
        if (!CheckCardPrice()) return;
        cards[cardId].transform.Translate(dragAmount);

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        bool planeHit = Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask);


        if (planeHit)
        {
            if (!cardIsActive)
            {
                cardIsActive = true;
                previewHolder.transform.position = hit.point;
                cards[cardId].ChangeActiveState(true);

                AgentConfig agentConfig = cards[cardId].agentConfig;
                GameObject.Instantiate(agentPrefab, hit.point, Quaternion.identity, previewHolder.transform);
            }
            else
            {
                previewHolder.transform.position = hit.point;
            }
        }
        else
        {
            if (cardIsActive)
            {
                cardIsActive = false;
                cards[cardId].ChangeActiveState(false);

                ClearPreviewObjects();
            }
        }
    }

    private void CardReleased(int cardId)
    {
        if (!CheckCardPrice()) return;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask))
        {
            agentDeployed?.Invoke(cards[cardId].agentConfig, hit.point);

            ClearPreviewObjects();
            //Destroy(cards[cardId].gameObject);
            cards[cardId].ResetImage();
        }
        else
        {
            cards[cardId].ResetImage();
        }

       // forbiddenAreaRenderer.enabled = false;
    }

    private void ClearPreviewObjects()
    {
        //destroy all the preview Placeables
        for (int i = 0; i < previewHolder.transform.childCount; i++)
        {
            Destroy(previewHolder.transform.GetChild(i).gameObject);
        }
    }

    public bool CheckCardPrice()
    {
        if(_arena.currentCoins >= 100)
        {
            return true;
        }

        return false;
    }
}