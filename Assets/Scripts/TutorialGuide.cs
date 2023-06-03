using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class TutorialGuide : MonoBehaviour
{
    public string tableName = "StringTable";

    public GameObject tutorialTip;
    public string emergencyText;
    public float tooltipWidth;
    public float tooltipHeight;

    RectTransform myTransform;
    TutorialGuideUI currentTip;
    MapManager mm;
    //PlayManager pm;

    int storedI;
    int nowI;

    bool hasIronRemovedInTutorial5;
    bool isBallIn12AndIronIn02InTutorial9;
    bool hasShowed10TextInTutorial9;

    public Dictionary<TutorialTuple, string> tipDict = new Dictionary<TutorialTuple, string>();
    public List<TutorialTuple> tipKeys;

    public GameObject[] tips;

    void Awake()
    {
        
        currentTip = null; // 가장 처음 나와야 할 것을 나오게 하는 방법 찾기

        //pm = GameObject.FindGameObjectWithTag("PlayManager").GetComponent<PlayManager>();

        mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
        
        myTransform = GetComponent<RectTransform>();

        // You can manipulate the direction of gravity using the arrow buttons.

        tipDict.Add(new TutorialTuple(1, 2, 0), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_01"));
        tipDict.Add(new TutorialTuple(1, 0, 0), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_02"));
        tipDict.Add(new TutorialTuple(1, 0, 1), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_03"));
        tipDict.Add(new TutorialTuple(2, 2, 0), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_04"));
        tipDict.Add(new TutorialTuple(2, 0, 0), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_05"));
        tipDict.Add(new TutorialTuple(2, 0, 2), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_05"));
        tipDict.Add(new TutorialTuple(2, 1, 2), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_06"));
        tipDict.Add(new TutorialTuple(2, 1, 1), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_07"));
        tipDict.Add(new TutorialTuple(2, 0, 1), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_07"));
        tipDict.Add(new TutorialTuple(3, 1, 0), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_08"));
        tipDict.Add(new TutorialTuple(4, 2, 1), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_09"));
        tipDict.Add(new TutorialTuple(4, 2, 0), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_10"));
        tipDict.Add(new TutorialTuple(5, 2, 0), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_11"));
        tipDict.Add(new TutorialTuple(6, 2, 2), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_12"));
        tipDict.Add(new TutorialTuple(6, 2, 0), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_13"));
        tipDict.Add(new TutorialTuple(7, 1, 2), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_14"));
        tipDict.Add(new TutorialTuple(7, 1, 0), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_15"));
        tipDict.Add(new TutorialTuple(7, 2, 2), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_16"));
        tipDict.Add(new TutorialTuple(7, 2, 0), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_17"));
        tipDict.Add(new TutorialTuple(7, 0, 1), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_18"));
        tipDict.Add(new TutorialTuple(8, 1, 1), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_19"));
        tipDict.Add(new TutorialTuple(8, 3, 1), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_20"));
        tipDict.Add(new TutorialTuple(8, 0, 3), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_21"));
        tipDict.Add(new TutorialTuple(9, 2, 1), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_29"));
        tipDict.Add(new TutorialTuple(9, 3, 1), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_30"));
        tipDict.Add(new TutorialTuple(9, 2, 2), LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_31"));

        tipKeys = new List<TutorialTuple>(tipDict.Keys);

        hasIronRemovedInTutorial5 = false;
        isBallIn12AndIronIn02InTutorial9 = true;
        hasShowed10TextInTutorial9 = false;
    }

    public bool IsBallThere(TutorialTuple tutorialTuple)
    {
        int posX = tutorialTuple.xIndex;
        int posY = tutorialTuple.yIndex;

        if (mm != null && mm.currentMovableCoord[posX, posY] is Ball)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsIronThere(TutorialTuple tutorialTuple)
    {
        int posX = tutorialTuple.xIndex;
        int posY = tutorialTuple.yIndex;
        if (mm != null && mm.currentMovableCoord[posX, posY] is Iron)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public void ShowText(string text)
    {
        currentTip = Instantiate(tutorialTip, myTransform).GetComponent<TutorialGuideUI>();

        currentTip.Initialize(new Vector2(0f, 0f), 1080, 400, text);
    }

    public void HideText(TutorialGuideUI currentTip)
    {
        Destroy(currentTip.gameObject);
        this.currentTip = null;
    }

     public bool BallInMap4()
    {
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 2; y++)
                if (mm != null && mm.currentMovableCoord[x, y] is Ball)
                {
                    int[] coord = { x, y };
                    return true;
                };
        return false;
    }

    public void SpecificCaseGuide(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Burned:
                if (GameManager.gm.PlayingMapIndex + 1 == 9)
                {
                    emergencyText = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_35");
                }
                else
                {
                    emergencyText = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_22");
                }

                if (currentTip != null)
                {
                    HideText(currentTip);
                }
                ShowText(emergencyText);

                break;

            case MapManager.Flag.Squashed:
                if (GameManager.gm.PlayingMapIndex + 1 == 9)
                {
                    emergencyText = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_36");
                }
                else
                {
                    emergencyText = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_23");
                }

                if (currentTip != null)
                {
                    HideText(currentTip);
                }
                ShowText(emergencyText);
                break;

            case MapManager.Flag.TimeOver:
                if (GameManager.gm.PlayingMapIndex + 1 == 9)
                {
                    emergencyText = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_37");
                }
                else
                {
                    emergencyText = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_24");
                }

                if (currentTip != null)
                {
                    HideText(currentTip);
                }
                ShowText(emergencyText);

                currentTip.changeColor();
                break;

            case MapManager.Flag.Escaped:

                if (GameManager.gm.PlayingMapIndex + 1 == 1)
                {
                    emergencyText = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_28");
                }
                else if (GameManager.gm.PlayingMapIndex + 1 == 9)
                {
                    emergencyText = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_26");
                }
                else
                {
                    emergencyText = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_25");
                }

                if (currentTip != null)
                {
                    HideText(currentTip);
                }
                ShowText(emergencyText);
                break;
            default:
                break;
        }
    }

    public void RetryButtonDown()
    {
        //Debug.Log("BP");
        HideText(currentTip);
        for (int j = 0; j < tipKeys.Count; j++)
        {
            tipKeys[j].isPassed = false;
        }
        hasIronRemovedInTutorial5 = false;
        isBallIn12AndIronIn02InTutorial9 = true;
        hasShowed10TextInTutorial9 = false;
    }



    void Update()
    {
        if (mm == null || !mm.IsReady)
        {
            return;
        }
        for (int i = 0; i < tipKeys.Count; i++)
        {
            int mapNumber = tipKeys[i].tutorialNumber;
            if (GameManager.gm.PlayingMapIndex + 1 == mapNumber)
            {
                if (GameManager.gm.PlayingMapIndex + 1 == 6)
                {
                    if (IsIronThere(tipKeys[i]))
                    {
                        storedI = i;
                        if (!tipKeys[i].isPassed)
                        {
                            tips = GameObject.FindGameObjectsWithTag("Tip");
                            if (currentTip != null)
                            {
                                Destroy(tips[0]);
                            }

                            if (tips.Length == 0)
                            {
                                ShowText(tipDict[tipKeys[i]]);
                                tipKeys[i].isPassed = true;
                            }


                            nowI = i;
                        }
                    }

                    if (storedI != nowI)
                    {
                        tipKeys[storedI].isPassed = false;
                    }
                    // 이전의 것 다시 안 나오게 하기
                    // 이상한 길로 갔을 때 다른 말 나오게 하기
                }
                else if (GameManager.gm.PlayingMapIndex + 1 == 4)
                {
                    //Debug.Log(BallInMap4().ToString());
                    if (IsIronThere(tipKeys[i]) && BallInMap4())
                    {
                        storedI = i;
                        if (!tipKeys[i].isPassed)
                        {
                            tips = GameObject.FindGameObjectsWithTag("Tip");
                            if (currentTip != null)
                            {
                                Destroy(tips[0]);
                            }

                            if (tips.Length == 0)
                            {
                                ShowText(tipDict[tipKeys[i]]);
                                tipKeys[i].isPassed = true;
                            }


                            nowI = i;
                        }
                    }

                    if (storedI != nowI)
                    {
                        tipKeys[storedI].isPassed = false;
                    }
                }
                else if (GameManager.gm.PlayingMapIndex + 1 == 5)
                {
                    bool hasIronExists = false;
                    foreach (Movable m in mm.currentMovableCoord)
                    {
                        if (m != null && m is Iron)
                        {
                            hasIronExists = true;
                            break;
                        }
                    }

                    if (!hasIronRemovedInTutorial5 && !hasIronExists)
                    {
                        if (currentTip != null)
                        {
                            HideText(currentTip);
                        }

                        if (currentTip == null)
                        {
                            ShowText(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_27"));
                        }
                        hasIronRemovedInTutorial5 = true;
                    }

                    if (hasIronExists && IsBallThere(tipKeys[i]))
                    {
                        storedI = i;
                        if (!tipKeys[i].isPassed)
                        {
                            tips = GameObject.FindGameObjectsWithTag("Tip");
                            if (currentTip != null)
                            {
                                Destroy(tips[0]);
                            }

                            if (tips.Length == 0)
                            {
                                ShowText(tipDict[tipKeys[i]]);
                                tipKeys[i].isPassed = true;
                            }

                            nowI = i;
                        }
                    }

                    if (storedI != nowI)
                    {
                        tipKeys[storedI].isPassed = false;
                    }
                }
                else if (GameManager.gm.PlayingMapIndex + 1 == 9)
                {
                    bool hasShowed = false;
                    if (mm.currentMovableCoord[1, 0] is Ball && mm.currentMovableCoord[0, 0] is Iron &&
                        !hasShowed10TextInTutorial9)
                    {
                        tips = GameObject.FindGameObjectsWithTag("Tip");
                        if (currentTip != null)
                        {
                            Destroy(tips[0]);
                        }

                        if (tips.Length == 0)
                        {
                            ShowText(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_32"));
                            hasShowed10TextInTutorial9 = true;
                            isBallIn12AndIronIn02InTutorial9 = false;
                        }
                        hasShowed = true;
                    }
                    else if (mm.currentMovableCoord[1, 0] is Ball && mm.currentMovableCoord[0, 0] is null &&
                        !hasShowed10TextInTutorial9)
                    {
                        tips = GameObject.FindGameObjectsWithTag("Tip");
                        if (currentTip != null)
                        {
                            Destroy(tips[0]);
                        }

                        if (tips.Length == 0)
                        {
                            ShowText(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_33"));
                            hasShowed10TextInTutorial9 = true;
                            isBallIn12AndIronIn02InTutorial9 = false;
                        }
                        hasShowed = true;
                    }
                    
                    if (mm.currentMovableCoord[1, 2] is Ball && mm.currentMovableCoord[0, 2] is Iron &&
                        !isBallIn12AndIronIn02InTutorial9)
                    {
                        tips = GameObject.FindGameObjectsWithTag("Tip");
                        if (currentTip != null)
                        {
                            Destroy(tips[0]);
                        }

                        if (tips.Length == 0)
                        {
                            ShowText(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_34"));
                            isBallIn12AndIronIn02InTutorial9 = true;
                            hasShowed10TextInTutorial9 = false;
                        }
                        hasShowed = true;
                    }
                    else if (mm.currentMovableCoord[1, 2] is Ball && mm.currentMovableCoord[0, 2] is null &&
                        isBallIn12AndIronIn02InTutorial9)
                    {
                        tips = GameObject.FindGameObjectsWithTag("Tip");
                        if (currentTip != null)
                        {
                            Destroy(tips[0]);
                        }

                        if (tips.Length == 0)
                        {
                            ShowText(LocalizationSettings.StringDatabase.GetLocalizedString(tableName, "tutorial_message_31"));
                            isBallIn12AndIronIn02InTutorial9 = false;
                            hasShowed10TextInTutorial9 = false;
                        }
                        hasShowed = true;
                    }
                    
                    if (!hasShowed && IsBallThere(tipKeys[i]))
                    {
                        storedI = i;
                        if (!tipKeys[i].isPassed)
                        {
                            tips = GameObject.FindGameObjectsWithTag("Tip");
                            if (currentTip != null)
                            {
                                Destroy(tips[0]);
                            }

                            if (tips.Length == 0)
                            {
                                ShowText(tipDict[tipKeys[i]]);
                                tipKeys[i].isPassed = true;
                            }

                            nowI = i;
                        }
                    }

                    if (storedI != nowI)
                    {
                        tipKeys[storedI].isPassed = false;
                    }
                }
                else
                {
                    if (IsBallThere(tipKeys[i]))
                    {

                        storedI = i;
                        if (!tipKeys[i].isPassed)
                        {
                            tips = GameObject.FindGameObjectsWithTag("Tip");
                            if (currentTip != null)
                            {
                                Destroy(tips[0]);
                            }

                            if (tips.Length == 0)
                            {
                                ShowText(tipDict[tipKeys[i]]);
                                tipKeys[i].isPassed = true;
                            }

                            nowI = i;
                        }
                    }

                    if (storedI != nowI)
                    {
                        tipKeys[storedI].isPassed = false;
                    }
                }
            }

        }
    }
    
}

