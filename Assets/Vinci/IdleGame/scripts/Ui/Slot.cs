using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IDropHandler
{
    public int id = 0;
    public event Action<int, string, DraggableItem> OnDropNft;

    public void OnDrop(PointerEventData eventData)
    {
        Image image = GetComponent<Image>();
        //image.raycastTarget = false;

        if (transform.childCount == 0)
        {
            
            GameObject dropped = eventData.pointerDrag;
            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            Debug.Log("SLOT pubkey: " + draggableItem.pubkeyNft);
            OnDropNft?.Invoke(id, draggableItem.pubkeyNft, draggableItem);
            draggableItem.parentAfterDrag = transform;
        }
    }
}
