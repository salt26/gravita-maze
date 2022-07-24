using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialGuide : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum Pivot { TopRight = 0, BottomRight = 1, TopLeft = 2 }

    public GameObject tooltipPrefab;
    public GameObject tutorialTip;
    public string tooltipMessage;
    public float tooltipWidth;
    public float tooltipHeight;
    public Pivot pivot;

    public Dictionary<TutorialTuple, string> tipDict = new Dictionary<TutorialTuple, string>();

    RectTransform myTransform;
    TooltipBox myTooltipUI;
    MapManager mm;

    void Start()
    {
        mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
        // myTooltipUI = Instantiate(tooltipPrefab, em.tooltipUI.transform).GetComponent<TooltipBox>();
        myTransform = GetComponent<RectTransform>();
        tipDict.Add(new TutorialTuple(1, 2, 0), "You can manipulate the direction of gravity using the arrow buttons. Press the left arrow to make the ball roll to the left!");
        tipDict.Add(new TutorialTuple(1, 0, 0), "Great job! Now, let's press the up arrow to make the ball roll up");
        tipDict.Add(new TutorialTuple(1, 0, 1), "Okay! Then let's get the ball out!");
        tipDict.Add(new TutorialTuple(2, 2, 0), "Next is a more complicated maze... Good luck!");
        tipDict.Add(new TutorialTuple(2, 0, 0), "Oh, that's a good choice");
        tipDict.Add(new TutorialTuple(2, 0, 2), "Oh, that's a good choice");
        tipDict.Add(new TutorialTuple(2, 1, 2), "We're almost there...");
        tipDict.Add(new TutorialTuple(2, 1, 1), "Here's only one step left!");
        tipDict.Add(new TutorialTuple(2, 0, 1), "Here's only one step left!");
        tipDict.Add(new TutorialTuple(3, 1, 2), "Fire can burn your balls...");
        tipDict.Add(new TutorialTuple(4, 2, 0), "A heavy Iron can crush a ball...");
        tipDict.Add(new TutorialTuple(4, 2, 1), "And also, the Iron can escape outside with the ball!");
        tipDict.Add(new TutorialTuple(4, 2, 2), "And also, the Iron can escape outside with the ball!");
        tipDict.Add(new TutorialTuple(4, 2, 0), "Now, you can just escape with Iron.");
        tipDict.Add(new TutorialTuple(5, 1, 1), "Also, a heavy Iron can make it go out regardless of the ball!");
        tipDict.Add(new TutorialTuple(6, 2, 2), "Finally, the Iron can cover the fire for a while!");
        tipDict.Add(new TutorialTuple(6, 2, 0), "Now that the Iron has blocked the fire, you can step on it!");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        /*
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
        */
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(HideTooltip());
    }

    IEnumerator HideTooltip()
    {
        while (true)
        {
            yield return null;
        }
        if (myTooltipUI != null)
        {
            Destroy(myTooltipUI.gameObject);
            myTooltipUI = null;
        }
    }
    bool isBallThere(Ball ball, List<int> positionIndex)
    {
        return true;
    }

    bool isIronthere(Iron box, List<int> positionIndex)
    {
        return true;
    }
}

