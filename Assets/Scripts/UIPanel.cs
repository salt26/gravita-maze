using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIPanel : MonoBehaviour
{
    public Sprite panel1080x1920; // 9:16
    public Sprite panel1080x2160; // 9:18
    public Sprite panel1080x2220; // 9:18.5
    public Sprite panel1080x2280; // 9:19
    public Sprite panel1080x2340; // 9:19.5
    public Sprite panel1080x2400; // 9:20
    public Sprite panel1080x2460; // 9:20.5
    public Sprite panel1080x2520; // 9:21
    public Sprite panel1080x2640; // 9:22

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Screen.width / 9 * 16 == Screen.height)
        {
            // 1080x1920, 1440x2560, 540x960
            image.sprite = panel1080x1920;
            image.pixelsPerUnitMultiplier = 1 / 12f;
        }
        else if (Screen.width * 2 == Screen.height)
        {
            // 1080x2160, 1440x2880
            image.sprite = panel1080x2160;
            image.pixelsPerUnitMultiplier = 1 / 12f;
        }
        else if (Screen.width / 18 * 37 == Screen.height)
        {
            // 1080x2220, 1440x2960
            image.sprite = panel1080x2220;
            image.pixelsPerUnitMultiplier = 1 / 3f;
        }
        else if (Screen.width / 9 * 19 == Screen.height)
        {
            // 1080x2280, 1440x3040
            image.sprite = panel1080x2280;
            image.pixelsPerUnitMultiplier = 1 / 12f;
        }
        else if (Screen.width / 18 * 39 == Screen.height)
        {
            // 1080x2340, 1440x3120
            image.sprite = panel1080x2340;
            image.pixelsPerUnitMultiplier = 1 / 3f;
        }
        else if (Screen.width / 9 * 20 == Screen.height)
        {
            // 1080x2400, 1440x3200
            image.sprite = panel1080x2400;
            image.pixelsPerUnitMultiplier = 1 / 12f;
        }
        else if (Screen.width / 18 * 41 == Screen.height)
        {
            // 1080x2460, 1440x3280
            image.sprite = panel1080x2460;
            image.pixelsPerUnitMultiplier = 1 / 3f;
        }
        else if (Screen.width / 9 * 21 == Screen.height)
        {
            // 1080x2520, 1440x3360
            image.sprite = panel1080x2520;
            image.pixelsPerUnitMultiplier = 1 / 12f;
        }
        else if (Screen.width / 9 * 22 == Screen.height)
        {
            // 1080x2640, 1440x3520
            image.sprite = panel1080x2640;
            image.pixelsPerUnitMultiplier = 1 / 12f;
        }
        else
        {
            // Unsupported (ex: 1080x1440)
            image.sprite = panel1080x1920;
            image.pixelsPerUnitMultiplier = 1 / 12f;
        }
    }
}
