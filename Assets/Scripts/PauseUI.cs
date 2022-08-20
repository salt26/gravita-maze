using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    public Button pausePassButton;
    public Button pauseReturnButton;
    public Button pauseExitButton;
    public Slider pauseBgmVolumn;
    public Slider pauseSfxVolumn;

    public virtual void Initialize(UnityAction onPassClick = null, UnityAction onReturnClick = null, UnityAction onExitClick = null){
        pausePassButton.onClick.RemoveAllListeners();
        pauseReturnButton.onClick.RemoveAllListeners();
        pauseReturnButton.onClick.RemoveAllListeners();

        if (onExitClick != null)
        {
            pauseExitButton.onClick.AddListener(onExitClick);
        }
        pauseExitButton.onClick.AddListener(() => GameManager.gm.PlayButtonSFX());
        pauseExitButton.onClick.AddListener(() => gameObject.SetActive(false));
        
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
}
