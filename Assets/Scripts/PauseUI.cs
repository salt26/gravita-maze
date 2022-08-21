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

    void Update(){
        
    }
    /*
    public virtual void Initialize(UnityAction onPassClick = null, UnityAction onReturnClick = null, UnityAction onExitClick = null){
        pausePassButton.onClick.RemoveAllListeners();
        pauseReturnButton.onClick.RemoveAllListeners();
        pauseReturnButton.onClick.RemoveAllListeners();

        if (onPassClick != null)
        {
        
            pauseExitButton.onClick.AddListener(onExitClick);
        }
        pausePassButton.onClick.AddListener(() => GameManager.gm.PlayButtonSFX());
        pausePassButton.onClick.AddListener(() => gameObject.SetActive(false));
        
        if (onReturnClick != null)
        {
            pauseReturnButton.onClick.AddListener(onReturnClick);
        }
        pauseReturnButton.onClick.AddListener(() => GameManager.gm.PlayButtonSFX());
        pauseReturnButton.onClick.AddListener(() => gameObject.SetActive(false));

        if (onExitClick != null)
        {
            pauseExitButton.onClick.AddListener(onExitClick);
        }
        pauseExitButton.onClick.AddListener(() => GameManager.gm.PlayButtonSFX());
        pauseExitButton.onClick.AddListener(() => gameObject.SetActive(false));

        gameObject.SetActive(true);


    }
    */
}
