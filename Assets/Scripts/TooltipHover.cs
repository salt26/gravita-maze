using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TooltipHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum Pivot { TopRight = 0, BottomRight = 1, TopLeft = 2 }

    public GameObject tooltipPrefab;
    public string tooltipMessage;
    public float tooltipWidth;
    public float tooltipHeight;
    public Pivot pivot;

    RectTransform myTransform;
    TooltipBox myTooltipUI;
    Button button;
    EditorManager em;
    float lastEnterTime;

    void Awake()
    {
        myTooltipUI = null;
        button = GetComponent<Button>();
        myTransform = GetComponent<RectTransform>();
        em = GameObject.FindGameObjectWithTag("EditorManager").GetComponent<EditorManager>();
        lastEnterTime = -1f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        lastEnterTime = Time.time;
        if (!button.interactable && myTooltipUI == null)
        {
            myTooltipUI = Instantiate(tooltipPrefab, em.tooltipUI.transform).GetComponent<TooltipBox>();
            switch (pivot)
            {
                case Pivot.TopRight:
                    myTooltipUI.Initialize(myTransform.localPosition + new Vector3(myTransform.rect.width / 2f, myTransform.rect.height / 2f - 24),
                        tooltipWidth, tooltipHeight, (TooltipBox.Pivot)pivot, tooltipMessage);
                    break;
                case Pivot.BottomRight:
                    myTooltipUI.Initialize(myTransform.localPosition + new Vector3(myTransform.rect.width / 2f, -myTransform.rect.height / 2f + 12),
                        tooltipWidth, tooltipHeight, (TooltipBox.Pivot)pivot, tooltipMessage);
                    break;
                case Pivot.TopLeft:
                    myTooltipUI.Initialize(myTransform.localPosition + new Vector3(-myTransform.rect.width / 2f, myTransform.rect.height / 2f - 24),
                        tooltipWidth, tooltipHeight, (TooltipBox.Pivot)pivot, tooltipMessage);
                    break;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(HideTooltip());
    }

    IEnumerator HideTooltip()
    {
        while (Time.time < lastEnterTime + 0.8f)
        {
            yield return null;
        }
        if (myTooltipUI != null)
        {
            Destroy(myTooltipUI.gameObject);
            myTooltipUI = null;
        }
    }
}
