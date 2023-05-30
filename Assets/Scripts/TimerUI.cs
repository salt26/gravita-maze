using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public MapManager mm;
    public Image timerBar;
    public Image timerLabel10;
    public Image timerLabel1;
    public List<Sprite> numberLabels = new List<Sprite>();

    private Color stoppedColor = new Color(1f, 0f, 0.8627452f);
    private Color flowingColor1 = new Color(0.6980392f, 0f, 1f);
    private Color flowingColor2 = new Color(0.827451f, 0.4313726f, 1f);

    // Update is called once per frame
    void Update()
    {
        float pixels = (1080 - 240) / 12f;

        if (!mm.IsReady)
        {
            timerLabel10.sprite = numberLabels[0];
            timerLabel1.sprite = numberLabels[0];
            timerBar.GetComponent<RectTransform>().offsetMax = new Vector2(
                -168 - pixels * 12,
                timerBar.GetComponent<RectTransform>().offsetMax.y);

            timerLabel10.color = stoppedColor;
            timerLabel1.color = stoppedColor;
            timerBar.color = stoppedColor;
            return;
        }

        if (mm.RemainingTime > 99f)
        {
            timerLabel10.sprite = numberLabels[9];
            timerLabel1.sprite = numberLabels[9];
        }
        else
        {
            timerLabel10.sprite = numberLabels[Mathf.CeilToInt(mm.RemainingTime) / 10];
            timerLabel1.sprite = numberLabels[Mathf.CeilToInt(mm.RemainingTime) % 10];
        }

        if (mm.TimeLimit <= 0f)
        {
            timerBar.GetComponent<RectTransform>().offsetMax = new Vector2(
                -168 - pixels * 12,
                timerBar.GetComponent<RectTransform>().offsetMax.y);

            timerLabel10.color = stoppedColor;
            timerLabel1.color = stoppedColor;
            timerBar.color = stoppedColor;
        }
        else
        {
            int t = Mathf.CeilToInt(Mathf.Clamp01(mm.RemainingTime / mm.TimeLimit) * pixels);

            timerBar.GetComponent<RectTransform>().offsetMax = new Vector2(
                -168 - (pixels - t) * 12,
                timerBar.GetComponent<RectTransform>().offsetMax.y);

            if (mm.DoesTimeGoBy)
            {
                timerLabel10.color = flowingColor2;
                timerLabel1.color = flowingColor2;
                timerBar.color = flowingColor1;
            }
            else
            {
                timerLabel10.color = stoppedColor;
                timerLabel1.color = stoppedColor;
                timerBar.color = stoppedColor;
            }
        }
    }
}
