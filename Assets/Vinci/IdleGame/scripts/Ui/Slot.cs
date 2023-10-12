using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public int id = 0;
    public event Action<int> OnDropNft;

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            OnDropNft?.Invoke(id);
            GameObject dropped = eventData.pointerDrag;
            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            draggableItem.parentAfterDrag = transform;
        }
    }
}
