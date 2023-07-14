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

    private Color normalColor_flow = new Color(195/255f, 90/255f, 244/255f);
    private Color normalColor_stop = new Color(225/255f, 158/255f, 1);

    private Color warningColor_phase1_flow_bright = new Color(224/255f, 0, 209/255f);
    private Color warningColor_phase1_flow_dark = new Color(195/255f, 0, 182/255f);
    private Color warningColor_phase1_stop = new Color(1, 81/255f, 249/255f);

    private Color warningColor_phase2_flow_bright = new Color(244/255f, 41/255f, 85/255f);
    private Color warningColor_phase2_flow_dark = new Color(213/255f, 35/255f, 74/255f);
    private Color warningColor_phase2_stop = new Color(1, 94/255f, 137/255f);

    private int blink_frequency = 2;

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

            timerLabel10.color = normalColor_stop;
            timerLabel1.color = normalColor_stop;
            timerBar.color = normalColor_stop;
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

            timerLabel10.color = normalColor_stop;
            timerLabel1.color = normalColor_stop;
            timerBar.color = normalColor_stop;
        }
        else
        {
            int t = Mathf.CeilToInt(Mathf.Clamp01(mm.RemainingTime / mm.TimeLimit) * pixels);

            timerBar.GetComponent<RectTransform>().offsetMax = new Vector2(
                -168 - (pixels - t) * 12,
                timerBar.GetComponent<RectTransform>().offsetMax.y);

            int remainingTimeInt = Mathf.FloorToInt(mm.RemainingTime);
            int remainingTimeFloatQuotient = Mathf.FloorToInt((mm.RemainingTime - remainingTimeInt) * blink_frequency);

            if (mm.DoesTimeGoBy)
            {
                switch (remainingTimeInt)
                {
                    case < 10 and >= 5:
                        if (remainingTimeFloatQuotient % 2 == 1) {
                            timerLabel10.color = warningColor_phase1_flow_bright;
                            timerLabel1.color = warningColor_phase1_flow_bright;
                            timerBar.color = warningColor_phase1_flow_bright;
                            break;
                        }
                        else {
                            timerLabel10.color = warningColor_phase1_flow_dark;
                            timerLabel1.color = warningColor_phase1_flow_dark;
                            timerBar.color = warningColor_phase1_flow_dark;
                            break;
                        }
                    case < 5:
                        if (remainingTimeFloatQuotient % 2 == 1) {
                            timerLabel10.color = warningColor_phase2_flow_bright;
                            timerLabel1.color = warningColor_phase2_flow_bright;
                            timerBar.color = warningColor_phase2_flow_bright;
                            break;
                        }
                        else {
                            timerLabel10.color = warningColor_phase2_flow_dark;
                            timerLabel1.color = warningColor_phase2_flow_dark;
                            timerBar.color = warningColor_phase2_flow_dark;
                            break;
                        }
                    default:
                        timerLabel10.color = normalColor_flow;
                        timerLabel1.color = normalColor_flow;
                        timerBar.color = normalColor_flow;
                        break;
                }
            }
            else
            {
                switch (remainingTimeInt)
                {
                    case < 10 and >= 5:
                        timerLabel10.color = warningColor_phase1_stop;
                        timerLabel1.color = warningColor_phase1_stop;
                        timerBar.color = warningColor_phase1_stop;
                        break;
                    case < 5:
                        timerLabel10.color = warningColor_phase2_stop;
                        timerLabel1.color = warningColor_phase2_stop;
                        timerBar.color = warningColor_phase2_stop;
                        break;
                    default:
                        timerLabel10.color = normalColor_stop;
                        timerLabel1.color = normalColor_stop;
                        timerBar.color = normalColor_stop;
                        break;
                }
            }
        }
    }
}
