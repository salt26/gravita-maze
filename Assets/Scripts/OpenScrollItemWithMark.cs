using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenScrollItemWithMark : OpenSaveScrollItem
{
    public Sprite okMark;
    public Sprite xMark;
    public Image markIcon;

    public bool isClearedInTime;
    public bool isClearedInMove;

    public void Initialize(Type type, string path, bool isFolder, bool isClearedInTime, bool isClearedInMove, PlayManager pm, bool isUpOneLevel = false)
    {
        this.type = type;
        this.pm = pm;
        this.isFolder = isFolder;
        this.isUpOneLevel = isFolder & isUpOneLevel;
        this.path = path.Replace('\\', '/');
        this.isClearedInTime = isClearedInTime;
        this.isClearedInMove = isClearedInMove;
        if (isFolder)
        {
            labelName = this.path.Substring(this.path.LastIndexOf('/') + 1);
        }
        else
        {
            labelName = Path.GetFileNameWithoutExtension(this.path);
        }
        Update();
    }

    public void Initialize(string name, bool isFolder, bool isClearedInTime, bool isClearedInMove, PlayManager pm, TextAsset textAsset = null, bool isUpOneLevel = false)
    {
        this.type = Type.TrainingOpen;
        this.pm = pm;
        this.isFolder = isFolder;
        this.isUpOneLevel = isFolder & isUpOneLevel;
        this.path = name;
        this.isClearedInTime = isClearedInTime;
        this.isClearedInMove = isClearedInMove;
        labelName = name;
        this.textAsset = textAsset;
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
                label.color = highlightedColor;
            }
            else
            {
                icon.sprite = highlightedFileIcon;
                label.color = highlightedColor;
            }
        }

        if (isFolder)
        {
            markIcon.gameObject.SetActive(false);
        }
        else if (GameManager.mm.LimitMode == MapManager.LimitModeEnum.Time)
        {
            if (isClearedInTime)
            {
                markIcon.gameObject.SetActive(true);
                markIcon.sprite = okMark;
            }
            else
            {
                markIcon.gameObject.SetActive(true);
                markIcon.sprite = xMark;
            }
        }
        else
        {
            if (isClearedInMove)
            {
                markIcon.gameObject.SetActive(true);
                markIcon.sprite = okMark;
            }
            else
            {
                markIcon.gameObject.SetActive(true);
                markIcon.sprite = xMark;
            }
        }
    }

}
