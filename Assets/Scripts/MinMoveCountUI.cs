using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinMoveCountUI : MonoBehaviour
{
    public MapManager mm;
    public Image minMoveCountLabel100;
    public Image minMoveCountLabel10;
    public Image minMoveCountLabel1;
    public List<Sprite> numberLabels = new List<Sprite>();


    // Update is called once per frame
    void Update()
    {
        if (!mm.IsReady)
        {
            minMoveCountLabel100.sprite = numberLabels[0];
            minMoveCountLabel10.sprite = numberLabels[0];
            minMoveCountLabel1.sprite = numberLabels[0];
            return;
        }

        if (mm.MoveLimit > 999)
        {
            minMoveCountLabel100.sprite = numberLabels[9];
            minMoveCountLabel10.sprite = numberLabels[9];
            minMoveCountLabel1.sprite = numberLabels[9];
        }
        else if (mm.MoveLimit < 0)
        {
            minMoveCountLabel100.sprite = numberLabels[0];
            minMoveCountLabel10.sprite = numberLabels[0];
            minMoveCountLabel1.sprite = numberLabels[0];
        }
        else
        {
            minMoveCountLabel100.sprite = numberLabels[mm.MoveLimit / 100];
            minMoveCountLabel10.sprite = numberLabels[(mm.MoveLimit % 100) / 10];
            minMoveCountLabel1.sprite = numberLabels[mm.MoveLimit % 10];
        }
    }


}
