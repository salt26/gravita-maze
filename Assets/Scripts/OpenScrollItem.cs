using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

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

    public EditorManager em;

    public bool isFolder = false;
    public bool isUpOneLevel = false;
    public string path = "";
    public string labelName = "";

    public bool isSelected = false;

    public void Initialize(string path, bool isFolder, EditorManager em, bool isUpOneLevel = false)
    {
        this.em = em;
        this.isFolder = isFolder;
        this.isUpOneLevel = isFolder & isUpOneLevel;
        this.path = path;
        if (isFolder)
        {
            labelName = path.Substring(path.LastIndexOf('\\') + 1);
        }
        else
        {
            labelName = Path.GetFileNameWithoutExtension(path);
        }
        Update();
    }

    void Update()
    {
        if (isFolder)
        {
            if (isUpOneLevel)
                label.text = "[Up One Level]";
            else if (labelName.Length <= 24)
                label.text = labelName;
            else
                label.text = labelName.Substring(0, 21) + "...";
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

    public void Select()
    {
        em.EditOpenItemSelect(this);
    }
}
