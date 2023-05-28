using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Return))
        {
            GameManager.gm.PlayButtonSFX();
            GameManager.gm.LoadSetting();
        }
    }
    public void BackMainButtonDown(){
        GameManager.gm.LoadSetting();
    }
}
