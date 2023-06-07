﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class OpenSaveScrollItem : MonoBehaviour
{
    public enum Type { Open = 0, Save = 1, TrainingOpen = 2 };
    public Type type;

    public Image icon;
    public Text label;

    public static Color basicColor = Color.black;
    public static Color highlightedColor = new Color(191f / 255f, 0f, 165f / 255f);
    public Sprite basicFileIcon;
    public Sprite highlightedFileIcon;
    public Sprite basicFolderIcon;
    public Sprite highlightedFolderIcon;

    public EditorManager em;
    public PlayManager pm;

    public bool isFolder = false;
    public bool isUpOneLevel = false;
    public string path = "";
    public string labelName = "";
    public TextAsset textAsset = null;

    public bool isSelected = false;

    public void Initialize(Type type, string path, bool isFolder, EditorManager em, bool isUpOneLevel = false)
    {
        this.type = type;
        this.em = em;
        this.isFolder = isFolder;
        this.isUpOneLevel = isFolder & isUpOneLevel;
        this.path = path.Replace('\\', '/');
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

    public void Initialize(Type type, string path, bool isFolder, PlayManager pm, bool isUpOneLevel = false)
    {
        this.type = type;
        this.pm = pm;
        this.isFolder = isFolder;
        this.isUpOneLevel = isFolder & isUpOneLevel;
        this.path = path.Replace('\\', '/');
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

    public void Initialize(string name, bool isFolder, PlayManager pm, TextAsset textAsset = null, bool isUpOneLevel = false)
    {
        this.type = Type.TrainingOpen;
        this.pm = pm;
        this.isFolder = isFolder;
        this.isUpOneLevel = isFolder & isUpOneLevel;
        this.path = name;
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
    }

    public void Select()
    {
        if (em != null)
        {
            switch (type)
            {
                case Type.Open:
                    em.EditOpenItemSelect(this);
                    break;
                case Type.Save:
                    em.EditSaveItemSelect(this);
                    break;
            }
        }
        else if (pm != null)
        {
            pm.OpenItemSelect((OpenScrollItemWithMark)this);
        }
    }
}
