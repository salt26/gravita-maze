using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialGuideUI : MonoBehaviour
{
    RectTransform myTransform;
    Text text;


    public void Initialize(Vector2 position, float width, float height, string message)
    {
        text = GetComponentInChildren<Text>();
        //myTransform = GetComponent<RectTransform>();

        // myTransform.localPosition = new Vector2(position.x, position.y);

        //myTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        //myTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        text.text = message.Replace('\\', '\n');
    }

    public void changeColor(){
        text.color = Color.white;
    }

}

