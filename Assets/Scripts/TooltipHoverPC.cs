using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;

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
    GameObject tooltipUIParent;
    float lastEnterTime;
    
    void Awake()
    {
        myTooltipUI = null;
        button = GetComponent<Button>();
        myTransform = GetComponent<RectTransform>();
        tooltipUIParent = GameObject.FindGameObjectWithTag("TooltipUI");

        lastEnterTime = -1f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

#if !(UNITY_IOS || UNITY_ANDROID) || UNITY_EDITOR

        if (GetComponent<Button>() != null && !GetComponent<Button>().interactable) return;

        if (tooltipUIParent.transform.childCount >= 1)
        {
            for (int i = 0; i < tooltipUIParent.transform.childCount; i++)
            {
                Destroy(tooltipUIParent.transform.GetChild(i).gameObject);
            }
        }
        lastEnterTime = Time.time;
        if (myTooltipUI == null)
        {
            myTooltipUI = Instantiate(tooltipPrefab, tooltipUIParent.transform).GetComponent<TooltipBox>();

            switch (pivot)
            {
                case Pivot.TopRight:
                    if (tooltipMessage.Equals("NextButton") && SceneManager.GetActiveScene().name.Equals("Adventure") &&
                        GameManager.mm != null && GameManager.mm.IsReady && !GameManager.mm.HasCleared)
                    {
                        myTooltipUI.Initialize(myTransform.localPosition + new Vector3(myTransform.rect.width / 2f, myTransform.rect.height / 2f - 24),
                            tooltipWidth + 120f, tooltipHeight, (TooltipBox.Pivot)pivot, LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", "Non-clearedNextButton"));
                    }
                    else {
                        myTooltipUI.Initialize(myTransform.localPosition + new Vector3(myTransform.rect.width / 2f, myTransform.rect.height / 2f - 24),
                            tooltipWidth, tooltipHeight, (TooltipBox.Pivot)pivot, LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", tooltipMessage));
                    }
                    break;
                case Pivot.BottomRight:
                    myTooltipUI.Initialize(myTransform.localPosition + new Vector3(myTransform.rect.width / 2f, -myTransform.rect.height / 2f + 12),
                        tooltipWidth, tooltipHeight, (TooltipBox.Pivot)pivot, LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", tooltipMessage));
                    break;
                case Pivot.TopLeft:
                    myTooltipUI.Initialize(myTransform.localPosition + new Vector3(-myTransform.rect.width / 2f, myTransform.rect.height / 2f - 24),
                        tooltipWidth, tooltipHeight, (TooltipBox.Pivot)pivot, LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", tooltipMessage));
                    break;
                case Pivot.BottomLeft:
                    myTooltipUI.Initialize(myTransform.localPosition + new Vector3(-myTransform.rect.width / 2f, -myTransform.rect.height / 2f + 12),
                        tooltipWidth, tooltipHeight, (TooltipBox.Pivot)pivot, LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", tooltipMessage));
                    break;
            }
        }
#endif
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {

#if !(UNITY_IOS || UNITY_ANDROID) || UNITY_EDITOR

        StartCoroutine(HideTooltip());

#endif
    }

    IEnumerator HideTooltip()
    {
        while (Time.time < lastEnterTime + 0.4f)
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
