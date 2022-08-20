using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TutorialManager : MonoBehaviour
{

    public void SkipTutorial()
    {
        DoTutorialSave();
        GameManager.gm.LoadMain();
    }
    public void PlayTutorial()
    {
        GameManager.gm.LoadTutorial();
    }
    public void PlayButtonSFX()
    {
        GameManager.gm.PlayButtonSFX();
    }
    public void DoTutorialSave()
    {
        var file = File.CreateText(Application.persistentDataPath + "/TutorialDone.txt");
        file.Close();
    }
}
