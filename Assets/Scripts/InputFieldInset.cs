using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputFieldInset : InputField
{
    private bool isInset = false;

    private void Update()
    {

        if (!isInset &&
            (currentSelectionState == SelectionState.Highlighted ||
            currentSelectionState == SelectionState.Pressed ||
            currentSelectionState == SelectionState.Selected))
        {
            isInset = true;
            UpdateChildren();
        }
        else if (isInset &&
            !(currentSelectionState == SelectionState.Highlighted ||
            currentSelectionState == SelectionState.Pressed ||
            currentSelectionState == SelectionState.Selected))
        {
            isInset = false;
            UpdateChildren();
        }

        if (currentSelectionState == SelectionState.Selected)
        {
            placeholder.enabled = false;
        }
        if (currentSelectionState != SelectionState.Selected && text.Length == 0)
        {
            placeholder.enabled = true;
        }
    }

    public void UpdateChildren()
    {
        if (isInset)
        {
            textComponent.rectTransform.localPosition -= new Vector3(0f, 12f);
            placeholder.rectTransform.localPosition -= new Vector3(0f, 12f);
        }
        else
        {
            textComponent.rectTransform.localPosition += new Vector3(0f, 12f);
            placeholder.rectTransform.localPosition += new Vector3(0f, 12f);
        }
    }
}
