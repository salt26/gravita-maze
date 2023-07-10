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

    private Color stoppedColor = new Color(1, 137/255f, 1);
    private Color flowingColor1 = new Color(178/255f, 0, 1);
    private Color flowingColor2 = new Color(211/255f, 110/255f, 1);

    private Color warningColor1_phase1 = new Color(223/255f, 55/255f, 102/255f);
    private Color warningColor1_phase2 = new Color(213/255f, 35/255f, 74/255f);

    private Color warningColor2_phase1 = new Color(1, 63/255f, 117/255f);
    private Color warningColor2_phase2 = new Color(244/255f, 41/255f, 85/255f);

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

            int remainingTimeInt = Mathf.CeilToInt(mm.RemainingTime);

            if (remainingTimeInt <= 10)
            {
                switch (remainingTimeInt)
                {
                    case > 5:
                        timerLabel10.color = warningColor2_phase1;
                        timerLabel1.color = warningColor2_phase1;
                        timerBar.color = warningColor1_phase1;
                        break;
                    case <= 5:
                        timerLabel10.color = warningColor2_phase2;
                        timerLabel1.color = warningColor2_phase2;
                        timerBar.color = warningColor1_phase2;
                        break;
                }
            }
            else
            {
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
}
