using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;

public class SettingManager : MonoBehaviour
{
    public enum Language { English = 0, 한국어 = 1 }

    public GameObject creditPanel;
    public Button quitButton;
    public Button creditButton;
    public Button backSettingButton;
    public RectTransform creditScrollContent;
    public RectTransform creditDescription;
    public RectTransform creditTeamLogo;
    public Image creditTitleImage;
    public Sprite creditTitleEnglish;
    public Sprite creditTitleKorean;

    public bool isCredit = false;

    public Dropdown languageSetting;
    Animator panelAnimation;

    void Start()
    {
        if (SceneManager.GetActiveScene().name.Equals("Setting"))
        {
            panelAnimation = creditPanel.GetComponent<Animator>();
            creditPanel.SetActive(false);
        }
        string localeRaw = LocalizationSettings.SelectedLocale.ToString();
        string locale;
        switch (localeRaw)
        {
            case "English (en)":
                locale = "English";
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
                break;
            case "Korean (ko)":
                locale = "한국어";
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("ko");
                break;
            default:
                locale = "Engilsh";
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
                break;
        }
        languageSetting.value = (int)(Language)System.Enum.Parse(typeof(Language), locale);
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name.Equals("First")) return;

        if (isCredit)
        {
            if (backSettingButton.interactable==true && (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Return)))
            {
                isCredit = false;
                StartCoroutine(OffCreditAnimation());
                GameManager.gm.PlayButtonSFX();
            }

            if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("en"))
            {
                creditTitleImage.sprite = creditTitleEnglish;
                creditScrollContent.sizeDelta = new Vector2(creditScrollContent.sizeDelta.x, 5760);
                creditDescription.sizeDelta = new Vector2(creditDescription.sizeDelta.x, 4800);
                creditTeamLogo.anchoredPosition = new Vector2(creditTeamLogo.anchoredPosition.x, -5040);
            }
            else if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.GetLocale("ko"))
            {
                creditTitleImage.sprite = creditTitleKorean;
                creditScrollContent.sizeDelta = new Vector2(creditScrollContent.sizeDelta.x, 4920);
                creditDescription.sizeDelta = new Vector2(creditDescription.sizeDelta.x, 3960);
                creditTeamLogo.anchoredPosition = new Vector2(creditTeamLogo.anchoredPosition.x, -4200);
            }
        }
        else
        {
            if (quitButton.interactable && (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Return)))
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
        backSettingButton.interactable = true;
    }

    void StopSoundEffect()
    {
        StopCoroutine(SoundEffect());
    }

    IEnumerator OffCreditAnimation()
    {
        panelAnimation.SetTrigger("BackSettingDown");
        yield return new WaitForSeconds(1.5f);

        quitButton.interactable = true;
        creditButton.interactable = true;
        creditPanel.SetActive(false);
    }

    public void BackMainButtonDown()
    {
        GameManager.gm.SaveSettingsValue();
        GameManager.gm.LoadMain();
    }

    public void InSettingBackButtonDown()
    {
        backSettingButton.interactable = false;
        StartCoroutine(OffCreditAnimation());
        isCredit = false;
    }

    public void CreditButtonDown()
    {
        creditButton.interactable = false;
        quitButton.interactable = false;
        backSettingButton.interactable = false;

        isCredit = true;
        creditPanel.SetActive(true);
        StartCoroutine(SoundEffect());
    }

    public void ChangeLanguage()
    {
        //Language selected = (Language)System.Enum.Parse(typeof(Language), languageSetting.options[languageSetting.value].text);
        //Debug.Log(selected);

        switch (languageSetting.options[languageSetting.value].text)
        {
            case "English":
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
                break;
            case "한국어":
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
