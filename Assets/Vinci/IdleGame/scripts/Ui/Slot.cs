using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public int id = 0;
    public event Action<int, string> OnDropNft;

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            
            GameObject dropped = eventData.pointerDrag;
            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            Debug.Log("SLOT pubkey: " + draggableItem.pubkeyNft);
            OnDropNft?.Invoke(id, draggableItem.pubkeyNft);
            draggableItem.parentAfterDrag = transform;
        }
    }
}
