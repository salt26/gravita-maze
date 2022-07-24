using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIPanel : MonoBehaviour
{
    public Sprite panel1920x1080;
    public Sprite panel2160x1080;
    public Sprite panel2960x1440;

    // Update is called once per frame
    void Update()
    {
        if (Screen.width / 9 * 16 == Screen.height)
        {
            GetComponent<Image>().sprite = panel1920x1080;
        }
        else if (Screen.width * 2 == Screen.height)
        {
            GetComponent<Image>().sprite = panel2160x1080;
        }
        else if (Screen.width == 1440 && Screen.height == 2960)
        {
            GetComponent<Image>().sprite = panel2960x1440;
        }
        else
        {
            GetComponent<Image>().sprite = panel1920x1080;
        }

        if (SceneManager.GetActiveScene().name.Equals("Editor"))
        {
            GameObject.Find("EditorManager").GetComponent<EditorManager>().statusUI.SetStatusMessage(Screen.width + " * " + Screen.height);
        }
    }
}
