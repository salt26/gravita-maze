using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessageUI : MonoBehaviour
{
    public Text messageText;
    public Button messageOKButton;
    public Button messageXButton;

    public void Initialize(string text, UnityAction onOKClick = null, UnityAction onXClick = null)
    {
        messageText.text = text;
        messageOKButton.onClick.RemoveAllListeners();
        messageXButton.onClick.RemoveAllListeners();
        if (onOKClick != null)
        {
            messageOKButton.onClick.AddListener(onOKClick);
        }
        messageOKButton.onClick.AddListener(() => gameObject.SetActive(false));
        if (onXClick != null)
        {
            messageXButton.onClick.AddListener(onXClick);
        }
        messageXButton.onClick.AddListener(() => gameObject.SetActive(false));

        gameObject.SetActive(true);
    }

}
