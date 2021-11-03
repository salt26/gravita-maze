using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownInset : Dropdown
{
    private bool isInset = false;
    private bool isDisabled = false;
    Vector3 initialImagePos;
    Vector3 initialTextPos;

    protected override void Start()
    {
        base.Start();
        initialImagePos = new Vector3(0f, 6f); // captionImage.rectTransform.localPosition;
        initialTextPos = new Vector3(0f, 0f); // captionText.rectTransform.localPosition;
    }

    private void Update()
    {
        if (!isDisabled && !interactable)
        {
            isDisabled = true;
            UpdateChildren();
        }
        else if (isDisabled && interactable)
        {
            isDisabled = false;
            UpdateChildren();
        }
        else if (!isInset && interactable &&
            (currentSelectionState == SelectionState.Pressed))
        {
            isInset = true;
            UpdateChildren();
        }
        else if (isInset && interactable &&
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
        if (isDisabled)
        {
            captionImage.rectTransform.localPosition = initialImagePos - new Vector3(0f, 12f);
            captionText.rectTransform.localPosition = initialTextPos - new Vector3(0f, 12f);
        }
        else if (isInset)
        {
            captionImage.rectTransform.localPosition = initialImagePos -  new Vector3(0f, 12f);
            captionText.rectTransform.localPosition = initialTextPos -  new Vector3(0f, 12f);
        }
        else
        {
            captionImage.rectTransform.localPosition = initialImagePos;
            captionText.rectTransform.localPosition = initialTextPos;
        }
    }
}
