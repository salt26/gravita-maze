using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using Interhaptics.Internal;

public class SettingManager : MonoBehaviour
{
    public GameObject CreditPanel;
    public Button QuitButton;
    public Button CreditButton;
    public Button BackSettingButton;

    public bool isCredit=false;
    public bool isOnce = false;

    public Dropdown languageSetting;
    Animator PanelAnimation;

    void Start()
    {
        if (SceneManager.GetActiveScene().name.Equals("Setting"))
        {
            PanelAnimation = CreditPanel.GetComponent<Animator>();
            CreditPanel.SetActive(false);
        }
        string localeRaw = LocalizationSettings.SelectedLocale.ToString();
        string locale = localeRaw.Substring(0, localeRaw.IndexOf('(')).TrimEnd(' ');
        languageSetting.value = (int)(GameManager.Language)System.Enum.Parse(typeof(GameManager.Language), locale);
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name.Equals("First")) return;

        if (isCredit)
        {
            if (!isOnce)
            {
                isOnce = true;
                QuitButton.interactable = false;
                CreditButton.interactable = false;
                CreditPanel.SetActive(true);
                StartCoroutine(SoundEffect());
            }

            if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Return))
            {
                isOnce = false;
                isCredit = false;
                StartCoroutine(OffCreditAnimation());
                GameManager.gm.PlayButtonSFX();
            }
        }
        else
        {
            if (QuitButton.interactable && (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Return)))
            {
                GameManager.gm.PlayButtonSFX();
                GameManager.gm.LoadMain();
            }
        }
    }

    IEnumerator SoundEffect()
    {
        yield return new WaitForSeconds(56.0f/60.0f);
        GameManager.gm.PlayFallSFX(1.0f);
        yield return new WaitForSeconds(23.0f/60.0f);
        GameManager.gm.PlayFallSFX(0.75f);
        yield return new WaitForSeconds(14.0f/60.0f);
        GameManager.gm.PlayFallSFX(0.5f);
        yield return new WaitForSeconds(4.0f/60.0f);
    }

    IEnumerator OffCreditAnimation()
    {
        PanelAnimation.SetTrigger("BackSettingDown");
        yield return new WaitForSeconds(1.5f);

        QuitButton.interactable = true;
        CreditButton.interactable = true;
        CreditPanel.SetActive(false);
    }

    public void BackMainButtonDown()
    {
        GameManager.gm.LoadMain();
    }

    public void InSettingBackButtonDown()
    {
        isOnce = false;
        StartCoroutine(OffCreditAnimation());
        isCredit = false;
    }

    public void CreditButtonDown()
    {
        isCredit = true;
        CreditPanel.SetActive(true);
        StartCoroutine(SoundEffect());
    }

    public void ChangeLanguage()
    {
        GameManager.Language selected = (GameManager.Language)System.Enum.Parse(typeof(GameManager.Language), languageSetting.options[languageSetting.value].text);
        //Debug.Log(selected);

        switch (languageSetting.options[languageSetting.value].text)
        {
            case "English":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
                break;
            case "Korean":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("ko");
                break;
        }
    }

    public void PlayButtonSFX()
    {
        GameManager.gm.PlayButtonSFX();
    }

    public void PlayFallSFX(float volume)
    {
        GameManager.gm.PlayFallSFX(volume);
    }

    public void SetBGMVolume(Slider slider)
    {
        GameManager.gm.bgmVolume = slider.value;
    }

    public void SetSFXVolume(Slider slider)
    {
        GameManager.gm.sfxVolume = slider.value;
    }


}
