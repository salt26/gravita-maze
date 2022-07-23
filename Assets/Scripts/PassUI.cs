using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassUI : MonoBehaviour
{
    public PlayManager pm;
    public Image passCurrentLabel10;
    public Image passCurrentLabel1;
    public Image passTotalLabel10;
    public Image passTotalLabel1;
    public List<Sprite> numberLabels = new List<Sprite>();

    // Update is called once per frame
    void Update()
    {

        if (!pm.IsReady)
        {
            passCurrentLabel10.sprite = numberLabels[0];
            passCurrentLabel1.sprite = numberLabels[0];
            passTotalLabel10.sprite = numberLabels[0];
            passTotalLabel1.sprite = numberLabels[0];
            return;
        }

        if (pm.PlayLength > 99)
        {
            passTotalLabel10.sprite = numberLabels[9];
            passTotalLabel1.sprite = numberLabels[9];
        }
        else
        {
            passTotalLabel10.sprite = numberLabels[pm.PlayLength / 10];
            passTotalLabel1.sprite = numberLabels[pm.PlayLength % 10];
        }

        if (pm.EscapedCount > 99)
        {
            passCurrentLabel10.sprite = numberLabels[9];
            passCurrentLabel1.sprite = numberLabels[9];
        }
        else
        {
            passCurrentLabel10.sprite = numberLabels[pm.EscapedCount / 10];
            passCurrentLabel1.sprite = numberLabels[pm.EscapedCount % 10];
        }
    }
}
