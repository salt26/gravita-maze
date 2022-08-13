using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialGuideUI : MonoBehaviour
{
    public enum Pivot { TopRight = 0, BottomRight = 1, TopLeft = 2 }

    RectTransform myTransform;
    Text text;


    public void Initialize(Vector2 pivotPosition, float width, float height, Pivot pivot, string message)
    {
        text = GetComponentInChildren<Text>();
        myTransform = GetComponent<RectTransform>();
        switch (pivot)
        {
            case Pivot.TopRight:
                myTransform.localPosition = new Vector2(pivotPosition.x - width / 2f, pivotPosition.y + height / 2f);
                break;
            case Pivot.BottomRight:
                myTransform.localPosition = new Vector2(pivotPosition.x - width / 2f, pivotPosition.y - height / 2f);
                break;
            case Pivot.TopLeft:
                myTransform.localPosition = new Vector2(pivotPosition.x + width / 2f, pivotPosition.y + height / 2f);
                break;
        }
        myTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        myTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        text.text = message.Replace('\\', '\n');
    }

}

