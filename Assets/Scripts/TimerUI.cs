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
    private Color warningColor1_phase3 = new Color(175/255f, 29/255f, 61/255f);
    private Color warningColor1_phase4 = new Color(138/255f, 20/255f, 37/255f);
    private Color warningColor1_phase5 = new Color(106/255f, 0, 6/255f);

    private Color warningColor2_phase1 = new Color(1, 63/255f, 117/255f);
    private Color warningColor2_phase2 = new Color(244/255f, 41/255f, 85/255f);
    private Color warningColor2_phase3 = new Color(201/255f, 34/255f, 70/255f);
    private Color warningColor2_phase4 = new Color(158/255f, 23/255f, 43/255f);
    private Color warningColor2_phase5 = new Color(122/255f, 0, 8/255f);

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
                int remainingTimeInt = Mathf.CeilToInt(mm.RemainingTime);

                switch (remainingTimeInt)
                {
                    case 10 or 9:
                        timerLabel10.color = warningColor2_phase1;
                        timerLabel1.color = warningColor2_phase1;
                        timerBar.color = warningColor1_phase1;
                        break;
                    case 8 or 7:
                        timerLabel10.color = warningColor2_phase2;
                        timerLabel1.color = warningColor2_phase2;
                        timerBar.color = warningColor1_phase2;
                        break;
                    case 6 or 5:
                        timerLabel10.color = warningColor2_phase3;
                        timerLabel1.color = warningColor2_phase3;
                        timerBar.color = warningColor1_phase3;
                        break;
                    case 4 or 3:
                        timerLabel10.color = warningColor2_phase4;
                        timerLabel1.color = warningColor2_phase4;
                        timerBar.color = warningColor1_phase4;
                        break;
                    case 2 or 1 or 0:
                        timerLabel10.color = warningColor2_phase5;
                        timerLabel1.color = warningColor2_phase5;
                        timerBar.color = warningColor1_phase5;
                        break;
                    default:
                        timerLabel10.color = flowingColor2;
                        timerLabel1.color = flowingColor2;
                        timerBar.color = flowingColor1;
                        break;
                }
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
