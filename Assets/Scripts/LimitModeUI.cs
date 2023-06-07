using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class LimitModeUI : MonoBehaviour, IPointerClickHandler
{
    public Image handleImage;
    public Sprite timeSprite;
    public Sprite moveSprite;
    public UnityEvent onClick;
    private Slider slider;
    private float oldValue;
    //private bool isDragging;

    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        oldValue = slider.value;
        //isDragging = false;
        if (GameManager.mm != null)
        {
            if (oldValue < (slider.minValue + slider.maxValue) * 0.5f)
            {
                GameManager.mm.LimitMode = MapManager.LimitModeEnum.Time;
            }
            else
            {
                GameManager.mm.LimitMode = MapManager.LimitModeEnum.Move;
            }
        }
    }

    public void UpdateImage()
    {
        if (slider == null) return;

        if (slider.value < (slider.minValue + slider.maxValue) * 0.5f)
        {
            handleImage.sprite = timeSprite;
        }
        else if (slider.value > (slider.minValue + slider.maxValue) * 0.5f)
        {
            handleImage.sprite = moveSprite;
        }
        else // slider.value == (slider.minValue + slider.maxValue) * 0.5f
        {
            if (slider.value >= oldValue)
                handleImage.sprite = timeSprite;
            else 
                handleImage.sprite = moveSprite;
        }
        /*
        if (slider.value != oldValue)
        {
            print("isDragging");
            isDragging = true;
        }
        */
    }

    /*
    public void UpdateValue()
    {
        if (slider == null) return;

        if (slider.value < (slider.minValue + slider.maxValue) * 0.5f)
        {
            slider.value = slider.minValue;
        }
        else if (slider.value > (slider.minValue + slider.maxValue) * 0.5f)
        {
            slider.value = slider.maxValue;
        }
        else // slider.value == (slider.minValue + slider.maxValue) * 0.5f
        {
            if (slider.value >= oldValue)
                slider.value = slider.minValue;
            else
                slider.value = slider.maxValue;
        }
        oldValue = slider.value;
        isDragging = false;
    }
    */

    public void Toggle()
    {
        if (slider == null) return;

        if (oldValue < (slider.minValue + slider.maxValue) * 0.5f)
        {
            slider.value = slider.maxValue;
            handleImage.sprite = moveSprite;
        }
        else if (oldValue > (slider.minValue + slider.maxValue) * 0.5f)
        {
            slider.value = slider.minValue;
            handleImage.sprite = timeSprite;
        }
        oldValue = slider.value;

        if (GameManager.mm != null)
        {
            if (oldValue < (slider.minValue + slider.maxValue) * 0.5f)
            {
                GameManager.mm.LimitMode = MapManager.LimitModeEnum.Time;
            }
            else
            {
                GameManager.mm.LimitMode = MapManager.LimitModeEnum.Move;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick.Invoke();
    }
}
