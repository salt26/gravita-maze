using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonWithText : Button
{
    private bool isDisabledOrPressed = false;
    Vector3 initialTextPos;
    Text textComponent;

    private static Color basicColor = Color.black;
    private static Color highlightedColor = new Color(191f / 255f, 0f, 165f / 255f);
    private static Color highlightedPressedColor = new Color(167f / 255f, 0f, 144f / 255f);

    private bool _isHighlighted = true;

    public bool IsTextHighlighted
    {
        get
        {
            return _isHighlighted;
        }
        set
        {
            _isHighlighted = value;
            UpdateChildren();
        }
    }

    private void Update()
    {
        if (!isDisabledOrPressed &&
            (!interactable || currentSelectionState == SelectionState.Pressed ||
            currentSelectionState == SelectionState.Disabled))
        {
            isDisabledOrPressed = true;
            UpdateChildren();
        }
        else if (isDisabledOrPressed && interactable &&
            currentSelectionState != SelectionState.Pressed &&
            currentSelectionState != SelectionState.Disabled)
        {
            isDisabledOrPressed = false;
            UpdateChildren();
        }
    }

    public void UpdateChildren()
    {
        if (textComponent == null || initialTextPos == null)
        {
            textComponent = GetComponentInChildren<Text>();
            initialTextPos = new Vector3(60f, 6f);
        }

        if (isDisabledOrPressed)
        {
            textComponent.rectTransform.localPosition = initialTextPos - new Vector3(0f, 12f);
            if (_isHighlighted)
            {
                textComponent.color = highlightedPressedColor;
            }
            else
            {
                textComponent.color = basicColor;
            }
        }
        else
        {
            textComponent.rectTransform.localPosition = initialTextPos;
            if (_isHighlighted)
            {
                textComponent.color = highlightedColor;
            }
            else
            {
                textComponent.color = basicColor;
            }
        }
    }
}
