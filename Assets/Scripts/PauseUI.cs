using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    public Button pauseSkipButton;
    public Button pauseReturnButton;
    public Button pauseExitButton;
    public Slider pauseBgmVolume;
    public Slider pauseSfxVolume;

    public float bgmVolume;
    public float sfxVolume;

    void Start(){

        bgmVolume = GameManager.gm.bgmVolume;
        sfxVolume = GameManager.gm.sfxVolume;
        
        transform.GetChild(0).transform.GetChild(5).GetComponent<Slider>().value = bgmVolume;
        transform.GetChild(0).transform.GetChild(7).GetComponent<Slider>().value = sfxVolume;
    }

    void Update()
    {
        if (GameManager.mm == null || !GameManager.mm.IsReady) return;
        if (GameManager.mm.IsTimeActivated && GameManager.mm.RemainingTime > 0f && !GameManager.mm.HasCleared)
        {
            pauseSkipButton.interactable = true;
        }
        else if (GameManager.mm.IsTimeActivated && (GameManager.mm.RemainingTime <= 0f || GameManager.mm.HasCleared))
        {
            pauseSkipButton.interactable = false;
        }
        
    }
}
