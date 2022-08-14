using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialGuide : MonoBehaviour
{
    public enum Pivot { TopRight = 0, BottomRight = 1, TopLeft = 2 }

    public GameObject tooltipPrefab;
    // public GameObject tutorialTip;
    public string tooltipMessage;
    public string emergencyText;
    public float tooltipWidth;
    public float tooltipHeight;
    public Pivot pivot;

    RectTransform myTransform;
    TutorialGuideUI CurrentTip;
    MapManager mm;
    PlayManager pm;

    public Dictionary<TutorialTuple, string> tipDict = new Dictionary<TutorialTuple, string>();
    public int mapNum;
    public List<TutorialTuple> tipKeys;

    void Awake()
    {
        
        CurrentTip = null; // 가장 처음 나와애 할 것을 나오게 하는 방법 찾기

        pm = GameObject.FindGameObjectWithTag("PlayManager").GetComponent<PlayManager>();

        mm = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
        
        myTransform = GetComponent<RectTransform>();

        tipDict.Add(new TutorialTuple(1, 2, 0), "You can manipulate the direction of gravity using the arrow buttons. Press the left arrow to make the ball roll to the left!");
        tipDict.Add(new TutorialTuple(1, 0, 0), "Great job! Now, let's press the up arrow to make the ball roll up");
        tipDict.Add(new TutorialTuple(1, 0, 1), "Okay! Then let's get the ball out!");
        tipDict.Add(new TutorialTuple(2, 2, 0), "Next is a more complicated maze... Good luck!");
        tipDict.Add(new TutorialTuple(2, 0, 0), "Oh, that's a good choice");
        tipDict.Add(new TutorialTuple(2, 0, 2), "Oh, that's a good choice");
        tipDict.Add(new TutorialTuple(2, 1, 2), "We're almost there...");
        tipDict.Add(new TutorialTuple(2, 1, 1), "Here's only one step left!");
        tipDict.Add(new TutorialTuple(2, 0, 1), "Here's only one step left!");
        tipDict.Add(new TutorialTuple(3, 1, 2), "Fire can burn your balls...");
        tipDict.Add(new TutorialTuple(4, 2, 0), "A heavy Iron can crush a ball...");
        tipDict.Add(new TutorialTuple(4, 2, 1), "And also, the Iron can escape outside with the ball!");
        tipDict.Add(new TutorialTuple(4, 2, 2), "And also, the Iron can escape outside with the ball!");
        //tipDict.Add(new TutorialTuple(4, 2, 0), "Now, you can just escape with Iron.");
        tipDict.Add(new TutorialTuple(5, 1, 1), "Also, a heavy Iron can make it go out regardless of the ball!");
        tipDict.Add(new TutorialTuple(6, 2, 2), "Finally, the Iron can cover the fire for a while!");
        tipDict.Add(new TutorialTuple(6, 2, 0), "Now that the Iron has blocked the fire, you can step on it!");

        tipKeys = new List<TutorialTuple>(tipDict.Keys);
        
    }

    void Start(){
        mapNum = 1;
        // Dict의 각 원소에 t/f 값 줘서 튜토리얼 시작할 때마다 초기화 시키도록 하기 (한번 나온 거 안나오게 하는 기믹 초기화용)
    }


    public bool isBallThere(TutorialTuple tutorialTuple)
    {
        int posX = tutorialTuple.xIndex;
        int posY = tutorialTuple.yIndex;
        if (mm.currentMovableCoord[posX, posY] is Ball){
            return true;
        }
        else{
            return false;
        }
    }

    public bool isIronThere(TutorialTuple tutorialTuple)
    {
        int posX = tutorialTuple.xIndex;
        int posY = tutorialTuple.yIndex;
        if (mm.currentMovableCoord[posX, posY] is Iron){
            return true;
        }
        else{
            return false;
        }
    }
    

    public void TutorialCount(){
        mapNum++;
    }

    public void showText(string text){
        CurrentTip = Instantiate(tooltipPrefab).GetComponent<TutorialGuideUI>(); //Vector3 값 어케 하지
        switch (pivot)
        {
            case Pivot.TopRight:
                CurrentTip.Initialize(myTransform.localPosition + new Vector3(myTransform.rect.width / 2f, myTransform.rect.height / 2f - 24), // 실제 적용시 위치 봐가며 수정
                    tooltipWidth, tooltipHeight, (TutorialGuideUI.Pivot)pivot, text);
                break;
            case Pivot.BottomRight:
                CurrentTip.Initialize(myTransform.localPosition + new Vector3(myTransform.rect.width / 2f, -myTransform.rect.height / 2f + 12),
                    tooltipWidth, tooltipHeight, (TutorialGuideUI.Pivot)pivot, text);
                break;
            case Pivot.TopLeft:
                CurrentTip.Initialize(myTransform.localPosition + new Vector3(-myTransform.rect.width / 2f, myTransform.rect.height / 2f - 24),
                    tooltipWidth, tooltipHeight, (TutorialGuideUI.Pivot)pivot, text);
                break;
            }
    }

    public void hideText(){
        Destroy(CurrentTip.gameObject);
        CurrentTip = null;
    }


/*    void Update(){
        MapManager.Flag flag = mm.flag;
        switch(flag){
            case MapManager.Flag.Burned:
                emergencyText = "Your ball burned down! Press the shiny return button at the bottom to try again.";
                if(CurrentTip != null){
                    hideText();
                }
                showText(emergencyText);
                break;

            case MapManager.Flag.Squashed:
                emergencyText = "Your ball is crushed by the box! Press the shiny return button at the bottom to try again.";
                if(CurrentTip != null){
                    hideText();
                }
                showText(emergencyText);
                break;

            case MapManager.Flag.TimeOver:
                emergencyText = "Unfortunately, all time have passed! Press the shiny return button at the bottom to try again.";
                if(CurrentTip != null){
                    hideText();
                }
                showText(emergencyText);
                break;
                
            default:
                break;
        
        }

        // 죽으면 나오는 것들 출력하기 (3개)
        for(int i=0;i <= tipKeys.Count;i++){
            int mapNumber = tipKeys[i].tutorialNumber;
            if(mapNum == mapNumber){
                if(mapNum == 6){

                    if(isIronThere(tipKeys[i])){

                        if(!tipKeys[i].isPassed){
                            if(CurrentTip != null){
                                hideText();
                            }
                            showText(tipDict[tipKeys[i]]);
                        }
                    }
                    // 이전의 것 다시 안 나오게 하기
                    // 이상한 길로 갔을 때 다른 말 나오개 하기
                }

                else{

                    if(isBallThere(tipKeys[i])){
                        if(!tipKeys[i].isPassed){
                            if(CurrentTip != null){
                                hideText();
                            }
                            showText(tipDict[tipKeys[i]]);
                        }
                        
                    }
                }
            }

        }
    }
    */
}

