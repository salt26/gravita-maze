using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialGuide : MonoBehaviour
{
    public enum Pivot { TopRight = 0, BottomRight = 1, TopLeft = 2 }
    public GameObject tutorialTip;
    public string emergencyText;
    public float tooltipWidth;
    public float tooltipHeight;
    public Pivot pivot;

    RectTransform myTransform;
    TutorialGuideUI CurrentTip;
    MapManager mm;
    PlayManager pm;
    
    int ballCount;
    int passCount;

    int storedI;
    int nowI;

    public Dictionary<TutorialTuple, string> tipDict = new Dictionary<TutorialTuple, string>();
    public List<TutorialTuple> tipKeys;

    public GameObject[] tips;

    void Awake()
    {
        
        CurrentTip = null; // 가장 처음 나와애 할 것을 나오게 하는 방법 찾기

        pm = GameObject.FindGameObjectWithTag("PlayManager").GetComponent<PlayManager>();

        mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
        
        myTransform = GetComponent<RectTransform>();

        //You can manipulate the direction of gravity using the arrow buttons.

        tipDict.Add(new TutorialTuple(1, 2, 0), "Press the left arrow to make the ball roll to the left!");
        tipDict.Add(new TutorialTuple(1, 0, 0), "Great job!\nNow, let's press the up arrow to make the ball roll up");
        tipDict.Add(new TutorialTuple(1, 0, 1), "Okay! Then let's get the ball out!");
        tipDict.Add(new TutorialTuple(2, 2, 0), "Next is a more complicated maze...\nGood luck!");
        tipDict.Add(new TutorialTuple(2, 0, 0), "Oh, that's a good choice");
        tipDict.Add(new TutorialTuple(2, 0, 2), "Oh, that's a good choice");
        tipDict.Add(new TutorialTuple(2, 1, 2), "We're almost there...");
        tipDict.Add(new TutorialTuple(2, 1, 1), "Here's only one step left!");
        tipDict.Add(new TutorialTuple(2, 0, 1), "Here's only one step left!");
        tipDict.Add(new TutorialTuple(3, 1, 0), "Fire can burn the ball...");
        tipDict.Add(new TutorialTuple(4, 2, 0), "A heavy iron can crush a ball...");
        tipDict.Add(new TutorialTuple(4, 1, 0), "The iron can be removed by escaping outside!");
        tipDict.Add(new TutorialTuple(4, 2, 2), "The iron can be removed by escaping outside!");
        tipDict.Add(new TutorialTuple(5, 1, 1), "A heavy iron can crush a ball.");
        tipDict.Add(new TutorialTuple(5, 2, 1), "The iron can be removed by escaping outside!");
        tipDict.Add(new TutorialTuple(5, 2, 2), "The iron can be removed by escaping outside!");
        tipDict.Add(new TutorialTuple(5, 2, 0), "Now, you can just escape with the iron.");
        tipDict.Add(new TutorialTuple(6, 2, 2), "The iron can cover the fire for a while!");
        tipDict.Add(new TutorialTuple(6, 2, 0), "Now that the iron has blocked the fire, you can step on it!");
        tipDict.Add(new TutorialTuple(7, 0, 2), "Press the right arrow to close the green shutter.");
        tipDict.Add(new TutorialTuple(7, 0, 0), "Let's use the shutter!");
        tipDict.Add(new TutorialTuple(7, 1, 2), "If the ball passes through the green shutter, it will become a wall.");
        tipDict.Add(new TutorialTuple(7, 1, 0), "After the shutter becomes a wall, it continues to exist as a wall.");
        tipDict.Add(new TutorialTuple(8, 1, 1), "A walled shutter can also block iron. Don't let iron crush the ball!");
        tipDict.Add(new TutorialTuple(8, 4, 0), "Of course, sometimes you can be trapped by the shutter, too. Press retry button!");
        tipDict.Add(new TutorialTuple(8, 2, 1), "The iron cannot activate the shutter. Only the ball can activate the shutter.");
        

        tipKeys = new List<TutorialTuple>(tipDict.Keys);
        
    }

    void Start(){
        // Dict의 각 원소에 t/f 값 줘서 튜토리얼 시작할 때마다 초기화 시키도록 하기 (한번 나온 거 안나오게 하는 기믹 초기화용)
    }


    public bool IsBallThere(TutorialTuple tutorialTuple)
    {
        int posX = tutorialTuple.xIndex;
        int posY = tutorialTuple.yIndex;

        if (mm != null && mm.currentMovableCoord[posX, posY] is Ball){
            return true;
        }
        else{
            return false;
        }
    }

    public bool IsIronThere(TutorialTuple tutorialTuple)
    {
        int posX = tutorialTuple.xIndex;
        int posY = tutorialTuple.yIndex;
        if (mm != null && mm.currentMovableCoord[posX, posY] is Iron){
            return true;
        }
        else{
            return false;
        }
    }
    

    public void ShowText(string text){
        CurrentTip = Instantiate(tutorialTip, myTransform).GetComponent<TutorialGuideUI>();

        CurrentTip.Initialize(new Vector2(0f,0f), 1080, 400, text);

    }

    public void HideText(TutorialGuideUI currentTip){
        Destroy(currentTip.gameObject);
        CurrentTip = null;
    }

    public void SpecificCaseGuide(MapManager.Flag flag){
        switch(flag){
            case MapManager.Flag.Burned:
                emergencyText = "The ball burned down!\nPress the shiny retry button to try again.";
                if(CurrentTip != null){
                    HideText(CurrentTip);
                }
                ShowText(emergencyText);

                break;

            case MapManager.Flag.Squashed:
                emergencyText = "The ball is crushed by the iron!\nPress the shiny retry button to try again.";
                if(CurrentTip != null){
                    HideText(CurrentTip);
                }
                ShowText(emergencyText);
                break;

            case MapManager.Flag.TimeOver:
                emergencyText = "Unfortunately, all time have passed!\nPress the shiny retry button to try again.";
                if(CurrentTip != null){
                    HideText(CurrentTip);
                }
                ShowText(emergencyText);
                
                CurrentTip.changeColor();
                break;

            case MapManager.Flag.Escaped:
                emergencyText = "The ball escaped! Congratulation!";
                if(GameManager.gm.PlayingMapIndex+1 == 8){
                    emergencyText = "Congratulations!\nYou passed all tutorials!";
                }
                if(CurrentTip != null){
                    HideText(CurrentTip);
                }
                ShowText(emergencyText);
                break;
                
            default:
                break;
        }
    }

    public void RetryButtonDown(){
        Debug.Log("BP");
        Destroy(CurrentTip);
        for(int j=0;j<tipKeys.Count;j++){
            tipKeys[j].isPassed = false;
        }
    }



    void Update(){
        if(mm == null || !mm.IsReady){
            return;
        }
        for(int i=0;i < tipKeys.Count;i++){
            int mapNumber = tipKeys[i].tutorialNumber;
            if(GameManager.gm.PlayingMapIndex+1 == mapNumber){
                if(GameManager.gm.PlayingMapIndex+1 == 6){

                    if(IsIronThere(tipKeys[i])){
                        
                        storedI = i;
                        if(!tipKeys[i].isPassed){
                            tips = GameObject.FindGameObjectsWithTag("Tip");
                            if(CurrentTip != null){
                                // tipskeys[]
                                // tips[0].SetActive(fasle);
                                Destroy(tips[0]);
                            }

                            if(tips.Length == 0){
                                ShowText(tipDict[tipKeys[i]]);
                                tipKeys[i].isPassed = true;
                            }
                            

                            nowI=i;
                        }
                    }
                    
                    if(storedI != nowI){
                        tipKeys[storedI].isPassed = false;
                    }   
                    // 이전의 것 다시 안 나오게 하기
                    // 이상한 길로 갔을 때 다른 말 나오개 하기
                }

                else{

                    if(IsBallThere(tipKeys[i])){
                        
                        storedI = i;
                        if(!tipKeys[i].isPassed){
                            tips = GameObject.FindGameObjectsWithTag("Tip");
                            if(CurrentTip != null){
                                // tipskeys[]
                                // tips[0].SetActive(fasle);
                                Destroy(tips[0]);
                            }

                            if(tips.Length == 0){
                                ShowText(tipDict[tipKeys[i]]);
                                tipKeys[i].isPassed = true;
                            }

                            nowI=i;
                        }
                    }
                    
                    if(storedI != nowI){
                        tipKeys[storedI].isPassed = false;
                    }
                }
            }

        }
    }
    
}

