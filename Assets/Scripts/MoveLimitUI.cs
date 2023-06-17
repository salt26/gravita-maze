using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveLimitUI : MonoBehaviour
{
    public MapManager mm;
    public GameObject moveLimitDelimitersParent;
    public GameObject moveLimitDelimiterPrefab;
    public Image moveLimitBar;
    public Image moveLimitLabel100;
    public Image moveLimitLabel10;
    public Image moveLimitLabel1;
    public List<Sprite> numberLabels = new List<Sprite>();

    private int oldMoveLimit = 0;
    private List<GameObject> moveLimitDelimiters = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        const float pixels = 744 / 12f;

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

        if (mm.MoveLimit <= 0)
        {
            if (oldMoveLimit != mm.MoveLimit)
            {
                oldMoveLimit = mm.MoveLimit;
                foreach (GameObject delimiter in moveLimitDelimiters)
                {
                    Destroy(delimiter);
                }
                moveLimitDelimiters.Clear();
            }

            moveLimitBar.GetComponent<RectTransform>().offsetMax = new Vector2(
                -240 - pixels * 12,
                moveLimitBar.GetComponent<RectTransform>().offsetMax.y);
        }
        /*
        else
        {
            int t = Mathf.CeilToInt(Mathf.Clamp01((float)remainingMove / mm.MoveLimit) * pixels);

            moveLimitBar.GetComponent<RectTransform>().offsetMax = new Vector2(
                -240 - (pixels - t) * 12,
                moveLimitBar.GetComponent<RectTransform>().offsetMax.y);
        }
        */
        else if (mm.MoveLimit == 1)
        {
            if (oldMoveLimit != mm.MoveLimit)
            {
                oldMoveLimit = mm.MoveLimit;
                foreach (GameObject delimiter in moveLimitDelimiters)
                {
                    Destroy(delimiter);
                }
                moveLimitDelimiters.Clear();
            }

            moveLimitBar.GetComponent<RectTransform>().offsetMax = new Vector2(
                -984 + pixels * remainingMove * 12,
                moveLimitBar.GetComponent<RectTransform>().offsetMax.y);
        }
        else
        {
            int d = SegmentNum(mm.MoveLimit);                                               // 칸의 총 개수
            int p;                                                                          // 불이 들어와야 하는 칸 수
            if (mm.ActionHistory.Length == 0) p = d;
            else p = Mathf.CeilToInt(remainingMove / (float)(mm.MoveLimit - 1) * (d - 1));
            int dw;                                                                         // 구분자의 가로 길이(px)
            if (d % 2 == 0) dw = 2;     // 2의 배수
            else dw = 1;                // 3, 7, 9의 배수 혹은 5
            float pw;                                                                       // 한 칸의 가로 길이(px)
            if (d == 5) pw = 12f;       // d == 5 이면 맨 앞과 맨 뒤 칸은 11px, 중간은 모두 12px
            else if (d == 6) pw = 9f;   // d == 6 이면 맨 앞과 맨 뒤 칸은 7px, 중간은 모두 8px
            else pw = (62 - (d - 1) * dw) / d;  // 모든 칸이 동일한 정수 px
            float offset = 0f;
            if (d == 5 || d == 6) offset = 12f;

            if (oldMoveLimit != mm.MoveLimit)
            {
                oldMoveLimit = mm.MoveLimit;
                foreach (GameObject delimiter in moveLimitDelimiters)
                {
                    Destroy(delimiter);
                }
                moveLimitDelimiters.Clear();

                for (int i = 0; i < d - 1; i++)
                {
                    GameObject delimiter = Instantiate(moveLimitDelimiterPrefab, moveLimitDelimitersParent.transform);
                    float dLeft = 96 - offset + (pw + i * (dw + pw)) * 12f;
                    float dRight = 984 + offset - (i + 1) * (dw + pw) * 12f;
                    delimiter.GetComponent<RectTransform>().offsetMin = new Vector2(dLeft, delimiter.GetComponent<RectTransform>().offsetMin.y);
                    delimiter.GetComponent<RectTransform>().offsetMax = new Vector2(-dRight, delimiter.GetComponent<RectTransform>().offsetMax.y);
                    moveLimitDelimiters.Add(delimiter);
                }
            }
            moveLimitBar.GetComponent<RectTransform>().offsetMax = new Vector2(-984 - offset + (p * pw + (p - 1) * dw) * 12f, moveLimitBar.GetComponent<RectTransform>().offsetMax.y);
        }

    }

    private int SegmentNum(int t)
    {
        int[] list = new int[] { 21, 16, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        foreach (int i in list)
        {
            if (i <= t) return i;
        }
        return 1;
    }
}
