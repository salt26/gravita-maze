using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TooltipHoverPC : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    public enum Pivot { TopRight = 0, BottomRight = 1, TopLeft = 2, BottomLeft = 3 }
    
    public GameObject tooltipPrefab;
    public string tooltipMessage;
    public float tooltipWidth;
    public float tooltipHeight;
    public Pivot pivot;
    
    RectTransform myTransform;
    TooltipBox myTooltipUI;
    Button button;
    PlayManager pm;
    float lastEnterTime;
    
    void Awake()
    {
        myTooltipUI = null;
        button = GetComponent<Button>();
        myTransform = GetComponent<RectTransform>();
        if (SceneManager.GetActiveScene().name.Equals("Tutorial"))
        {
            pm = GameObject.FindGameObjectWithTag("PlayManager").GetComponent<PlayManager>();
        }

        lastEnterTime = -1f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        #if (UNITY_IOS || UNITY_ANDROID)
        #else
        if (SceneManager.GetActiveScene().name.Equals("Tutorial"))
        {
            if (pm.tooltipUI.transform.childCount >= 1)
            {
                for (int i = 0; i < pm.tooltipUI.transform.childCount; i++)
                {
                    Destroy(pm.tooltipUI.transform.GetChild(i).gameObject);
                }
            }
        }
        lastEnterTime = Time.time;
        if (myTooltipUI == null)
        {
            if (SceneManager.GetActiveScene().name.Equals("Tutorial"))
            {
                myTooltipUI = Instantiate(tooltipPrefab, pm.tooltipUI.transform).GetComponent<TooltipBox>();
            }

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
                case Pivot.BottomLeft:
                    myTooltipUI.Initialize(myTransform.localPosition + new Vector3(-myTransform.rect.width / 2f, -myTransform.rect.height / 2f + 12),
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
    #endif
}
