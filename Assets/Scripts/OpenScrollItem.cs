using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenScrollItem : MonoBehaviour
{
    public Image icon;
    public Text label;

    public Color basicColor;
    public Color highlightedColor;
    public Sprite basicFileIcon;
    public Sprite highlightedFileIcon;
    public Sprite basicFolderIcon;
    public Sprite highlightedFolderIcon;

    public bool isFolder = false;
    public string labelName = "";

    public bool isSelected = false;

    public void Initialize(string name, bool isFolder)
    {
        this.isFolder = isFolder;
        labelName = name;
        Update();
    }

    void Update()
    {
        if (isFolder)
        {
            if (labelName.Length <= 22)
                label.text = "[" + labelName + "]";
            else
                label.text = "[" + labelName.Substring(0, 19) + "...]";
        }
        else
        {
            if (labelName.Length <= 24)
                label.text = labelName;
            else
                label.text = labelName.Substring(0, 21) + "...";
        }

        if (!isSelected)
        {
            if (isFolder)
            {
                icon.sprite = basicFolderIcon;
            }
            else
            {
                icon.sprite = basicFileIcon;
            }
            label.color = basicColor;
        }
        else
        {
            if (isFolder)
            {
                icon.sprite = highlightedFolderIcon;
            }
            else
            {
                icon.sprite = highlightedFileIcon;
            }
            label.color = highlightedColor;
        }
    }
}
