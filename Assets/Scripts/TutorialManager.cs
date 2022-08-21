using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;

public class TutorialManager : MonoBehaviour
{

    public void SkipTutorial()
    {
        DoTutorialSave();
        GameManager.gm.LoadMain();
    }
    public void PlayTutorial()
    {
        DoTutorialSave();
        GameManager.gm.LoadTutorial();
    }
    public void PlayButtonSFX()
    {
        GameManager.gm.PlayButtonSFX();
    }
    public void DoTutorialSave()
    {
        if (File.Exists(Application.persistentDataPath + "/TutorialDone.txt"))
        {
            try
            {
                FileStream fs = new FileStream(Application.persistentDataPath + "/TutorialDone.txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.WriteLine("0");
                sw.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        
    }
}
