using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveLimitUI : MonoBehaviour
{
    public MapManager mm;
    public Image moveLimitBar;
    public Image moveLimitLabel100;
    public Image moveLimitLabel10;
    public Image moveLimitLabel1;
    public List<Sprite> numberLabels = new List<Sprite>();

    // Update is called once per frame
    void Update()
    {
        float pixels = 744 / 12f;

        if (!mm.IsReady)
        {
            moveLimitLabel100.sprite = numberLabels[0];
            moveLimitLabel10.sprite = numberLabels[0];
            moveLimitLabel1.sprite = numberLabels[0];
            moveLimitBar.GetComponent<RectTransform>().offsetMax = new Vector2(
                -240 - pixels * 12,
                moveLimitBar.GetComponent<RectTransform>().offsetMax.y);

            return;
        }

        print(mm.MoveLimit + " " + mm.ActionHistory + " " + mm.ActionHistory.Length);
        int remainingMove = mm.MoveLimit - mm.ActionHistory.Length;
        if (remainingMove < 0)
        {
            remainingMove = 0;
        }

        if (remainingMove > 999f)
        {
            moveLimitLabel100.sprite = numberLabels[9];
            moveLimitLabel10.sprite = numberLabels[9];
            moveLimitLabel1.sprite = numberLabels[9];
        }
        else
        {
            moveLimitLabel100.sprite = numberLabels[remainingMove / 100];
            moveLimitLabel10.sprite = numberLabels[remainingMove % 100 / 10];
            moveLimitLabel1.sprite = numberLabels[remainingMove % 10];
        }

        if (mm.MoveLimit <= 0f)
        {
            moveLimitBar.GetComponent<RectTransform>().offsetMax = new Vector2(
                -240 - pixels * 12,
                moveLimitBar.GetComponent<RectTransform>().offsetMax.y);
        }
        else
        {
            int t = Mathf.CeilToInt(Mathf.Clamp01((float)remainingMove / mm.MoveLimit) * pixels);

            moveLimitBar.GetComponent<RectTransform>().offsetMax = new Vector2(
                -240 - (pixels - t) * 12,
                moveLimitBar.GetComponent<RectTransform>().offsetMax.y);
        }
    }
}
