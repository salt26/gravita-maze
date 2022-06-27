using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputMessageUI : MessageUI
{
    public InputFieldInset messageInputField;

    void Update()
    {
        messageOKButton.interactable = messageInputField.text != null && messageInputField.text != "";
    }

    public override void Initialize(string text, UnityAction onOKClick = null, UnityAction onXClick = null)
    {
        base.Initialize(text, onOKClick, onXClick);
        messageOKButton.onClick.AddListener(() => messageInputField.text = "");
        messageXButton.onClick.AddListener(() => messageInputField.text = "");
    }
}
