using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    public Button settingCreditButton;
    public Button settingExitButton;
    public Slider settingBgmVolume;
    public Slider settingSfxVolume;

    public Dropdown dropdown;
    public GameObject fillImage;

    public bool m_IsButtonDowning;
    public Text option;
    private bool changed;

    public float bgmVolume;
    public float sfxVolume;
    // Start is called before the first frame update
    void Start()
    {
        bgmVolume = GameManager.gm.bgmVolume;
        sfxVolume = GameManager.gm.sfxVolume;

        transform.GetChild(1).transform.GetChild(2).GetComponent<Slider>().value = bgmVolume;
        transform.GetChild(1).transform.GetChild(4).GetComponent<Slider>().value = sfxVolume;
        changed = false;

        if (dropdown.options.Count > 2)
        {
            fillImage.SetActive(false);
        }
        else
        {
            fillImage.SetActive(true);
        }
    }

    void Update()
    {
        if (m_IsButtonDowning && !changed)
        {
            Vector3 temp = option.transform.position;
            temp.y -= 24;
            option.transform.position = temp;
            changed = true;
        }
        if (!m_IsButtonDowning && changed) {

            Vector3 temp = option.transform.position;
            temp.y += 24;
            option.transform.position = temp;
            changed = false;
        }
    }

    public void PointerDown()
    {
        m_IsButtonDowning = true;
    }

    public void PointerUp()
    {
        m_IsButtonDowning = false;
    }


}
