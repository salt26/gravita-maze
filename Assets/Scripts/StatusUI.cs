using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    public Text statusText;

    private float timer = 0f;
    private string reservedText = "";

    void Awake()
    {
        statusText.text = "";
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0f;
                if (reservedText is null)
                {
                    statusText.text = "";
                }
                else
                {
                    statusText.text = reservedText;
                }
                reservedText = "";
            }
        }
    }

    public void SetStatusMessage(string message)
    {
        if (timer <= 0f)
        {
            timer = 0f;
            reservedText = "";
            if (message is null)
            {
                statusText.text = "";
                return;
            }
            statusText.text = message;
        }
        else
        {
            if (message is null)
            {
                reservedText = "";
            }
            else
            {
                reservedText = message;
            }
        }
    }

    /// <summary>
    /// timer초 동안 상태 바에 instantMessage를 출력하고, 그 후에는 reservedMessage를 출력합니다.
    /// </summary>
    /// <param name="instantMessage"></param>
    /// <param name="reservedMessage"></param>
    /// <param name="timer"></param>
    public void SetStatusMessageWithReservation(string instantMessage, string reservedMessage, float timer)
    {
        if (timer <= 0f)
        {
            if (reservedMessage is null)
            {
                statusText.text = "";
            }
            else
            {
                statusText.text = reservedMessage;
            }
        }
        else
        {
            this.timer = timer;
            reservedText = reservedMessage;
            if (instantMessage is null)
            {
                statusText.text = "";
            }
            else
            {
                statusText.text = instantMessage;
            }
        }
    }

    /// <summary>
    /// timer초 동안 상태 바에 instantMessage를 출력하고, 그 후에는 이전에 출력하고 있던 메시지를 출력합니다.
    /// </summary>
    /// <param name="instantMessage"></param>
    /// <param name="timer"></param>
    public void SetStatusMessageWithFlashing(string instantMessage, float timer)
    {
        if (timer <= 0f) return;

        this.timer = timer;
        if (reservedText is null || reservedText == "")
        {
            reservedText = statusText.text;
        }
        if (instantMessage is null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = instantMessage;
        }
    }
}
