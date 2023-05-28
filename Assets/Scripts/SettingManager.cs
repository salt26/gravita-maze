using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)||Input.GetKeyUp(KeyCode.Return))
        {
            GameManager.gm.PlayButtonSFX();
            GameManager.gm.LoadMain();            
        }
    }
    public Dropdown languageSetting;
    public void BackMainButtonDown()
    {
        GameManager.gm.LoadMain();
    }
    public void CreditButtonDown()
    {
        GameManager.gm.LoadCredit() ;
    }
    public void ChangeLanguage()
    {
        GameManager.Language selected = (GameManager.Language)System.Enum.Parse(typeof(GameManager.Language), languageSetting.options[languageSetting.value].text);
        Debug.Log(selected);
    }
    public void PlayButtonSFX()
    {
        GameManager.gm.PlayButtonSFX();
    }

}
