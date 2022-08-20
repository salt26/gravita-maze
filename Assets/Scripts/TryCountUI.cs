using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TryCountUI : MonoBehaviour
{
    public MapManager mm;
    public Image tryCountLabel100;
    public Image tryCountLabel10;
    public Image tryCountLabel1;
    public List<Sprite> numberLabels = new List<Sprite>(); 
    

    // Update is called once per frame
    void Update()
    {
        if (!mm.IsReady)
        {
            tryCountLabel100.sprite = numberLabels[0];
            tryCountLabel10.sprite = numberLabels[0];
            tryCountLabel1.sprite = numberLabels[0];
            return;
        }

        if (mm.tryCount> 999)
        {
            tryCountLabel100.sprite = numberLabels[9];
            tryCountLabel10.sprite = numberLabels[9];
            tryCountLabel1.sprite = numberLabels[9];
        }
        else
        {
            tryCountLabel100.sprite = numberLabels[mm.tryCount / 100];
            tryCountLabel10.sprite = numberLabels[(mm.tryCount % 100) / 10];
            tryCountLabel1.sprite = numberLabels[mm.tryCount % 10];
        }
    }
}
