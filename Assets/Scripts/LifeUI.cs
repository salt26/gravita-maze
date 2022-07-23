using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeUI : MonoBehaviour
{
    public PlayManager pm;
    public Image lifeLabel10;
    public Image lifeLabel1;
    public List<Sprite> numberLabels = new List<Sprite>();

    // Update is called once per frame
    void Update()
    {

        if (!pm.IsReady)
        {
            lifeLabel10.sprite = numberLabels[0];
            lifeLabel1.sprite = numberLabels[0];
            return;
        }

        if (pm.Life > 99)
        {
            lifeLabel10.sprite = numberLabels[9];
            lifeLabel1.sprite = numberLabels[9];
        }
        else
        {
            lifeLabel10.sprite = numberLabels[pm.Life / 10];
            lifeLabel1.sprite = numberLabels[pm.Life % 10];
        }
    }
}
