using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
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

}
