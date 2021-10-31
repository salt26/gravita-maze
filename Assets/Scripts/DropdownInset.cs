using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownInset : Dropdown
{
    private bool isInset = false;

    private void Update()
    {

        if (!isInset &&
            (currentSelectionState == SelectionState.Pressed))
        {
            isInset = true;
            UpdateChildren();
        }
        else if (isInset &&
            !(currentSelectionState == SelectionState.Pressed))
        {
            isInset = false;
            UpdateChildren();
        }
    }

    protected override GameObject CreateDropdownList(GameObject template)
    {
        GameObject g = base.CreateDropdownList(template);
        int step = g.GetComponentInChildren<DropdownScroll>().GetComponent<Scrollbar>().numberOfSteps;
        g.GetComponentInChildren<DropdownContent>().transform.localPosition = new Vector3(-12f, Mathf.Lerp(0f, 612f, (float)(value - 1) / (step - 1)));
        return g;
    }

    public void UpdateChildren()
    {
        if (isInset)
        {
            captionImage.rectTransform.localPosition -= new Vector3(0f, 12f);
            captionText.rectTransform.localPosition -= new Vector3(0f, 12f);
        }
        else
        {
            captionImage.rectTransform.localPosition += new Vector3(0f, 12f);
            captionText.rectTransform.localPosition += new Vector3(0f, 12f);
        }
    }
}
