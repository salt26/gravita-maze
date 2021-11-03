using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputFieldInset : InputField
{
    private bool isInset = false;
    private bool isDisabled = false;
    Vector3 initialTextPos;
    Vector3 initialPlaceholderPos;

    protected override void Start()
    {
        base.Start();
        initialTextPos = new Vector3(0f, 0f); // textComponent.rectTransform.localPosition;
        initialPlaceholderPos = new Vector3(0f, 0f); // textComponent.rectTransform.localPosition;
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
            (currentSelectionState == SelectionState.Highlighted ||
            currentSelectionState == SelectionState.Pressed ||
            currentSelectionState == SelectionState.Selected))
        {
            isInset = true;
            UpdateChildren();
        }
        else if (isInset && interactable &&
            !(currentSelectionState == SelectionState.Highlighted ||
            currentSelectionState == SelectionState.Pressed ||
            currentSelectionState == SelectionState.Selected))
        {
            isInset = false;
            UpdateChildren();
        }

        if (currentSelectionState == SelectionState.Selected || !interactable)
        {
            placeholder.enabled = false;
        }
        if (currentSelectionState != SelectionState.Selected && interactable && text.Length == 0)
        {
            placeholder.enabled = true;
        }
    }

    public void UpdateChildren()
    {
        if (isDisabled)
        {
            textComponent.rectTransform.localPosition = initialTextPos - new Vector3(0f, 12f);
            placeholder.rectTransform.localPosition = initialPlaceholderPos - new Vector3(0f, 12f);
        }
        else if (isInset)
        {
            textComponent.rectTransform.localPosition = initialTextPos - new Vector3(0f, 24f);
            placeholder.rectTransform.localPosition = initialPlaceholderPos - new Vector3(0f, 24f);
        }
        else
        {
            textComponent.rectTransform.localPosition = initialTextPos;
            placeholder.rectTransform.localPosition = initialTextPos;
        }
    }
}
