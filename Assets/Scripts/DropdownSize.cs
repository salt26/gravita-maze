using UnityEngine;
using UnityEngine.UI;   // UI와 관련된 스크립트 작업을 위해서 추가해 주어야 한다.
using System.Collections;

public class DropdownSize : MonoBehaviour
{
    public Dropdown dropdown;
    public GameObject fillImage;
    
    void Start()
    {
        if(dropdown.options.Count > 2)
        {
            fillImage.SetActive(false);
        }
        else
        {
            fillImage.SetActive(true);
        }

    }

}
