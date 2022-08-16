using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialGuide : MonoBehaviour
{
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

        tipDict.Add(new TutorialTuple(1, 2, 0), "Press the left arrow to make the ball roll to the left!");
        tipDict.Add(new TutorialTuple(1, 0, 0), "Great job!\nNow, let's press the up arrow to make the ball roll up.");
        tipDict.Add(new TutorialTuple(1, 0, 1), "Okay! Then let's get the ball out!");
        tipDict.Add(new TutorialTuple(2, 2, 0), "Next is a more complicated maze...\nGood luck!");
        tipDict.Add(new TutorialTuple(2, 0, 0), "Oh, that's a good choice!");
        tipDict.Add(new TutorialTuple(2, 0, 2), "Oh, that's a good choice!");
        tipDict.Add(new TutorialTuple(2, 1, 2), "We're almost there...");
        tipDict.Add(new TutorialTuple(2, 1, 1), "Here's only one step left!");
        tipDict.Add(new TutorialTuple(2, 0, 1), "Here's only one step left!");
        tipDict.Add(new TutorialTuple(3, 1, 0), "Fire can burn the ball...");
        tipDict.Add(new TutorialTuple(4, 2, 0), "A heavy iron can squash a ball...");
        tipDict.Add(new TutorialTuple(4, 1, 0), "The iron can be removed by escaping outside!");
        tipDict.Add(new TutorialTuple(4, 2, 2), "The iron can be removed by escaping outside!");
        tipDict.Add(new TutorialTuple(5, 1, 1), "A heavy iron can squash a ball...");
        tipDict.Add(new TutorialTuple(5, 2, 1), "Now press the down arrow! The ball is safe from the iron.");
        tipDict.Add(new TutorialTuple(5, 2, 2), "Now press the down arrow! The ball is safe from the iron.");
        tipDict.Add(new TutorialTuple(5, 2, 0), "Now, you can just escape with the iron.");
        tipDict.Add(new TutorialTuple(6, 2, 2), "The iron can temporarily cover a fire!");
        tipDict.Add(new TutorialTuple(6, 2, 0), "Now that the iron has blocked the fire, you can step on it!");
        tipDict.Add(new TutorialTuple(7, 0, 2), "Press the right arrow to close the bright green shutter.");
        tipDict.Add(new TutorialTuple(7, 0, 0), "Let's use the shutter!");
        tipDict.Add(new TutorialTuple(7, 1, 2), "If the ball passes through the bright green shutter, it will become a wall.");
        tipDict.Add(new TutorialTuple(7, 1, 0), "Once the shutter becomes a wall, it remains a wall until you retry.");
        tipDict.Add(new TutorialTuple(8, 1, 1), "A walled shutter can also block the iron. Don't let the iron squash the ball!");
        tipDict.Add(new TutorialTuple(8, 4, 0), "Of course, sometimes you can be trapped by the shutter. Press retry button!");
        tipDict.Add(new TutorialTuple(8, 2, 1), "The iron cannot make the shutter a wall. Only the ball can close the shutter.");
        
        tipKeys = new List<TutorialTuple>(tipDict.Keys);
        
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

    public void SpecificCaseGuide(MapManager.Flag flag)
    {
        switch (flag)
        {
            case MapManager.Flag.Burned:
                emergencyText = "The ball burned down!\nPress the shiny retry button to try again.";
                if (currentTip != null)
                {
                    HideText(currentTip);
                }
                ShowText(emergencyText);

                break;

            case MapManager.Flag.Squashed:
                emergencyText = "The ball is squashed by the iron! Press the shiny retry button to try again.";
                if (currentTip != null)
                {
                    HideText(currentTip);
                }
                ShowText(emergencyText);
                break;

            case MapManager.Flag.TimeOver:
                emergencyText = "Unfortunately, all the time given has passed!\nPress the shiny retry button to try again.";
                if (currentTip != null)
                {
                    HideText(currentTip);
                }
                ShowText(emergencyText);

                currentTip.changeColor();
                break;

            case MapManager.Flag.Escaped:
                emergencyText = "The ball escaped! Congratulations!";
                if (GameManager.gm.PlayingMapIndex + 1 == 8)
                {
                    emergencyText = "Congratulations!\nYou passed all tutorials!";
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
        Debug.Log("BP");
        HideText(currentTip);
        for (int j = 0; j < tipKeys.Count; j++)
        {
            tipKeys[j].isPassed = false;
        }
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
                                // tipskeys[]
                                // tips[0].SetActive(fasle);
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
                    // 이상한 길로 갔을 때 다른 말 나오개 하기
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
                                // tipskeys[]
                                // tips[0].SetActive(fasle);
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

