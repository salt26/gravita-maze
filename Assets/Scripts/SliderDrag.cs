using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

// https://answers.unity.com/questions/1292038/ui-slider-end-slide-event.html
[Serializable]
public class SliderDrag : MonoBehaviour, IPointerUpHandler
{
    public UnityEvent onPointerUp;

    public void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp.Invoke();
    }
}